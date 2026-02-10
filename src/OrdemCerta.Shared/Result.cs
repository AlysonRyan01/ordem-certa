namespace OrdemCerta.Shared;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; private set; }
    public string? ErrorMessage { get; private set; }

    protected Result(T value)
    {
        Value = value;
        IsSuccess = true;
        ErrorMessage = null;
    }

    protected Result(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or empty", nameof(errorMessage));
            
        ErrorMessage = errorMessage;
        IsSuccess = false;
        Value = default;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    public static Result<T> Failure(string errorMessage)
    {
        return new Result<T>(errorMessage);
    }

    public static implicit operator Result<T>(T value) => Success(value);
    
    public static implicit operator Result<T>(string errorMessage) => Failure(errorMessage);
}

public class Result
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; private set; }

    protected Result()
    {
        IsSuccess = true;
        ErrorMessage = null;
    }

    protected Result(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or empty", nameof(errorMessage));
            
        ErrorMessage = errorMessage;
        IsSuccess = false;
    }
    
    public static Result Success()
    {
        return new Result();
    }

    public static Result Failure(string errorMessage)
    {
        return new Result(errorMessage);
    }
    
    public static implicit operator Result(string errorMessage) => Failure(errorMessage);
}