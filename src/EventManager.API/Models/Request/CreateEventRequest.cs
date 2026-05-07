using System.ComponentModel.DataAnnotations;

using EventManager.API.Validation;

namespace EventManager.API.Models.Request;

/// <summary>
/// Запрос на создание мероприятия
/// </summary>
[EventEndNotBeforeStart]
public record CreateEventRequest
{
    /// <summary>
    /// Название мероприятия
    /// </summary>
    [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ErrorEventTitleRequired))]
    public required string Title { get; init; }

    /// <summary>
    /// Описание мероприятия
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Дата начала мероприятия
    /// </summary>
    [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ErrorEventStartRequired))]
    public required DateTime StartAt { get; init; }

    /// <summary>
    /// Дата конца мероприятия
    /// </summary>
    [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ErrorEventEndRequired))]
    public required DateTime EndAt { get; init; }
}

