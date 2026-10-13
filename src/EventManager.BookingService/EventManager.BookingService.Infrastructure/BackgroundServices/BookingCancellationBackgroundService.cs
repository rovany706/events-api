using System.Data;
using System.Text.Json;

using EventManager.BookingService.Application.Abstractions.Persistence.Repositories;
using EventManager.BookingService.Application.Common.Messaging;
using EventManager.BookingService.Domain.Entities;
using EventManager.BookingService.Infrastructure.Persistence;
using EventManager.Contracts;
using EventManager.Contracts.Events;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventManager.BookingService.Infrastructure.BackgroundServices;

public class BookingCancellationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingCancellationBackgroundService> _logger;

    private const int PollingIntervalInSeconds = 5;

    public BookingCancellationBackgroundService(IServiceScopeFactory scopeFactory,
        ILogger<BookingCancellationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(BookingCancellationBackgroundService)} started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                List<int> pendingBookingIds;
                using (var scope = _scopeFactory.CreateScope())
                {
                    var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                    _logger.LogInformation("Checking for pending cancellation bookings...");
                    var pendingBookings = await bookingRepository.GetPendingCancellationBookingsAsync(stoppingToken);
                    pendingBookingIds = pendingBookings.Select(b => b.Id).ToList();

                    _logger.LogInformation("Found {pendingCount} pending cancellation bookings.",
                        pendingBookingIds.Count);
                }

                var processingTasks = pendingBookingIds.Select(b => ProcessBookingAsync(b, stoppingToken));
                await Task.WhenAll(processingTasks);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unhandled exception in {nameof(BookingCancellationBackgroundService)}");
            }

            await Task.Delay(TimeSpan.FromSeconds(PollingIntervalInSeconds), stoppingToken);
        }

        _logger.LogInformation($"{nameof(BookingCancellationBackgroundService)} stopped.");
    }

    private async Task ProcessBookingAsync(int bookingId, CancellationToken ct)
    {
        _logger.LogInformation("Processing booking {Id}", bookingId);

        using var scope = _scopeFactory.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var booking = await bookingRepository.GetBookingByIdAsync(bookingId, ct);

        if (booking == null || booking.Status != BookingStatus.PendingCancellation)
        {
            _logger.LogDebug("Booking {Id} not found or already processed.", bookingId);
            return;
        }

        booking.Cancel();
        SaveOutboxMessage(booking, context);

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Booking {BookingId} was changed by another instance", booking.Id);
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when processing booking {bookingId}", booking.Id);
        }

        _logger.LogInformation("Processed booking {Id} ({BookingStatus})", booking.Id, booking.Status);
    }

    private static void SaveOutboxMessage(Booking booking, BookingDbContext context)
    {
        var cancellation = new BookingCancelledEvent(Guid.NewGuid(), booking.EventId, booking.Id, booking.UserId, 1);
        var outboxMessage = new OutboxMessage
        {
            Id = cancellation.MessageId,
            Key = booking.EventId.ToString(),
            Topic = KafkaTopics.BookingCancelledTopicName,
            Payload = JsonSerializer.Serialize(cancellation),
            OccurredOn = DateTime.UtcNow,
            IsProcessed = false
        };

        context.OutboxMessages.Add(outboxMessage);
    }
}