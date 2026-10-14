using System.Text.Json;

using Confluent.Kafka;

using EventManager.Contracts;
using EventManager.Contracts.Events;
using EventManager.EventService.Application.Abstractions.Caching;
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
                    var message = JsonSerializer.Deserialize<BookingConfirmedEvent>(consumeResult.Message.Value)!;

                    await ProcessMessageAsync(message, stoppingToken);

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

    private async Task ProcessMessageAsync(BookingConfirmedEvent message, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        var cache = scope.ServiceProvider.GetRequiredService<ICache>();

        if (await context.InboxMessages.AnyAsync(x => x.Id == message.MessageId, ct))
        {
            return;
        }

        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var eventToBook = await eventRepository.GetEventByIdAsync(message.EventId, ct);

        try
        {
            if (eventToBook == null)
            {
                _logger.LogWarning("Event to book {EventId} not found", message.EventId);
                RejectBookingConfirmation(context, message);
            }
            else
            {
                var reserveResult = eventToBook.TryReserveSeats(message.SeatCount);

                if (!reserveResult)
                {
                    _logger.LogWarning("No seats available for {EventId}", message.EventId);
                    RejectBookingConfirmation(context, message);
                }
            }
        }
        catch (EventAlreadyStartedException e)
        {
            _logger.LogError(e, "Failed to reserve seats for {EventId}. Event already started", message.EventId);
            RejectBookingConfirmation(context, message);
        }
        catch (ArgumentOutOfRangeException e)
        {
            _logger.LogError(e, "Failed to reserve seats for {EventId}. Wrong seat count {SeatCount}",
                message.EventId, message.SeatCount);
            RejectBookingConfirmation(context, message);
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

        if (eventToBook != null)
            await cache.SetByKeyAsync($"event:{eventToBook.Id}", JsonSerializer.Serialize(eventToBook),
                CacheTtlValues.GetEventByIdTtl);
    }

    private static void RejectBookingConfirmation(EventDbContext context, BookingConfirmedEvent message)
    {
        var rejection = new BookingRejectedEvent(Guid.NewGuid(), message.EventId, message.BookingId, message.UserId);
        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = rejection.MessageId,
            Key = rejection.EventId.ToString(),
            Topic = KafkaTopics.BookingRejectedTopicName,
            Payload = JsonSerializer.Serialize(rejection),
            OccurredOn = DateTime.UtcNow,
            IsProcessed = false
        });
    }
}