namespace EventManager.BookingService.Domain.Exceptions;

public class BookingNotConfirmedException : Exception
{
    public BookingNotConfirmedException(): this("Бронирование не подтверждено") { }
    public BookingNotConfirmedException(string message) : base(message) { }
    public BookingNotConfirmedException(string message, Exception inner) : base(message, inner) { }
}
