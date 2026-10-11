using System.Text.Json;

using Confluent.Kafka;

using EventManager.Contracts;
using EventManager.Contracts.Events;
using EventManager.EventService.Application.Abstractions.Persistence.Repositories;
using EventManager.EventService.Domain.Exceptions;
using EventManager.EventService.Infrastructure.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventManager.EventService.Infrastructure.BackgroundServices;

public class BookingConfirmationEventConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingConfirmationEventConsumer> _logger;
    private readonly KafkaSettings _kafkaSettings;

    public BookingConfirmationEventConsumer(IServiceScopeFactory scopeFactory, IOptions<KafkaSettings> kafkaSettings,
        ILogger<BookingConfirmationEventConsumer> logger)
    {
        _kafkaSettings = kafkaSettings.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    private IConsumer<string, string> CreateKafkaConsumer()
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = _kafkaSettings.ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(KafkaTopics.BookingConfirmedTopicName);

        return consumer;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(BookingConfirmationEventConsumer)} started.");

        using var consumer = CreateKafkaConsumer();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    var confirmation = JsonSerializer.Deserialize<BookingConfirmedEvent>(consumeResult.Message.Value);

                    await ProcessConfirmation(confirmation, stoppingToken);

                    consumer.Commit(consumeResult);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unhandled exception in {nameof(BookingConfirmationEventConsumer)}");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        finally
        {
            consumer.Close();
        }

        _logger.LogInformation($"{nameof(BookingConfirmationEventConsumer)} stopped.");
    }

    private async Task ProcessConfirmation(BookingConfirmedEvent? confirmation, CancellationToken ct)
    {
        if (confirmation == null)
        {
            _logger.LogError("Confirmation is null");
            return;
        }
        
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
            var eventToBook = await eventRepository.GetEventByIdAsync(confirmation.EventId, ct);

            if (eventToBook == null)
            {
                _logger.LogWarning("Event to book {EventId} not found", confirmation.EventId);
                return;
            }

            var reserveResult = eventToBook.TryReserveSeats(confirmation.SeatCount);

            if (!reserveResult)
            {
                _logger.LogWarning("No seats available for {EventId}", confirmation.EventId);
                return;
            }

            await eventRepository.SaveChangesAsync(ct);
        }
        catch (EventAlreadyStartedException e)
        {
            _logger.LogError(e, "Failed to reserve seats for {EventId}. Event already started", confirmation.EventId);
        }
        catch (ArgumentOutOfRangeException e)
        {
            _logger.LogError(e, "Failed to reserve seats for {EventId}. Wrong seat count {SeatCount}",
                confirmation.EventId, confirmation.SeatCount);
        }
    }
}