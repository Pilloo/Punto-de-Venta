using ErrorHandling;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Inventory.Core;

/// <summary>
/// Represents a validation failure error that contains details about one or more validation errors associated with
/// specific fields or properties.
/// </summary>
/// <remarks>This record is typically used to convey validation errors resulting from model binding or
/// identity operations. The errors are structured to allow clients to associate error messages with specific input
/// fields, facilitating user feedback and error handling in client applications.</remarks>
/// <param name="Errors">A dictionary containing validation errors, where each key is the name of the field or property with an error,
/// and the value is either a single error message or a list of error messages for that field.</param>
public record ValidationFailed(Dictionary<string, object> Errors) : ErrorMessage
(
    Code: HttpCodes.BadRequest,
    Title: "Solicitud inválida.",
    Detail: "Uno o más errores de validación ocurrieron. Verifique los datos ingresados e intente de nuevo.",
    Extensions: Errors
)
{
    // For ModelStateDictionary (validation errors caught on controllers).
    public ValidationFailed(ModelStateDictionary validationErrors) : this(
        validationErrors.Where(kvp => kvp.Value!.Errors.Any(e => !string.IsNullOrEmpty(e.ErrorMessage)))
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Count == 1
                                ? (object)kvp.Value!.Errors[0].ErrorMessage
                                : kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        )
    )
    {
    }

    /// <summary>A dictionary containing validation errors, where each key is the name of the field or property with an error,
    /// and the value is either a single error message or a list of error messages for that field.</summary>
    public Dictionary<string, object> Errors { get; init; } = Errors;
};

/// <summary>
/// Represents an error message indicating that the provided refresh token is invalid or does not exist.
/// </summary>
/// <remarks>This record is typically used to signal authentication failures where the user's session or
/// credentials are missing or have expired. Use this error to prompt the user to re-authenticate.</remarks>
public record InvalidOrNonExistingToken() : ErrorMessage(
    Code: HttpCodes.ValidationError,
    Title: "Información de acceso inválida o no existente",
    Detail: "La información de acceso es inválida o no existente. Por favor, inicie sesión nuevamente para continuar."
);

/// <summary>
/// Represents a server-side error that occurs when processing a request fails due to an internal issue.
/// </summary>
/// <remarks>This error indicates an unexpected condition on the server. The client may retry the
/// operation, but if the error persists, further investigation may be required. This type is typically used to
/// signal generic server errors that are not exposed in detail to the client.</remarks>
public record InternalError() : ErrorMessage(
    Code: HttpCodes.InternalError,
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
    Code: HttpCodes.Ok,
    Title: "Operación cancelada.",
    Detail:
    "La operación fue cancelada. Si estimase que se trata de un error, intente realizar la operación nuevamente."
);

/// <summary>
/// Represents an error message indicating that the requested operation is not authorized due to insufficient
/// permissions.
/// </summary>
/// <remarks>Use this type to signal that an operation was denied because the caller lacks the necessary
/// credentials or authorization. The error includes a code, title, and detail message describing the unauthorized
/// access.</remarks>
public record UnauthorizedOperation() : ErrorMessage(
    Code: HttpCodes.Unauthorized,
    Title: "Operación no autorizada.",
    Detail: "No cuenta con los permisos para realizar la acción solicitada. Revise sus credenciales e intente de nuevo."
);

/// <summary>
/// Represents an error response indicating that the provided entity already exists in the system.
/// </summary>
/// <remarks>
/// This record is typically used to convey conflict errors where an attempt to create or update an entity
/// fails because an identical entity already exists in the system. It assists in preventing duplication of data
/// and ensures system integrity.
/// </remarks>
public record DuplicatedEntity() : ErrorMessage(
    Code: HttpCodes.Conflict,
    Title: "Elemento duplicado.",
    Detail:
    "El elemento ingresado ya existe en el sistema. Por favor, verifique los datos ingresados e intente de nuevo."
);

/// <summary>
/// Represents an error indicating that the requested information could not be found in the system.
/// </summary>
/// <remarks>
/// This record is typically used in cases where a query or operation is performed, but the target resource
/// does not exist or cannot be located. It allows for standardized error communication, with details
/// about the issue and guidance to check the input data for accuracy. This error response
/// is commonly associated with HTTP 404 (Not Found) status codes.
/// </remarks>
public record NotFound() : ErrorMessage(
    Code: HttpCodes.NotFound,
    Title: "Información no encontrada.",
    Detail:
    "La información solicitada no se encuentra en el sistema. Por favor, verifique los datos ingresados e intente de nuevo."
);

/// <summary>
/// Represents an error that occurs when there is insufficient stock available in the inventory to fulfill a requested quantity of products.
/// </summary>
/// <remarks>
/// This error is typically used in operations involving stock management, where a client attempts to purchase
/// or reserve a quantity of items that exceeds the available stock in the system. It provides feedback about the
/// inventory insufficiency to help inform user actions or system workflows.
/// </remarks>
/// <param name="RequestedQuantity">The quantity of the product requested that could not be fulfilled due to insufficient stock.</param>
public record InsufficientStock(int RequestedQuantity) : ErrorMessage(
    Code: HttpCodes.BadRequest,
    Title: "Existencias insuficientes.",
    Detail:
    $"No hay suficientes existencias de productos en el inventario para satisfacer la solicitud de {RequestedQuantity} unidades."
)
{
    /// <summary>The quantity of the product requested that could not be fulfilled due to insufficient stock.</summary>
    public int RequestedQuantity { get; init; } = RequestedQuantity;
}

/// <summary>
/// Represents an exception that occurs when a database constraint is violated.
/// </summary>
/// <remarks>
/// This exception is used to indicate that an operation has failed due to a violation of a database constraint,
/// such as a foreign key, uniqueness, or check constraint. It provides information about the specific constraint
/// that was violated.
/// </remarks>
/// <param name="ConstraintName">The name of the database constraint that was violated.</param>
public record ConstraintViolationException(string ConstraintName) : ErrorMessage(
    Code: HttpCodes.ValidationError,
    Title: "Entidad referenciada no encontrada.",
    Detail: $"Ha sucedido un problema al referenciar entidades en el sistema. Restricción violada: {ConstraintName}"
    )
{
    public string ConstraintName { get; init; } = ConstraintName;
}