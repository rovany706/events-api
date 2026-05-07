using EventManager.API.Models;

using System.ComponentModel.DataAnnotations;

namespace EventManager.API.Validation;

/// <summary>
/// Атрибут для валидации того, что дата конца мероприятия не раньше даты начала
/// </summary>
public class EventEndNotBeforeStart : ValidationAttribute
{
    public EventEndNotBeforeStart()
    {
        ErrorMessageResourceType = typeof(Resource);
        ErrorMessageResourceName = nameof(Resource.ErrorEventEndBeforeStart);
    }

    /// <inheritdoc />
    public override bool IsValid(object? value)
    {
        if (value is not Event eventDto)
        {
            return true;
        }

        return eventDto.StartAt < eventDto.EndAt;
    }
}
