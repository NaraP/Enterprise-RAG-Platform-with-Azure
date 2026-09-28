namespace RagPlatform.Domain.Common;

/// <summary>
/// Lightweight operation result used across Application handlers instead of throwing
/// for expected/business failures. Keeps controllers/handlers free of exception-driven flow.
/// </summary>
public class Result
{
    public bool Succeeded { get; }
    public IReadOnlyList<string> Errors { get; }

    protected Result(bool succeeded, IEnumerable<string>? errors = null)
    {
        Succeeded = succeeded;
        Errors = errors?.ToList() ?? new List<string>();
    }

    public static Result Success() => new(true);
    public static Result Failure(params string[] errors) => new(false, errors);
}

public class Result<T> : Result
{
    public T? Value { get; }

    protected Result(bool succeeded, T? value, IEnumerable<string>? errors = null)
        : base(succeeded, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value);
    public new static Result<T> Failure(params string[] errors) => new(false, default, errors);
}
