using System.ComponentModel.DataAnnotations;

using EventManager.API.Validation;

namespace EventManager.API.Models;

/// <summary>
/// Мероприятие
/// </summary>
[EventEndNotBeforeStart(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ErrorEventEndBeforeStart))]
public class EventDto
{
    /// <summary>
    /// Идентификатор мероприятия
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Название мероприятия
    /// </summary>
    [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ErrorEventTitleRequired))]
    public required string Title { get; set; }

    /// <summary>
    /// Описание мероприятия
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Дата начала мероприятия
    /// </summary>
    [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ErrorEventStartRequired))]
    public required DateTime StartAt { get; set; }

    /// <summary>
    /// Дата конца мероприятия
    /// </summary>
    [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ErrorEventEndRequired))]
    public required DateTime EndAt { get; set; }
}
