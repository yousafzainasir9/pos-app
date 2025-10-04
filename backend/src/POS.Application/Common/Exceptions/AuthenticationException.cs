using POS.Application.Common.Constants;

namespace POS.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when authentication fails
/// </summary>
public class AuthenticationException : ApplicationException
{
    public AuthenticationException(string message, string errorCode) 
        : base(message, errorCode, 401) // HTTP 401 Unauthorized
    {
    }

    public static AuthenticationException InvalidCredentials() =>
        new AuthenticationException(ErrorMessages.InvalidCredentials, ErrorCodes.AUTH_INVALID_CREDENTIALS);

    public static AuthenticationException InvalidPin() =>
        new AuthenticationException(ErrorMessages.InvalidPin, ErrorCodes.AUTH_INVALID_PIN);

    public static AuthenticationException ExpiredToken() =>
        new AuthenticationException(ErrorMessages.ExpiredToken, ErrorCodes.AUTH_EXPIRED_TOKEN);

    public static AuthenticationException InvalidRefreshToken() =>
        new AuthenticationException(ErrorMessages.InvalidRefreshToken, ErrorCodes.AUTH_INVALID_REFRESH_TOKEN);

    public static AuthenticationException AccountLocked() =>
        new AuthenticationException(ErrorMessages.AccountLocked, ErrorCodes.AUTH_ACCOUNT_LOCKED);

    public static AuthenticationException AccountDisabled() =>
        new AuthenticationException(ErrorMessages.AccountDisabled, ErrorCodes.AUTH_ACCOUNT_DISABLED);
}
