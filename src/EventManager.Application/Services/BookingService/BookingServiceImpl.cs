using EventManager.Application.Abstractions.Persistence.Repositories;
using EventManager.Application.Abstractions.Services;
using EventManager.Application.Common.Results;
using EventManager.Domain.Entities.Bookings;
using EventManager.Domain.Entities.Users;
using EventManager.Domain.Exceptions;

using Microsoft.Extensions.Logging;

namespace EventManager.Application.Services.BookingService;

/// <summary>
/// Сервис бронирования
/// </summary>
public class BookingServiceImpl : IBookingService
{
    private readonly IEventRepository _eventRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<BookingServiceImpl> _logger;
    private static readonly SemaphoreSlim BookingSemaphore = new(1, 1);

    public BookingServiceImpl(IEventRepository eventRepository, IBookingRepository bookingRepository,
        IUserRepository userRepository,
        ILogger<BookingServiceImpl> logger)
    {
        _eventRepository = eventRepository;
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<Booking?>> CreateBookingAsync(int eventId, int userId, CancellationToken cancellationToken)
    {
        await BookingSemaphore.WaitAsync(cancellationToken);
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);

            if (user == null)
                return Result<Booking?>.Failure(Error.NotFound($"Booking failed. User with {userId} not found."));

            var userBookingCount = await _userRepository.GetActiveBookingCount(userId, cancellationToken);
            if (userBookingCount == BookingConstants.MaxActiveBookingCountPerUser)
                throw new TooManyActiveBookingsException();

            var eventToBook = await _eventRepository.GetEventByIdAsync(eventId, cancellationToken);

            if (eventToBook == null)
            {
                _logger.LogDebug("Booking failed. Event with {eventId} not found.", eventId);
                return Result<Booking?>.Failure(Error.NotFound($"Booking failed. Event with {eventId} not found."));
            }

            var bookResult = eventToBook.TryReserveSeats();

            if (!bookResult)
                return Result<Booking?>.Failure(Error.Conflict($"No available seats available for event {eventId}"));

            var booking = Booking.CreateInstance(eventId, userId);

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
    public async Task<Result<Booking?>> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

        if (booking == null)
        {
            _logger.LogDebug("Booking with {bookingId} not found.", bookingId);
            return Result<Booking?>.Failure(Error.NotFound($"Booking with {bookingId} not found."));
        }

        return Result<Booking?>.Success(booking);
    }

    /// <inheritdoc />
    public async Task<Result> CancelBookingAsync(int bookingId, int userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);

        if (user == null)
            return Result<Booking?>.Failure(Error.NotFound($"User with {userId} not found."));

        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

        if (booking == null)
            return Result<Booking?>.Failure(Error.NotFound($"Booking with {bookingId} not found."));

        var isAdmin = user.Role == UserRole.Admin;
        var isUserBooking = booking.UserId == userId;

        if (!isAdmin && !isUserBooking)
            throw new InsufficientRightsException();

        await CancelBookingCoreAsync(booking, cancellationToken);
        return Result.Success();
    }

    private Task CancelBookingCoreAsync(Booking booking, CancellationToken cancellationToken)
    {
        booking.Cancel();
        booking.Event.ReleaseSeats();
        
        return _bookingRepository.SaveChangesAsync(cancellationToken);
    }
}