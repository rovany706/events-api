namespace EventManager.BookingService.Application.Common.Messaging;

/// <summary>
/// Полученное сообщение
/// </summary>
public class InboxMessage
{
    /// <summary>
    /// Идентификатор сообщения
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Дата получения сообщения
    /// </summary>
    public required DateTime ReceivedOn { get; init; }
}