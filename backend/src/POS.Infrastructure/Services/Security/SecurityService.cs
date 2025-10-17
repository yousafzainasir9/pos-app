using POS.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace POS.Infrastructure.Services.Security;

/// <summary>
/// Implementation of security-related operations
/// </summary>
public class SecurityService : ISecurityService
{
    /// <summary>
    /// Hashes a token using SHA256 and returns hex string
    /// </summary>
    public string HashToken(string token)
    {
        if (token == null)
            throw new ArgumentNullException(nameof(token));

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        
        // Convert to hex string (64 characters for SHA256)
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Verifies a token against its hash
    /// </summary>
    public bool VerifyToken(string token, string hash)
    {
        var tokenHash = HashToken(token);
        return tokenHash == hash;
    }

    /// <summary>
    /// Generates a cryptographically secure random token (URL-safe Base64)
    /// </summary>
    public string GenerateSecureToken(int byteSize = 32)
    {
        if (byteSize < 0)
            return string.Empty;

        if (byteSize == 0)
            return string.Empty;

        var randomBytes = new byte[byteSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        
        // Use URL-safe Base64: replace + with -, / with _, and remove padding =
        return Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
