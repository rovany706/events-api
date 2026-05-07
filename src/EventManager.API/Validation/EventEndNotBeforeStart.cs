using System.ComponentModel.DataAnnotations;

namespace EventManager.API.Validation;

/// <summary>
/// Атрибут для валидации того, что дата конца мероприятия (EndAt) не раньше даты начала (StartAt)
/// </summary>
public class EventEndNotBeforeStart : ValidationAttribute
{
    private readonly string _startAtPropertyName;
    private readonly string _endAtPropertyName;

    public EventEndNotBeforeStart(string startAtPropertyName, string endAtPropertyName)
    {
        _startAtPropertyName = startAtPropertyName;
        _endAtPropertyName = endAtPropertyName;

        ErrorMessageResourceType = typeof(Resource);
        ErrorMessageResourceName = nameof(Resource.ErrorEventEndBeforeStart);
    }

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var dtoType = validationContext.ObjectType;
        var startAtValue = dtoType.GetProperty(_startAtPropertyName)?.GetValue(validationContext.ObjectInstance);
        var endAtValue = dtoType.GetProperty(_endAtPropertyName)?.GetValue(validationContext.ObjectInstance);

        if (startAtValue is not DateTime startAt || endAtValue is not DateTime endAt)
        {
            return ValidationResult.Success;
        }

        if (endAt < startAt)
        {
            return new ValidationResult(ErrorMessageString, [_endAtPropertyName]);
        }

        return ValidationResult.Success;
    }
}
