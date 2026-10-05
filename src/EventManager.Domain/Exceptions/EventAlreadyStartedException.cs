namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение уже стартовавшего мероприятия
/// </summary>
public class EventAlreadyStartedException : Exception
{
    public EventAlreadyStartedException() { }
    public EventAlreadyStartedException(string message) : base(message) { }
    public EventAlreadyStartedException(string message, Exception inner) : base(message, inner) { }
}