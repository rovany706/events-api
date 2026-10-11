using System.Data;

using EventManager.BookingService.Application.Abstractions.Messaging;
using EventManager.BookingService.Application.Abstractions.Persistence.Repositories;
using EventManager.BookingService.Domain.Entities;
using EventManager.Contracts.Events;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventManager.BookingService.Application.BackgroundServices;

/// <summary>
/// Фоновый сервис обработки бронирований
/// </summary>
public class BookingProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBookingConfirmationPublisher _bookingConfirmationPublisher;
    private readonly ILogger<BookingProcessorService> _logger;

    private const int ProcessingDelayInSeconds = 5;
    private const int PollingIntervalInSeconds = 5;

    public BookingProcessorService(IServiceScopeFactory scopeFactory,
        IBookingConfirmationPublisher bookingConfirmationPublisher,
        ILogger<BookingProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _bookingConfirmationPublisher = bookingConfirmationPublisher;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(BookingProcessorService)} started.");

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
                _logger.LogError(e, $"Unhandled exception in {nameof(BookingProcessorService)}");
            }

            await Task.Delay(TimeSpan.FromSeconds(PollingIntervalInSeconds), stoppingToken);
        }

        _logger.LogInformation($"{nameof(BookingProcessorService)} stopped.");
    }

    private async Task ProcessBookingAsync(int bookingId, CancellationToken ct)
    {
        _logger.LogInformation("Processing booking {Id}", bookingId);

        using var scope = _scopeFactory.CreateScope();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var booking = await bookingRepository.GetBookingByIdAsync(bookingId, ct);

        if (booking == null || booking.Status != BookingStatus.Pending)
        {
            _logger.LogDebug("Booking {Id} not found or already processed.", bookingId);
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(ProcessingDelayInSeconds), ct); // working...

        try
        {
            booking.Confirm();
            await bookingRepository.SaveChangesAsync(ct);
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
        
        await _bookingConfirmationPublisher.PublishAsync(
            new BookingConfirmedEvent(booking.EventId, booking.Id, booking.UserId, 1, DateTime.UtcNow), ct);
            _logger.LogInformation("Processed booking {Id} ({BookingStatus})", booking.Id, booking.Status);
    }
}