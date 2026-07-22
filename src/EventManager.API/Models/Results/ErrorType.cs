namespace EventManager.API.Models.Results;

public enum ErrorType
{
    None,
    NotFound,
    ValidationError,
    Failure,
    Conflict
}