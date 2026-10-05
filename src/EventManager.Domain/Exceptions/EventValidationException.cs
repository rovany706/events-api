namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение валидации мероприятия
/// </summary>
public class EventValidationException : Exception
{
    public EventValidationException() { }
    public EventValidationException(string message) : base(message) { }
    public EventValidationException(string message, Exception inner) : base(message, inner) { }
}