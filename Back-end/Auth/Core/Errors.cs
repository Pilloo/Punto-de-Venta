using ErrorHandling;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    /// <summary>
    /// Represents an error that occurs when authentication fails due to invalid credentials.
    /// </summary>
    public record InvalidCredentials() : ErrorMessage
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
    /// The <see cref="ErrorMessage.Extensions"/> property contains detailed validation messages:
    /// </para>
    /// <para>
    /// Empty or null error messages are automatically filtered out.
    /// </para>
    /// <para>
    /// Corresponds to HTTP 400 Bad Request status code.
    /// </para>
    /// </remarks>
    public record ValidationFailed(Dictionary<string, object> errors) : ErrorMessage
    (
        Code: ErrorCodes.BadRequest,
        Title: "Solicitud inválida.",
        Detail: "Uno o más errores de validación ocurrieron. Verifique los datos ingresados e intente de nuevo.",
        Extensions: errors
    )
    {
        // For ModelStateDictionary (validation errors caught on controllers).
        public ValidationFailed(ModelStateDictionary validationErrors) : this(
            validationErrors.Where(kvp => kvp.Value!.Errors.Any(e => !string.IsNullOrEmpty(e.ErrorMessage)))
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value!.Errors.Count == 1
                                       ? (object)kvp.Value!.Errors[0].ErrorMessage!
                                       : kvp.Value.Errors.Select(e => e.ErrorMessage!).ToList()
                            )
            )
        { }

        // For IdentityErrors (validation errors caught on user creation).
        public ValidationFailed(IEnumerable<IdentityError> validationErrors) : this(
            validationErrors.GroupBy(e => e.Code)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Count() == 1 ? (object)g.First().Description : g.Select(e => e.Description).ToList()
                            )
            )
        { }
    };

    /// <summary>
    /// Represents an error indicating that the specified user name is already in use.
    /// </summary>
    /// <remarks>This error is typically returned when attempting to register or update a user account with a
    /// user name that is not available. Use this type to signal a conflict when enforcing unique user names.</remarks>
    public record UserNameAlreadyTaken() : ErrorMessage(
        Code: ErrorCodes.Conflict,
        Title: "Nombre de usuario en uso.",
        Detail: "El nombre de usuario ingresado no se encuentra disponible. Ingrese un nombre de usuario distinto e intente de nuevo."
    );

    /// <summary>
    /// Represents an error message indicating that the provided refresh token is invalid or does not exist.
    /// </summary>
    /// <remarks>This record is typically used to signal authentication failures where the user's session or
    /// credentials are missing or have expired. Use this error to prompt the user to re-authenticate.</remarks>
    public record InvalidOrNonExistingToken() : ErrorMessage(
        Code: ErrorCodes.ValidationError,
        Title: "Información de acceso inválida o no existente",
        Detail: "La información de acceso es inválida o no existente. Por favor, ingrese sesión nuevamente para continuar."
    );

    /// <summary>
    /// Represents a server-side error that occurs when processing a request fails due to an internal issue.
    /// </summary>
    /// <remarks>This error indicates an unexpected condition on the server. The client may retry the
    /// operation, but if the error persists, further investigation may be required. This type is typically used to
    /// signal generic server errors that are not exposed in detail to the client.</remarks>
    public record InternalError() : ErrorMessage(
        Code: ErrorCodes.InternalError,
        Title: "Error de servidor.",
        Detail: "Ha sucedido un error al procesar su solicitud. Por favor, intente la operación en un momento."
    );
}
