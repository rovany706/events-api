namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение отсутствия прав на выполнение операции
/// </summary>
public class InsufficientRightsException : Exception
{
    public InsufficientRightsException() : this("Не хватает прав на выполнение операции") { }
    public InsufficientRightsException(string message) : base(message) { }
    public InsufficientRightsException(string message, Exception inner) : base(message, inner) { }
}