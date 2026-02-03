using System.Diagnostics.CodeAnalysis;

namespace Volt.Core.Errors;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// Use this instead of exceptions for expected failure cases.
/// </summary>
public readonly struct Result
{
    private readonly VoltError? _error;

    private Result(VoltError? error)
    {
        _error = error;
    }

    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => _error is null;

    /// <summary>
    /// Whether the operation failed.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => _error is not null;

    /// <summary>
    /// The error if the operation failed; null otherwise.
    /// </summary>
    public VoltError? Error => _error;

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(null);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result Failure(VoltError error) => new(error);

    /// <summary>
    /// Creates a failed result with the specified error code and message.
    /// </summary>
    public static Result Failure(ErrorCode code, string message) =>
        new(VoltError.Create(code, message));

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    public static Result FromException(Exception exception, ErrorCode? code = null) =>
        new(VoltError.FromException(exception, code));

    /// <summary>
    /// Implicit conversion from VoltError to failed Result.
    /// </summary>
    public static implicit operator Result(VoltError error) => Failure(error);

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    public Result OnSuccess(Action action)
    {
        if (IsSuccess) action();
        return this;
    }

    /// <summary>
    /// Executes an action if the result is a failure.
    /// </summary>
    public Result OnFailure(Action<VoltError> action)
    {
        if (IsFailure) action(Error);
        return this;
    }

    /// <summary>
    /// Throws an exception if the result is a failure.
    /// </summary>
    public void ThrowIfFailure()
    {
        if (IsFailure)
        {
            throw new InvalidOperationException(Error.Message);
        }
    }
}

/// <summary>
/// Represents the result of an operation that returns a value or fails.
/// Use this instead of exceptions for expected failure cases.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly VoltError? _error;

    private Result(T value)
    {
        _value = value;
        _error = null;
    }

    private Result(VoltError error)
    {
        _value = default;
        _error = error;
    }

    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => _error is null;

    /// <summary>
    /// Whether the operation failed.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => _error is not null;

    /// <summary>
    /// The value if the operation succeeded.
    /// </summary>
    public T? Value => _value;

    /// <summary>
    /// The error if the operation failed; null otherwise.
    /// </summary>
    public VoltError? Error => _error;

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    public static Result<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result<T> Failure(VoltError error) => new(error);

    /// <summary>
    /// Creates a failed result with the specified error code and message.
    /// </summary>
    public static Result<T> Failure(ErrorCode code, string message) =>
        new(VoltError.Create(code, message));

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    public static Result<T> FromException(Exception exception, ErrorCode? code = null) =>
        new(VoltError.FromException(exception, code));

    /// <summary>
    /// Implicit conversion from T to successful Result.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicit conversion from VoltError to failed Result.
    /// </summary>
    public static implicit operator Result<T>(VoltError error) => Failure(error);

    /// <summary>
    /// Converts to a non-generic Result (discards the value).
    /// </summary>
    public Result ToResult() => IsSuccess ? Result.Success() : Result.Failure(Error);

    /// <summary>
    /// Gets the value or throws if failed.
    /// </summary>
    public T GetValueOrThrow()
    {
        if (IsFailure)
        {
            throw new InvalidOperationException(Error.Message);
        }
        return Value;
    }

    /// <summary>
    /// Gets the value or returns the specified default.
    /// </summary>
    public T GetValueOrDefault(T defaultValue) => IsSuccess ? Value : defaultValue;

    /// <summary>
    /// Gets the value or returns the result of the factory function.
    /// </summary>
    public T GetValueOrDefault(Func<VoltError, T> factory) =>
        IsSuccess ? Value : factory(Error!);

    /// <summary>
    /// Transforms the value if successful.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper) =>
        IsSuccess ? Result<TNew>.Success(mapper(Value)) : Result<TNew>.Failure(Error);

    /// <summary>
    /// Transforms the value if successful, or returns a new Result.
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder) =>
        IsSuccess ? binder(Value) : Result<TNew>.Failure(Error);

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess) action(Value);
        return this;
    }

    /// <summary>
    /// Executes an action if the result is a failure.
    /// </summary>
    public Result<T> OnFailure(Action<VoltError> action)
    {
        if (IsFailure) action(Error);
        return this;
    }

    /// <summary>
    /// Pattern matching: executes one of two functions based on success/failure.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<VoltError, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);
}
