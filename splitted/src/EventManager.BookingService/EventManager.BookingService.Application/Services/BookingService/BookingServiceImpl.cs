using System.Globalization;

using EventManager.BookingService.Application.Abstractions.Persistence.Repositories;
using EventManager.BookingService.Application.Abstractions.Services;
using EventManager.BookingService.Application.Common.Results;
using EventManager.BookingService.Domain.Entities;
using EventManager.BookingService.Domain.Exceptions;

using Microsoft.Extensions.Logging;

namespace EventManager.BookingService.Application.Services.BookingService;

/// <summary>
/// Сервис бронирования
/// </summary>
public class BookingServiceImpl : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<BookingServiceImpl> _logger;
    private static readonly SemaphoreSlim BookingSemaphore = new(1, 1);

    public BookingServiceImpl(IBookingRepository bookingRepository, ILogger<BookingServiceImpl> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<Booking?>> CreateBookingAsync(int eventId, int userId, CancellationToken cancellationToken)
    {
        await BookingSemaphore.WaitAsync(cancellationToken);
        try
        {
            // TODO
            // var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
            //
            // if (user == null)
            //     return Result<Booking?>.Failure(Error.NotFound($"Booking failed. User with {userId} not found."));
            //
            // var userBookingCount =
            //     await _bookingRepository.GetActiveBookingCountForUserAsync(userId, cancellationToken);
            // if (userBookingCount >= BookingConstants.MaxActiveBookingCountPerUser)
            //     throw new TooManyActiveBookingsException();

            //var eventToBook = await _eventRepository.GetEventByIdAsync(eventId, cancellationToken);

            // if (eventToBook == null)
            // {
            //     _logger.LogDebug("Booking failed. Event with {eventId} not found.", eventId);
            //     return Result<Booking?>.Failure(Error.NotFound($"Booking failed. Event with {eventId} not found."));
            // }
            //
            // var bookResult = eventToBook.TryReserveSeats();
            //
            // if (!bookResult)
            //     return Result<Booking?>.Failure(Error.Conflict($"No available seats available for event {eventId}"));

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
    public async Task<Result<Booking?>> GetBookingByIdAsync(int bookingId, int userId,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);
        // TODO
        // var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        //
        // if (booking == null)
        // {
        //     _logger.LogDebug("Booking with {bookingId} not found.", bookingId);
        //     return Result<Booking?>.Failure(
        //         Error.NotFound(string.Format(CultureInfo.InvariantCulture, Resource.ErrorBookingNotFound, bookingId)));
        // }
        //
        // if (user == null)
        //     return Result<Booking?>.Failure(Error.NotFound(Resource.ErrorUserNotFound));
        //
        // if (!IsAuthorized(user, booking))
        //     throw new InsufficientRightsException();

        return Result<Booking?>.Success(booking);
    }

    /// <inheritdoc />
    public async Task<Result> CancelBookingAsync(int bookingId, int userId, CancellationToken cancellationToken)
    {
        // TODO
        // var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        //
        // if (user == null)
        //     return Result<Booking?>.Failure(Error.NotFound($"User with {userId} not found."));

        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

        if (booking == null)
            return Result<Booking?>.Failure(Error.NotFound($"Booking with {bookingId} not found."));

        // if (!IsAuthorized(user, booking))
        //     throw new InsufficientRightsException();

        await CancelBookingCoreAsync(booking, cancellationToken);
        return Result.Success();
    }

    // TODO
    // private static bool IsAuthorized(User user, Booking booking)
    // {
    //     var isAdmin = user.Role == UserRole.Admin;
    //     var isUserBooking = booking.UserId == user.Id;
    //     
    //     return isAdmin || isUserBooking;
    // }

    private Task CancelBookingCoreAsync(Booking booking, CancellationToken cancellationToken)
    {
        booking.Cancel();
        // booking.Event.ReleaseSeats(); // TODO

        return _bookingRepository.SaveChangesAsync(cancellationToken);
    }
}