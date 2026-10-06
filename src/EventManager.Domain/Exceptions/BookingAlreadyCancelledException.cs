namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение повторной отмены бронирования
/// </summary>
public class BookingAlreadyCancelledException : Exception
{
    public BookingAlreadyCancelledException() : this("Бронирование уже отменено") { }
    public BookingAlreadyCancelledException(string message) : base(message) { }
    public BookingAlreadyCancelledException(string message, Exception inner) : base(message, inner) { }
}
