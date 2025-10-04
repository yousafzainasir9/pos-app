namespace POS.Application.Interfaces;

/// <summary>
/// Interface for security-related operations
/// </summary>
public interface ISecurityService
{
    /// <summary>
    /// Hashes a refresh token using SHA256
    /// </summary>
    string HashToken(string token);

    /// <summary>
    /// Verifies a token against its hash
    /// </summary>
    bool VerifyToken(string token, string hash);

    /// <summary>
    /// Generates a cryptographically secure random token
    /// </summary>
    string GenerateSecureToken(int byteSize = 32);
}
