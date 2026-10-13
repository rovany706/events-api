using System.Text.Json;

using Confluent.Kafka;

using EventManager.BookingService.Application.Abstractions.Persistence.Repositories;
using EventManager.BookingService.Application.Common.Messaging;
using EventManager.BookingService.Infrastructure.Options;
using EventManager.BookingService.Infrastructure.Persistence;
using EventManager.Contracts;
using EventManager.Contracts.Events;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

namespace EventManager.BookingService.Infrastructure.BackgroundServices;

public class BookingRejectedEventConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingRejectedEventConsumer> _logger;
    private readonly KafkaSettings _kafkaSettings;

    public BookingRejectedEventConsumer(IServiceScopeFactory scopeFactory, IOptions<KafkaSettings> kafkaSettings,
        ILogger<BookingRejectedEventConsumer> logger)
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
        consumer.Subscribe(KafkaTopics.BookingRejectedTopicName);

        return consumer;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(BookingRejectedEventConsumer)} started.");

        using var consumer = CreateKafkaConsumer();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    var message = JsonSerializer.Deserialize<BookingRejectedEvent>(consumeResult.Message.Value)!;

                    await ProcessMessage(message, stoppingToken);

                    consumer.Commit(consumeResult);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unhandled exception in {nameof(BookingRejectedEventConsumer)}");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        finally
        {
            consumer.Close();
        }

        _logger.LogInformation($"{nameof(BookingRejectedEventConsumer)} stopped.");
    }

    private async Task ProcessMessage(BookingRejectedEvent message, CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            
            if (await context.InboxMessages.AnyAsync(x => x.Id == message.MessageId, ct))
            {
                return;
            }
            
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            
            var bookingToReject = await bookingRepository.GetBookingByIdAsync(message.BookingId, ct);

            if (bookingToReject == null)
            {
                _logger.LogWarning("Booking to reject {BookingId} not found", message.BookingId);
                return;
            }

            bookingToReject.Reject();

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