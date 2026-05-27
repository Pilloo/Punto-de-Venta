namespace ErrorHandling;

/**
 * <summary>Enum that contains all the most typically used HTTP return codes.</summary>
 */
public enum ErrorCodes : int
{
    BadRequest = 400,

    Unauthorized = 401,

    Forbidden = 403,

    NotFound = 404,

    Conflict = 409,

    ValidationError = 422,

    TooManyRequests = 429,

    InternalError = 500,

    ServiceUnavailable = 503,

    OperationCancelled = 499
}
