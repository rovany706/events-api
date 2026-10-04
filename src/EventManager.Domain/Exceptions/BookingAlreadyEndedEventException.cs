namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение бронирования законченного события
/// </summary>
public class BookingAlreadyEndedEventException : Exception
{
    public BookingAlreadyEndedEventException() { }
    public BookingAlreadyEndedEventException(string message) : base(message) { }
    public BookingAlreadyEndedEventException(string message, Exception inner) : base(message, inner) { }
}