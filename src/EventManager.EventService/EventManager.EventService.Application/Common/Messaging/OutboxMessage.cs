namespace EventManager.EventService.Application.Common.Messaging;

/// <summary>
/// Отправленное сообщение
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// Идентификатор сообщения
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Дата генерации сообщения
    /// </summary>
    public required DateTime OccurredOn { get; init; }

    /// <summary>
    /// Топик
    /// </summary>
    public required string Topic { get; init; }
    
    /// <summary>
    /// Ключ сообщения
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Содержимое сообщения
    /// </summary>
    public required string Payload { get; init; }

    /// <summary>
    /// Флаг обработки сообщения
    /// </summary>
    public required bool IsProcessed { get; set; }
}