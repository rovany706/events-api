namespace EventManager.API.Application.Services.EventService.Models;

/// <summary>
/// Параметры фильтрации
/// </summary>
public record EventFilterDto
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
