using Microsoft.AspNetCore.Mvc;

namespace ErrorHandling;

/// <summary>
/// Represents the outcome of an operation that can either succeed with a value of type <typeparamref name="T"/>
/// or fail with a structured <see cref="ProblemDetails"/> error.
/// </summary>
/// <typeparam name="T">The type of the value returned when the operation succeeds.</typeparam>
/// <remarks>
/// <para>
/// This implementation follows the Result pattern for explicit error handling, using RFC 7807 
/// (<see cref="ProblemDetails"/>) for standardized error responses.
/// </para>
/// </remarks>
public class Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    /// <value>
    /// <c>true</c> if the operation succeeded; <c>false</c> if it failed.
    /// </value>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the RFC 7807 compliant error details when the operation fails.
    /// </summary>
    /// <value>
    /// A <see cref="ProblemDetails"/> object containing error information when <see cref="IsSuccess"/> is <c>false</c>.
    /// <c>null</c> when the operation succeeds.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown if accessed when <see cref="IsSuccess"/> is <c>true</c>.
    /// </exception>
    public ProblemDetails? Error { get; }

    /// <summary>
    /// Gets the result value when the operation succeeds.
    /// </summary>
    /// <value>
    /// The operation result value when <see cref="IsSuccess"/> is <c>true</c>.
    /// Default value of <typeparamref name="T"/> when failed (use null-forgiving operator if needed).
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown if accessed when <see cref="IsSuccess"/> is <c>false</c>.
    /// </exception>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <param name="isSuccess">The success status.</param>
    /// <param name="error">The error details.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when:
    /// <list type="bullet">
    /// <item><description>A success result contains an error object</description></item>
    /// <item><description>A failure result has no error object</description></item>
    /// </list>
    /// </exception>
    private Result(T value, bool isSuccess, ProblemDetails? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("Success result can't have an error.");

        if (!isSuccess && error is null)
            throw new InvalidOperationException("Failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
        Value = value;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A successful <see cref="Result{T}"/> instance.</returns>
    public static Result<T> Success(T value) => new(value, true, null);

    /// <summary>
    /// Creates a failed result with the specified <see cref="ProblemDetails"/> error.
    /// </summary>
    /// <param name="error">The RFC 7807 compliant error details.</param>
    /// <returns>A failed <see cref="Result{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="error"/> is <c>null</c>.
    /// </exception>
    public static Result<T> Failure(ProblemDetails error)
    {
        if (error == null) throw new ArgumentNullException(nameof(error));
        return new Result<T>(default!, false, error);
    }
}