using Microsoft.AspNetCore.Mvc;

namespace ErrorHandling;

/// <summary>
/// Represents a standardized error structure for internal application errors,
/// compatible with RFC 7807 (Problem Details for HTTP APIs).
/// </summary>
/// <remarks>
/// <para>
/// This record type serves as the base for all domain-specific errors in the application.
/// It can be converted to <see cref="ProblemDetails"/> for HTTP responses.
/// </para>
/// </remarks>
public abstract record ErrorMessage
{
    /// <summary>
    /// Gets the error classification code.
    /// </summary>
    /// <value>One of the standard <see cref="ErrorCodes"/> values.</value>
    public ErrorCodes Code { get; init; }

    /// <summary>
    /// Gets the human-readable error summary.
    /// </summary>
    /// <value>A brief description of the error.</value>
    public string Title { get; init; }

    /// <summary>
    /// Gets additional error details (optional).
    /// </summary>
    /// <value>More specific information about the error, or <c>null</c>.</value>
    public string? Detail { get; init; }

    /// <summary>
    /// Gets additional error metadata (optional).
    /// </summary>
    /// <value>
    /// A dictionary of extended error information, or <c>null</c>.
    /// These extensions will be included when converting to <see cref="ProblemDetails"/>.
    /// </value>
    public Dictionary<string, object>? Extensions { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorMessage"/> record.
    /// </summary>
    /// <param name="Code">The error category from <see cref="ErrorCodes"/>.</param>
    /// <param name="Title">A human-readable summary of the error.</param>
    /// <param name="Detail">Additional details about the error (optional).</param>
    /// <param name="Extensions">Additional error metadata as key-value pairs (optional).</param>
    protected ErrorMessage(
        ErrorCodes Code,
        string Title,
        string? Detail = null,
        Dictionary<string, object>? Extensions = null)
    {
        this.Code = Code;
        this.Title = Title;
        this.Detail = Detail;
        this.Extensions = Extensions;
    }
}
