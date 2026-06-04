namespace EventManager.API.Models.Request;

/// <summary>
/// Параметры запроса всех событий
/// </summary>
public record GetEventsFilterParams
{
    /// <summary>
    /// Название события (регистронезависимое, частичное совпадение)
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// События, которые начинаются не раньше указанной даты
    /// </summary>
    public DateTime? From { get; init; }

    /// <summary>
    /// События, которые заканчиваются не позже указанной даты
    /// </summary>
    public DateTime? To { get; init; }
}
