namespace UnameIT.RevenueIntelligence.Domain.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(T value) { IsSuccess = true; Value = value; Errors = []; }
    private Result(string error) { IsSuccess = false; Error = error; Errors = [error]; }
    private Result(IReadOnlyList<string> errors) { IsSuccess = false; Errors = errors; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);
    public static Result<T> Failure(IReadOnlyList<string> errors) => new(errors);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result() { IsSuccess = true; Errors = []; }
    private Result(string error) { IsSuccess = false; Error = error; Errors = [error]; }

    public static Result Success() => new();
    public static Result Failure(string error) => new(error);
}
