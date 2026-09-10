using EventManager.API.Domain.DataAccess;
using EventManager.API.Models.Entities;
using EventManager.API.Models.Results;

using Microsoft.EntityFrameworkCore;

namespace EventManager.API.Application.Services.BookingService;

/// <summary>
/// Сервис бронирования
/// </summary>
public class BookingServiceImpl : IBookingService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<BookingServiceImpl> _logger;
    private static readonly SemaphoreSlim BookingSemaphore = new(1, 1);

    public BookingServiceImpl(AppDbContext dbContext, ILogger<BookingServiceImpl> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<Booking?>> CreateBookingAsync(int eventId, CancellationToken cancellationToken)
    {
        await BookingSemaphore.WaitAsync(cancellationToken);
        try
        {
            var eventToBook = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

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

            _dbContext.Bookings.Add(booking);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
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
        var booking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

        if (booking == null)
        {
            _logger.LogDebug("Booking with {bookingId} not found.", bookingId);
            return Result<Booking?>.Failure(Error.NotFound($"Booking with {bookingId} not found."));
        }

        return Result<Booking?>.Success(booking);
    }
}