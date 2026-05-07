namespace EventManager.API.Models.Response;

/// <summary>
/// Мероприятие
/// </summary>
public record EventResponse
{
    /// <summary>
    /// Идентификатор мероприятия
    /// </summary>
    public required int Id { get; init; }

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
