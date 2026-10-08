using EventManager.Domain.Entities.Bookings;

namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение превышения количества активных броней
/// </summary>
public class TooManyActiveBookingsException : Exception
{
    public TooManyActiveBookingsException() : this($"Превышено количество активных бронирований ({BookingConstants.MaxActiveBookingCountPerUser})") { }
    public TooManyActiveBookingsException(string message) : base(message) { }
    public TooManyActiveBookingsException(string message, Exception inner) : base(message, inner) { }
}