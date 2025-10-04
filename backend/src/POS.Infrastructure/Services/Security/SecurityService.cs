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
    /// Hashes a token using SHA256
    /// </summary>
    public string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
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
    /// Generates a cryptographically secure random token
    /// </summary>
    public string GenerateSecureToken(int byteSize = 32)
    {
        var randomBytes = new byte[byteSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
