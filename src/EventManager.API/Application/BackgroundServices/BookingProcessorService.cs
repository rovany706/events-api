using EventManager.API.Domain.Interfaces;
using EventManager.API.Models.Entities;
using EventManager.API.Models.Tasks;

namespace EventManager.API.Application.BackgroundServices;

/// <summary>
/// Фоновый сервис обработки бронирований
/// </summary>
public class BookingProcessorService : BackgroundService
{
    private readonly IBookingTaskQueue _bookingTaskQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingProcessorService> _logger;

    public BookingProcessorService(IBookingTaskQueue bookingTaskQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<BookingProcessorService> logger)
    {
        _bookingTaskQueue = bookingTaskQueue;
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
                if (_bookingTaskQueue.TryDequeue(out var bookingTask))
                {
                    _logger.LogInformation("Processing booking {Id}", bookingTask.BookingId);
                    await ProcessBookingAsync(bookingTask, stoppingToken);
                    _logger.LogInformation("Processed booking {Id}", bookingTask.BookingId);
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
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        
        _logger.LogInformation($"{nameof(BookingProcessorService)} stopped.");
    }

    private async Task ProcessBookingAsync(BookingTask bookingTask, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

        var booking = bookingRepository.GetBookingById(bookingTask.BookingId);
        if (booking == null)
        {
            _logger.LogWarning("Skipping booking {Id}. Not found.", bookingTask.BookingId);
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(2), ct); // working...

        var processedBooking = booking with { Status = BookingStatus.Confirmed, ProcessedAt = DateTime.UtcNow };
        bookingRepository.UpdateBooking(processedBooking);
    }
}