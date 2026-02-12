namespace OrdemCerta.Shared;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; private set; }
    public List<string> Errors { get; private set; } = new();

    protected Result(T value)
    {
        Value = value;
        IsSuccess = true;
    }

    protected Result(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or empty", nameof(errorMessage));
            
        Errors.Add(errorMessage);
        IsSuccess = false;
        Value = default;
    }
    
    protected Result(List<string> errors)
    {
        Errors = errors;
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
    
    public static Result<T> Failure(List<string> errors)
    {
        return new Result<T>(errors);
    }

    public static implicit operator Result<T>(T value) => Success(value);
    
    public static implicit operator Result<T>(string errorMessage) => Failure(errorMessage);
}

public class Result
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public List<string> Errors { get; private set; } = new();

    protected Result()
    {
        IsSuccess = true;
    }

    protected Result(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or empty", nameof(errorMessage));
            
        Errors.Add(errorMessage);
        IsSuccess = false;
    }
    
    protected Result(List<string> errors)
    {
        Errors = errors;
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
    
    public static Result Failure(List<string> errors)
    {
        return new Result(errors);
    }
    
    public static implicit operator Result(string errorMessage) => Failure(errorMessage);
}