using EventManager.API.Domain.DataAccess;
using EventManager.API.Domain.Repositories;
using EventManager.API.Models.Entities;
using EventManager.API.Models.Results;

using Microsoft.EntityFrameworkCore;

namespace EventManager.API.Application.Services.BookingService;

/// <summary>
/// Сервис бронирования
/// </summary>
public class BookingServiceImpl : IBookingService
{
    private readonly IEventRepository _eventRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<BookingServiceImpl> _logger;
    private static readonly SemaphoreSlim BookingSemaphore = new(1, 1);

    public BookingServiceImpl(IEventRepository eventRepository, IBookingRepository bookingRepository, ILogger<BookingServiceImpl> logger)
    {
        _eventRepository = eventRepository;
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<Booking?>> CreateBookingAsync(int eventId, CancellationToken cancellationToken)
    {
        await BookingSemaphore.WaitAsync(cancellationToken);
        try
        {
            var eventToBook = await _eventRepository.GetEventByIdAsync(eventId, cancellationToken);

            if (eventToBook == null)
            {
                _logger.LogDebug("Booking failed. Event with {eventId} not found.", eventId);
                return Result<Booking?>.Failure(Error.NotFound($"Booking failed. Event with {eventId} not found."));
            }

            var bookResult = eventToBook.TryReserveSeats();

            if (!bookResult)
            {
                return Result<Booking?>.Failure(Error.Conflict($"No available seats available for event {eventId}"));
            }

            var booking = Booking.CreateInstance(eventId);

            await _bookingRepository.AddBookingAsync(booking, cancellationToken);
            await _bookingRepository.SaveChangesAsync(cancellationToken);
            
            return Result<Booking?>.Success(booking);
        }
        finally
        {
            BookingSemaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<Result<Booking?>> GetBookingByIdAsync(int bookingId,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

        if (booking == null)
        {
            _logger.LogDebug("Booking with {bookingId} not found.", bookingId);
            return Result<Booking?>.Failure(Error.NotFound($"Booking with {bookingId} not found."));
        }

        return Result<Booking?>.Success(booking);
    }
}