namespace EventManager.Application.Common.Results;

public static class ErrorExtensions
{
    public static int GetHttpStatusCodeForError(this Error error)
    {
        return error.ErrorType switch
        {
            ErrorType.NotFound => 404,
            ErrorType.Unauthorized => 401,
            ErrorType.Conflict => 409,
            ErrorType.ValidationError => 400,
            ErrorType.Forbidden => 403,
            _ => 500,
        };
    }
}