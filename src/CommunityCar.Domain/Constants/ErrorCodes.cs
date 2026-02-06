namespace CommunityCar.Domain.Constants;

/// <summary>
/// Standardized error codes for API and application responses
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// Validation error - One or more validation rules failed (400)
    /// </summary>
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";

    /// <summary>
    /// Resource not found (404)
    /// </summary>
    public const string NOT_FOUND = "NOT_FOUND";

    /// <summary>
    /// User is not authenticated (401)
    /// </summary>
    public const string UNAUTHORIZED = "UNAUTHORIZED";

    /// <summary>
    /// User does not have permission to access resource (403)
    /// </summary>
    public const string FORBIDDEN = "FORBIDDEN";

    /// <summary>
    /// Duplicate entry - Resource already exists (409)
    /// </summary>
    public const string DUPLICATE_ENTRY = "DUPLICATE_ENTRY";

    /// <summary>
    /// Invalid login credentials (401)
    /// </summary>
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";

    /// <summary>
    /// Authentication token has expired (401)
    /// </summary>
    public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";

    /// <summary>
    /// Internal server error (500)
    /// </summary>
    public const string INTERNAL_ERROR = "INTERNAL_ERROR";
}
