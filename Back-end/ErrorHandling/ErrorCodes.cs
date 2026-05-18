namespace ErrorHandling;

/**
 * <summary>Enum that contains all the most typically used HTTP return codes.</summary>
 */
public enum ErrorCodes : int
{
    // Bad Request
    BadRequest = 400,

    // Token is expired, missing, or signature is invalid. 
    Unauthorized = 401,

    // Token is valid, but the account is disabled or locked.
    AccountLocked = 403,

    // Specific record missing.
    NotFound = 404,

    // Resource conflict.
    Conflict = 409,

    // Validation failures.
    ValidationError = 422,

    // Rate limiting for the Auth service.
    TooManyRequests = 429,

    // Standard unhandled exception.
    InternalError = 500,

    // Communication failure between services.
    ServiceUnavailable = 503,

    // Operation cancelled by the user.
    OperationCancelled = 499
}
