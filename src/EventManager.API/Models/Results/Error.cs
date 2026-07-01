namespace EventManager.API.Models.Results;

public class Error
{
    public Error(string errorMessage, ErrorType errorType)
    {
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public string ErrorMessage { get; }

    public ErrorType ErrorType { get; }

    public static Error Failure(string errorMessage) => new(errorMessage, ErrorType.Failure);

    public static Error NotFound(string errorMessage) => new(errorMessage, ErrorType.NotFound);

    public static Error ValidationError(string errorMessage) => new(errorMessage, ErrorType.ValidationError);
}