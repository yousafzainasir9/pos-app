namespace POS.Application.Common.Constants;

/// <summary>
/// Authentication and authorization related constants
/// </summary>
public static class AuthConstants
{
    /// <summary>
    /// Size of refresh token in bytes (used for secure random generation)
    /// </summary>
    public const int RefreshTokenByteSize = 32;

    /// <summary>
    /// Access token expiry time in minutes
    /// </summary>
    public const int AccessTokenExpiryMinutes = 60;

    /// <summary>
    /// Refresh token expiry time in days
    /// </summary>
    public const int RefreshTokenExpiryDays = 7;

    /// <summary>
    /// Maximum login attempts before account lockout
    /// </summary>
    public const int MaxLoginAttempts = 5;

    /// <summary>
    /// Account lockout duration in minutes
    /// </summary>
    public const int LockoutDurationMinutes = 15;

    /// <summary>
    /// Minimum password length requirement
    /// </summary>
    public const int MinPasswordLength = 8;

    /// <summary>
    /// Maximum password length
    /// </summary>
    public const int MaxPasswordLength = 128;

    /// <summary>
    /// PIN length requirement
    /// </summary>
    public const int PinLength = 4;
}
