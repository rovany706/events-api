using EventManager.API.Application.Services.EventService;
using EventManager.API.Domain.Interfaces;
using EventManager.API.Models.Entities;
using EventManager.API.Models.Results;
using EventManager.API.Models.Tasks;

namespace EventManager.API.Application.Services.BookingService;

/// <summary>
/// Сервис бронирования
/// </summary>
public class BookingServiceImpl : IBookingService
{
    private readonly IBookingRepository _repository;
    private readonly IEventService _eventService;
    private readonly IBookingTaskQueue _bookingTaskQueue;
    private readonly ILogger<BookingServiceImpl> _logger;

    public BookingServiceImpl(
        IBookingRepository repository,
        IEventService eventService,
        IBookingTaskQueue bookingTaskQueue,
        ILogger<BookingServiceImpl> logger)
    {
        _repository = repository;
        _eventService = eventService;
        _bookingTaskQueue = bookingTaskQueue;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<Booking?>> CreateBookingAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var result = _eventService.GetEventById(eventId);

        if (!result.IsSuccess && result.Error!.ErrorType == ErrorType.NotFound)
        {
            _logger.LogDebug("Booking failed. Event with {eventId} not found.", eventId);
            return Result<Booking?>.Failure(Error.NotFound($"Booking failed. Event with {eventId} not found."));
        }

        var booking = new Booking
        {
            Id = 0, EventId = eventId, Status = BookingStatus.Pending, CreatedAt = DateTime.UtcNow
        };

        var newId = _repository.AddBooking(booking);
        _bookingTaskQueue.Enqueue(new BookingTask(newId, DateTime.UtcNow));

        return Result<Booking?>.Success(booking with { Id = newId });
    }

    /// <inheritdoc />
    public async Task<Result<Booking?>> GetBookingByIdAsync(int bookingId,
        CancellationToken cancellationToken = default)
    {
        var booking = _repository.GetBookingById(bookingId);

        if (booking == null)
        {
            _logger.LogDebug("Booking with {bookingId} not found.", bookingId);
            return Result<Booking?>.Failure(Error.NotFound($"Booking with {bookingId} not found."));
        }

        return Result<Booking?>.Success(booking);
    }
}