using EventManager.API.Domain.Repositories;
using EventManager.API.Models.Entities;

namespace EventManager.API.Application.BackgroundServices;

/// <summary>
/// Фоновый сервис обработки бронирований
/// </summary>
public class BookingProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingProcessorService> _logger;
    private readonly SemaphoreSlim _bookingSemaphore = new(1, 1);

    private const int ProcessingDelayInSeconds = 5;
    private const int PollingIntervalInSeconds = 5;

    public BookingProcessorService(IServiceScopeFactory scopeFactory,
        ILogger<BookingProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
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
        
        var eventToBook = booking.Event;

        await _bookingSemaphore.WaitAsync(ct);

        try
        {
            if (eventToBook == null)
            {
                booking.Reject();
                _logger.LogWarning("Event {eventId} is not found for booking {bookingId}", booking.EventId, booking.Id);
            }
            else
            {
                booking.Confirm();
            }

            await bookingRepository.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when processing booking {bookingId}", booking.Id);
            ReleaseBooking(booking, eventToBook);

            await bookingRepository.SaveChangesAsync(ct);
        }
        finally
        {
            _bookingSemaphore.Release();
            _logger.LogInformation("Processed booking {Id} ({BookingStatus})", booking.Id, booking.Status);
        }
    }

    private static void ReleaseBooking(Booking booking, Event? eventToBook)
    {
        booking.Reject();
        eventToBook?.ReleaseSeats();
    }
}