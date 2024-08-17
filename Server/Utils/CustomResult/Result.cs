namespace Server.Utils.CustomResult;

public class Result<T>
{
    private readonly T? _value;

    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
        Error = Errors.Error.None;
    }

    private Result(Errors.Error error)
    {
        if (error == Errors.Error.None)
            throw new ArgumentException("invalid error", nameof(error));

        IsSuccess = false;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T Value
    {
        get
        {
            if (IsFailure)
                throw new InvalidOperationException("there is no value for failure");
            return _value!;
        }

        private init => _value = value;
    }

    public Errors.Error Error { get; }

    public static Result<T> Success(T value) => new(value);
    
    public static Result<T> Failure(Errors.Error error) => new(error);
}