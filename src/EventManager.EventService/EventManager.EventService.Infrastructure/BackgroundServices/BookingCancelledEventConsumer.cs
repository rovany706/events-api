using System.Text.Json;

using Confluent.Kafka;

using EventManager.Contracts;
using EventManager.Contracts.Events;
using EventManager.EventService.Application.Abstractions.Persistence.Repositories;
using EventManager.EventService.Application.Common.Messaging;
using EventManager.EventService.Domain.Exceptions;
using EventManager.EventService.Infrastructure.Options;
using EventManager.EventService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

namespace EventManager.EventService.Infrastructure.BackgroundServices;

public class BookingCancelledEventConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingConfirmationEventConsumer> _logger;
    private readonly KafkaSettings _kafkaSettings;

    public BookingCancelledEventConsumer(IServiceScopeFactory scopeFactory, IOptions<KafkaSettings> kafkaSettings,
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
        consumer.Subscribe(KafkaTopics.BookingCancelledTopicName);

        return consumer;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(BookingCancelledEventConsumer)} started.");

        using var consumer = CreateKafkaConsumer();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    var message = JsonSerializer.Deserialize<BookingCancelledEvent>(consumeResult.Message.Value)!;

                    await ProcessMessage(message, stoppingToken);

                    consumer.Commit(consumeResult);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unhandled exception in {nameof(BookingCancelledEventConsumer)}");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        finally
        {
            consumer.Close();
        }

        _logger.LogInformation($"{nameof(BookingCancelledEventConsumer)} stopped.");
    }

    private async Task ProcessMessage(BookingCancelledEvent message, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<EventDbContext>();

        try
        {
            if (await context.InboxMessages.AnyAsync(x => x.Id == message.MessageId, ct))
            {
                return;
            }

            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
            var eventToCancel = await eventRepository.GetEventByIdAsync(message.EventId, ct);

            if (eventToCancel == null)
            {
                _logger.LogWarning("Event to cancel {EventId} not found", message.EventId);
                return;
            }

            eventToCancel.ReleaseSeats(message.SeatCount);
        }
        catch (ArgumentOutOfRangeException e)
        {
            _logger.LogError(e, "Failed to release seats for {EventId}. Wrong seat count {SeatCount}",
                message.EventId, message.SeatCount);
        }

        try
        {
            context.InboxMessages.Add(new InboxMessage { Id = message.MessageId, ReceivedOn = DateTime.UtcNow });
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e)
            when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            _logger.LogWarning("Message {MessageId} already processed", message.MessageId);
        }
    }
}