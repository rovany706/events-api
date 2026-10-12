using System.Globalization;

using EventManager.BookingService.Application.Abstractions.Persistence.Repositories;
using EventManager.BookingService.Application.Abstractions.Services;
using EventManager.BookingService.Application.Common.Results;
using EventManager.BookingService.Domain.Entities;
using EventManager.BookingService.Domain.Exceptions;
using EventManager.Contracts.Jwt;

using Microsoft.Extensions.Logging;

namespace EventManager.BookingService.Application.Services.BookingService;

/// <summary>
/// Сервис бронирования
/// </summary>
public class BookingServiceImpl : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<BookingServiceImpl> _logger;

    public BookingServiceImpl(IBookingRepository bookingRepository, ILogger<BookingServiceImpl> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<Booking?>> CreateBookingAsync(int eventId, int userId, CancellationToken cancellationToken)
    {
        var userBookingCount =
            await _bookingRepository.GetActiveBookingCountForUserAsync(userId, cancellationToken);
        if (userBookingCount >= BookingConstants.MaxActiveBookingCountPerUser)
            throw new TooManyActiveBookingsException();

        var booking = Booking.CreateInstance(eventId, userId);

        await _bookingRepository.AddBookingAsync(booking, cancellationToken);
        await _bookingRepository.SaveChangesAsync(cancellationToken);

        return Result<Booking?>.Success(booking);
    }

    /// <inheritdoc />
    public async Task<Result<Booking?>> GetBookingByIdAsync(int bookingId, int userId, string userRole,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);
        if (booking == null)
        {
            _logger.LogDebug("Booking with {bookingId} not found.", bookingId);
            return Result<Booking?>.Failure(
                Error.NotFound(string.Format(CultureInfo.InvariantCulture, Resource.ErrorBookingNotFound, bookingId)));
        }

        if (!IsAuthorized(booking.UserId, userId, userRole))
            throw new InsufficientRightsException();

        return Result<Booking?>.Success(booking);
    }

    /// <inheritdoc />
    public async Task<Result> CancelBookingAsync(int bookingId, int userId, string userRole,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

        if (booking == null)
            return Result<Booking?>.Failure(Error.NotFound($"Booking with {bookingId} not found."));

        if (!IsAuthorized(booking.UserId, userId, userRole))
            throw new InsufficientRightsException();

        booking.SetPendingCancel();
        await _bookingRepository.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }

    private static bool IsAuthorized(int bookingUserId, int userId, string userRole)
    {
        var isAdmin = userRole == JwtRoles.AdminRole;
        var isUserBooking = bookingUserId == userId;

        return isAdmin || isUserBooking;
    }
}