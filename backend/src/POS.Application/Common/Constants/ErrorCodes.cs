namespace POS.Application.Common.Constants;

/// <summary>
/// Error codes and messages for the application
/// </summary>
public static class ErrorCodes
{
    // Authentication Errors (AUTH_xxx)
    public const string AUTH_INVALID_CREDENTIALS = "AUTH_001";
    public const string AUTH_INVALID_PIN = "AUTH_002";
    public const string AUTH_EXPIRED_TOKEN = "AUTH_003";
    public const string AUTH_INVALID_REFRESH_TOKEN = "AUTH_004";
    public const string AUTH_ACCOUNT_LOCKED = "AUTH_005";
    public const string AUTH_ACCOUNT_DISABLED = "AUTH_006";

    // Validation Errors (VAL_xxx)
    public const string VAL_REQUIRED_FIELD = "VAL_001";
    public const string VAL_INVALID_FORMAT = "VAL_002";
    public const string VAL_OUT_OF_RANGE = "VAL_003";

    // Business Logic Errors (BIZ_xxx)
    public const string BIZ_INSUFFICIENT_STOCK = "BIZ_001";
    public const string BIZ_SHIFT_ALREADY_OPEN = "BIZ_002";
    public const string BIZ_NO_ACTIVE_SHIFT = "BIZ_003";
    public const string BIZ_INVALID_PAYMENT_AMOUNT = "BIZ_004";

    // Resource Errors (RES_xxx)
    public const string RES_NOT_FOUND = "RES_001";
    public const string RES_ALREADY_EXISTS = "RES_002";
    public const string RES_CONFLICT = "RES_003";

    // System Errors (SYS_xxx)
    public const string SYS_INTERNAL_ERROR = "SYS_001";
    public const string SYS_DATABASE_ERROR = "SYS_002";
    public const string SYS_EXTERNAL_SERVICE_ERROR = "SYS_003";
}

public static class ErrorMessages
{
    // Authentication Messages
    public const string InvalidCredentials = "Invalid username or password";
    public const string InvalidPin = "Invalid PIN for this store";
    public const string ExpiredToken = "Token has expired";
    public const string InvalidRefreshToken = "Invalid or expired refresh token";
    public const string AccountLocked = "Account is locked due to too many failed login attempts";
    public const string AccountDisabled = "Account is disabled";

    // Generic Messages
    public const string InternalServerError = "An unexpected error occurred. Please try again later";
    public const string ValidationError = "Validation failed for the request";
    public const string NotFound = "The requested resource was not found";
    public const string Unauthorized = "You are not authorized to perform this action";
}
