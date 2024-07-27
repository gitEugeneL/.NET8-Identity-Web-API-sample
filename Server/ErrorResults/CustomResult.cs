namespace Server.ErrorResults;

public class CustomResult<T>
{
    private readonly T? _value;
    
    public T? Value
    {
        get
        {
            if (IsFailure)
                throw new InvalidOperationException("there is no value for failure");
            return _value;
        }
    }
    
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    
    public CustomError? Error { get; }

    private CustomResult(T value)
    {
        _value = value;
        IsSuccess = true;
        Error = CustomError.None;
    }

    private CustomResult(CustomError error)
    {
        if (error == CustomError.None)
            throw new ArgumentException("Invalid error message");

        IsSuccess = false;
        Error = error;
    }

    public static CustomResult<T> Success(T value) => new(value);

    public static CustomResult<T> Failure(CustomError error) => new(error);
}