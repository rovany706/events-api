namespace EventManager.API.Models.Results;

public class Result
{
    protected Result()
    {
        IsSuccess = true;
        Error = null;
    }

    protected Result(Error error)
    {
        IsSuccess = false;
        Error = error;
    }

    public bool IsSuccess { get; }

    public Error? Error { get; }

    public static implicit operator Result(Error error) => new(error);

    public static Result Success() => new();

    public static Result Failure(Error error) => new(error);
}

public class Result<TValue> : Result
{
    private Result(TValue value)
    {
        Value = value;
    }

    private Result(Error error) : base(error)
    {
        Value = default;
    }

    public TValue Value => IsSuccess
        ? field!
        : throw new InvalidOperationException("Value can not be accessed when IsSuccess is false");
    
    public static implicit operator Result<TValue>(Error error) =>
        new(error);

    public static implicit operator Result<TValue>(TValue value) =>
        new(value);

    public static Result<TValue> Success(TValue value) =>
        new(value);

    public static new Result<TValue> Failure(Error error) =>
        new(error);
}