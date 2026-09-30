namespace EventManager.Domain.Common.Results;

public enum ErrorType
{
    None,
    NotFound,
    ValidationError,
    Failure,
    Conflict
}