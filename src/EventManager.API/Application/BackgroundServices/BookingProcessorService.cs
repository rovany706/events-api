using EventManager.API.Domain.Interfaces;
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
                using var scope = _scopeFactory.CreateScope();
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

                _logger.LogInformation("Checking for pending bookings...");
                var pendingBookings = bookingRepository.GetPendingBookings().ToList();
                _logger.LogInformation("Found {pendingCount} pending bookings.", pendingBookings.Count);

                var processingTasks = pendingBookings.Select(b => ProcessBookingAsync(b, bookingRepository, eventRepository, stoppingToken));
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

    private async Task ProcessBookingAsync(Booking booking, IBookingRepository bookingRepository, IEventRepository eventRepository, CancellationToken ct)
    {
        _logger.LogInformation("Processing booking {Id}", booking.Id);

        await Task.Delay(TimeSpan.FromSeconds(ProcessingDelayInSeconds), ct); // working...

        var eventToBook = eventRepository.GetEventById(booking.EventId);

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

            bookingRepository.UpdateBooking(booking);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when processing booking {bookingId}", booking.Id);
            ReleaseBooking(booking, eventToBook, bookingRepository, eventRepository);
        }
        finally
        {
            _bookingSemaphore.Release();
            _logger.LogInformation("Processed booking {Id} ({BookingStatus})", booking.Id, booking.Status);
        }
    }

    private static void ReleaseBooking(Booking booking, Event? eventToBook,
        IBookingRepository bookingRepository, IEventRepository eventRepository)
    {
        booking.Reject();
        bookingRepository.UpdateBooking(booking);

        if (eventToBook != null)
        {
            eventToBook.ReleaseSeats();
            eventRepository.UpdateEvent(eventToBook);
        }
    }
}