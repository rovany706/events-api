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
    private readonly Random _rnd = new();
    private const int ProcessingDelayInSeconds = 2;
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
                
                var bookings = bookingRepository.GetBookings();
                
                _logger.LogInformation("Checking for pending bookings...");
                var pendingBookings = bookings.Where(b => b.Status == BookingStatus.Pending).ToList();
                _logger.LogInformation("Found {pendingCount} pending bookings.", pendingBookings.Count);
                
                foreach (var booking in pendingBookings)
                {
                    await ProcessBookingAsync(booking, bookingRepository, stoppingToken);
                }
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

    private async Task ProcessBookingAsync(Booking booking, IBookingRepository bookingRepository, CancellationToken ct)
    {
        _logger.LogInformation("Processing booking {Id}", booking.Id);

        await Task.Delay(TimeSpan.FromSeconds(ProcessingDelayInSeconds), ct); // working...

        var bookingStatus = _rnd.Next(0, 2) == 0 ? BookingStatus.Confirmed : BookingStatus.Rejected;
        var processedBooking = booking with { Status = bookingStatus, ProcessedAt = DateTime.UtcNow };
        bookingRepository.UpdateBooking(processedBooking);

        _logger.LogInformation("Processed booking {Id} ({BookingStatus})", booking.Id, bookingStatus);
    }
}