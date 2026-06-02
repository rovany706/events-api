namespace EventManager.API.Models;

/// <summary>
/// Мероприятие
/// </summary>
public record Event
{
    /// <summary>
    /// Идентификатор мероприятия
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Название мероприятия
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Описание мероприятия
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Дата начала мероприятия
    /// </summary>
    public required DateTime StartAt { get; init; }

    /// <summary>
    /// Дата конца мероприятия
    /// </summary>
    public required DateTime EndAt { get; init; }
}
