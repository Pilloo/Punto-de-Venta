using ErrorHandling;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    /// <summary>
    /// Represents an error that occurs when authentication fails due to invalid credentials.
    /// </summary>
    public record InvalidCredentials() : InternalError
    (
        Code: ErrorCodes.Unauthorized,
        Title: "Credenciales inválidas.",
        Detail: "Las credenciales ingresadas son inválidas. Por favor, revise los datos ingresados e intente de nuevo."
    );

    /// <summary>
    /// Represents a validation error containing detailed field-specific error messages.
    /// </summary>
    /// <param name="validationErrors">The <see cref="ModelStateDictionary"/> containing validation errors.</param>
    /// <remarks>
    /// <para>
    /// Transforms <see cref="ModelStateDictionary"/> errors into RFC 7807 compliant format.
    /// The <see cref="InternalError.Extensions"/> property contains detailed validation messages:
    /// </para>
    /// <para>
    /// Empty or null error messages are automatically filtered out.
    /// </para>
    /// <para>
    /// Corresponds to HTTP 400 Bad Request status code.
    /// </para>
    /// </remarks>
    public record ValidationFailed(ModelStateDictionary validationErrors) : InternalError
    (
        Code: ErrorCodes.BadRequest,
        Title: "Invalid request",
        Detail: "One or more validation errors occurred.",
        Extensions: new Dictionary<string, object>(
            validationErrors
                .Where(kvp => kvp.Value!.Errors.Any(e => !string.IsNullOrEmpty(e.ErrorMessage)))
                .Select(kvp => KeyValuePair.Create(
                    kvp.Key,
                    kvp.Value!.Errors.Count == 1
                        ? (object)kvp.Value.Errors[0].ErrorMessage!
                        : kvp.Value.Errors.Select(e => e.ErrorMessage!).ToList()
                ))
        ));
}
