using EventManager.API.Models;

using System.ComponentModel.DataAnnotations;

namespace EventManager.API.Validation;

public class EventEndNotBeforeStart : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not EventDto eventDto)
        {
            return false;
        }

        return eventDto.StartAt < eventDto.EndAt;
    }
}
