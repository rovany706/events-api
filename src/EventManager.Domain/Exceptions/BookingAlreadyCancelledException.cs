namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение повторной отмены бронирования
/// </summary>
public class BookingAlreadyCancelledException : Exception
{
    public BookingAlreadyCancelledException() { }
    public BookingAlreadyCancelledException(string message) : base(message) { }
    public BookingAlreadyCancelledException(string message, Exception inner) : base(message, inner) { }
}
