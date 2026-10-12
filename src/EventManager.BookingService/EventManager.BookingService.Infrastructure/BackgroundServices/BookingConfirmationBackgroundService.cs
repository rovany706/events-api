using System.Data;
using System.Text.Json;

using EventManager.BookingService.Application.Abstractions.Persistence.Repositories;
using EventManager.BookingService.Application.Common.Messaging;
using EventManager.BookingService.Domain.Entities;
using EventManager.BookingService.Infrastructure.Persistence;
using EventManager.Contracts;
using EventManager.Contracts.Events;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventManager.BookingService.Infrastructure.BackgroundServices;

/// <summary>
/// Фоновый сервис обработки бронирований
/// </summary>
public class BookingConfirmationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingConfirmationBackgroundService> _logger;

    private const int ProcessingDelayInSeconds = 5;
    private const int PollingIntervalInSeconds = 5;

    public BookingConfirmationBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BookingConfirmationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(BookingConfirmationBackgroundService)} started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                List<int> pendingBookingIds;
                using (var scope = _scopeFactory.CreateScope())
                {
                    var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                    _logger.LogInformation("Checking for pending bookings...");
                    var pendingBookings = await bookingRepository.GetPendingBookingsAsync(stoppingToken);
                    pendingBookingIds = pendingBookings.Select(b => b.Id).ToList();

                    _logger.LogInformation("Found {pendingCount} pending bookings.", pendingBookingIds.Count);
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
                _logger.LogError(e, $"Unhandled exception in {nameof(BookingConfirmationBackgroundService)}");
            }

            await Task.Delay(TimeSpan.FromSeconds(PollingIntervalInSeconds), stoppingToken);
        }

        _logger.LogInformation($"{nameof(BookingConfirmationBackgroundService)} stopped.");
    }

    private async Task ProcessBookingAsync(int bookingId, CancellationToken ct)
    {
        _logger.LogInformation("Processing booking {Id}", bookingId);

        using var scope = _scopeFactory.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var booking = await bookingRepository.GetBookingByIdAsync(bookingId, ct);

        if (booking == null || booking.Status != BookingStatus.Pending)
        {
            _logger.LogDebug("Booking {Id} not found or already processed.", bookingId);
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(ProcessingDelayInSeconds), ct); // working...
        booking.Confirm();
        SaveOutboxMessage(booking, context);
        
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DBConcurrencyException)
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
        var confirmation = new BookingConfirmedEvent(Guid.NewGuid(), booking.EventId, booking.Id, booking.UserId, 1, DateTime.UtcNow);
        var outboxMessage = new OutboxMessage
        {
            Id = confirmation.MessageId,
            Key = booking.EventId.ToString(),
            Topic = KafkaTopics.BookingConfirmedTopicName,
            Payload = JsonSerializer.Serialize(confirmation),
            OccurredOn = DateTime.UtcNow,
            IsProcessed = false
        };
        
        context.OutboxMessages.Add(outboxMessage);
    }
}