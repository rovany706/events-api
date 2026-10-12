using EventManager.BookingService.Application.Abstractions.Messaging;
using EventManager.BookingService.Infrastructure.Persistence;
using EventManager.Contracts;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventManager.BookingService.Infrastructure.BackgroundServices;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<OutboxProcessor> _logger;
    private const int PollingIntervalInSeconds = 1;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, IMessageBroker messageBroker,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _messageBroker = messageBroker;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(OutboxProcessor)} started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

                var messages = await context.OutboxMessages
                    .Where(m => !m.IsProcessed)
                    .OrderBy(m => m.OccurredOn)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    if (string.IsNullOrWhiteSpace(message.Topic))
                    {
                        _logger.LogError("Outbox message {MessageId} has no topic", message.Id);
                        continue;
                    }

                    try
                    {
                        await _messageBroker.PublishAsync(message.Topic, message.Key,
                            message.Payload,
                            stoppingToken);
                        message.IsProcessed = true;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to publish outbox message {MessageId} to {Topic}",
                            message.Id, message.Topic);
                        break;
                    }
                }

                await context.SaveChangesAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(PollingIntervalInSeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unhandled exception in {nameof(OutboxProcessor)}");
            }
        }

        _logger.LogInformation($"{nameof(OutboxProcessor)} stopped.");
    }
}
