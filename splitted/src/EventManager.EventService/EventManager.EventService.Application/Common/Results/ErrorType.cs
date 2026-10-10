namespace EventManager.EventService.Application.Common.Results;

public enum ErrorType
{
    None,
    NotFound,
    ValidationError,
    Failure,
    Conflict,
    Unauthorized,
    Forbidden
}