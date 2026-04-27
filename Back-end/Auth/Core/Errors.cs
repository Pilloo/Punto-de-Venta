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
    /// Represents a validation failure error that contains details about one or more validation errors associated with
    /// specific fields or properties.
    /// </summary>
    /// <remarks>This record is typically used to convey validation errors resulting from model binding or
    /// identity operations. The errors are structured to allow clients to associate error messages with specific input
    /// fields, facilitating user feedback and error handling in client applications.</remarks>
    /// <param name="errors">A dictionary containing validation errors, where each key is the name of the field or property with an error,
    /// and the value is either a single error message or a list of error messages for that field.</param>
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
    /// Represents an error message indicating that the specified user was not found.
    /// </summary>
    /// <remarks>Use this error type to signal that a user lookup operation did not locate a matching user.
    /// This message is typically returned when search parameters do not correspond to any existing user
    /// records.</remarks>
    public record UserNotFound() : ErrorMessage(
        Code: ErrorCodes.NotFound,
        Title: "Usuario no encontrado.",
        Detail: "El usuario solicitado no fue encontrado. Revise los parámetros de búsqueda e intente de nuevo."
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

    /// <summary>
    /// Represents an error indicating that a server-side operation was cancelled or did not complete within the expected timeframe.
    /// </summary>
    /// <remarks>This error is typically returned when an operation times out, is interrupted, or is explicitly cancelled on the server.
    /// Common causes include request timeouts during long-running operations or when the server terminates a request due to resource constraints
    /// or system shutdown. Clients may retry the operation, but should implement appropriate backoff strategies to avoid overwhelming the server.</remarks>
    public record OperationCanceled() : ErrorMessage(
        Code: ErrorCodes.InternalError,
        Title: "Operación cancelada.",
        Detail: "La operación fue cancelada. Si estimase que se trata de un error, intente realizar la operación nuevamente."
    );
}
