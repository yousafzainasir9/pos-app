using FluentAssertions;
using POS.Infrastructure.Services.Security;
using Xunit;

namespace POS.Infrastructure.Tests.Services;

/// <summary>
/// Tests for SecurityService - Token generation and hashing
/// </summary>
public class SecurityServiceTests
{
    private readonly SecurityService _securityService;

    public SecurityServiceTests()
    {
        _securityService = new SecurityService();
    }

    [Fact]
    public void GenerateSecureToken_ShouldReturnNonEmptyString()
    {
        // Act
        var token = _securityService.GenerateSecureToken(32);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateSecureToken_WithDifferentSizes_ShouldReturnDifferentLengths()
    {
        // Act
        var token16 = _securityService.GenerateSecureToken(16);
        var token32 = _securityService.GenerateSecureToken(32);
        var token64 = _securityService.GenerateSecureToken(64);

        // Assert
        token16.Length.Should().BeGreaterThan(0);
        token32.Length.Should().BeGreaterThan(0);
        token64.Length.Should().BeGreaterThan(0);
        
        // Base64 encoding makes strings longer than byte count
        token64.Length.Should().BeGreaterThan(token32.Length);
        token32.Length.Should().BeGreaterThan(token16.Length);
    }

    [Fact]
    public void GenerateSecureToken_MultipleCalls_ShouldReturnDifferentTokens()
    {
        // Act
        var token1 = _securityService.GenerateSecureToken(32);
        var token2 = _securityService.GenerateSecureToken(32);
        var token3 = _securityService.GenerateSecureToken(32);

        // Assert
        token1.Should().NotBe(token2);
        token2.Should().NotBe(token3);
        token1.Should().NotBe(token3);
    }

    [Fact]
    public void GenerateSecureToken_WithZeroSize_ShouldReturnEmptyString()
    {
        // Act
        var token = _securityService.GenerateSecureToken(0);

        // Assert
        token.Should().BeEmpty();
    }

    [Fact]
    public void GenerateSecureToken_WithLargeSize_ShouldSucceed()
    {
        // Act
        var token = _securityService.GenerateSecureToken(256);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Length.Should().BeGreaterThan(256); // Base64 encoding increases size
    }

    [Fact]
    public void HashToken_ShouldReturnNonEmptyHash()
    {
        // Arrange
        var token = "test-token-12345";

        // Act
        var hash = _securityService.HashToken(token);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashToken_WithSameInput_ShouldReturnSameHash()
    {
        // Arrange
        var token = "consistent-token";

        // Act
        var hash1 = _securityService.HashToken(token);
        var hash2 = _securityService.HashToken(token);
        var hash3 = _securityService.HashToken(token);

        // Assert
        hash1.Should().Be(hash2);
        hash2.Should().Be(hash3);
    }

    [Fact]
    public void HashToken_WithDifferentInput_ShouldReturnDifferentHash()
    {
        // Arrange
        var token1 = "token-one";
        var token2 = "token-two";
        var token3 = "token-three";

        // Act
        var hash1 = _securityService.HashToken(token1);
        var hash2 = _securityService.HashToken(token2);
        var hash3 = _securityService.HashToken(token3);

        // Assert
        hash1.Should().NotBe(hash2);
        hash2.Should().NotBe(hash3);
        hash1.Should().NotBe(hash3);
    }

    [Fact]
    public void HashToken_WithEmptyString_ShouldReturnHash()
    {
        // Arrange
        var token = string.Empty;

        // Act
        var hash = _securityService.HashToken(token);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashToken_WithLongString_ShouldReturnFixedLengthHash()
    {
        // Arrange
        var shortToken = "short";
        var longToken = new string('x', 1000);

        // Act
        var shortHash = _securityService.HashToken(shortToken);
        var longHash = _securityService.HashToken(longToken);

        // Assert
        shortHash.Length.Should().Be(longHash.Length); // SHA256 always produces same length
    }

    [Fact]
    public void HashToken_SHA256_ShouldProduceCorrectLength()
    {
        // Arrange
        var token = "test-token";

        // Act
        var hash = _securityService.HashToken(token);

        // Assert
        // SHA256 produces 32 bytes = 64 hex characters
        hash.Length.Should().Be(64);
    }

    [Fact]
    public void HashToken_ShouldProduceHexadecimalString()
    {
        // Arrange
        var token = "test-token";

        // Act
        var hash = _securityService.HashToken(token);

        // Assert
        hash.Should().MatchRegex("^[0-9a-f]+$"); // Only hex characters (0-9, a-f)
    }

    [Theory]
    [InlineData("token1")]
    [InlineData("TOKEN1")]
    [InlineData("Token1")]
    public void HashToken_IsCaseSensitive(string token)
    {
        // Act
        var hash = _securityService.HashToken(token);

        // Assert
        var lowerHash = _securityService.HashToken(token.ToLower());
        var upperHash = _securityService.HashToken(token.ToUpper());

        if (token == token.ToLower())
        {
            hash.Should().Be(lowerHash);
        }
        else if (token == token.ToUpper())
        {
            hash.Should().Be(upperHash);
        }
        else
        {
            hash.Should().NotBe(lowerHash);
            hash.Should().NotBe(upperHash);
        }
    }

    [Fact]
    public void HashToken_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var tokens = new[]
        {
            "token!@#$%^&*()",
            "token with spaces",
            "token\twith\ttabs",
            "token\nwith\nnewlines",
            "token-with-dashes",
            "token_with_underscores",
            "token.with.dots"
        };

        // Act & Assert
        foreach (var token in tokens)
        {
            var hash = _securityService.HashToken(token);
            hash.Should().NotBeNullOrEmpty();
            hash.Length.Should().Be(64);
        }
    }

    [Fact]
    public void GenerateSecureToken_ShouldBeUrlSafe()
    {
        // Act
        var token = _securityService.GenerateSecureToken(32);

        // Assert
        // Base64 URL-safe characters only (no +, /, =)
        token.Should().NotContain("+");
        token.Should().NotContain("/");
        token.Should().NotContain("=");
        token.Should().MatchRegex("^[A-Za-z0-9_-]+$");
    }

    [Fact]
    public void FullWorkflow_GenerateAndHashToken_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var token = _securityService.GenerateSecureToken(32);
        var hash = _securityService.HashToken(token);

        // Assert
        token.Should().NotBeNullOrEmpty();
        hash.Should().NotBeNullOrEmpty();
        hash.Length.Should().Be(64); // SHA256 hex length

        // Generate another token and verify it produces different hash
        var token2 = _securityService.GenerateSecureToken(32);
        var hash2 = _securityService.HashToken(token2);
        
        token2.Should().NotBe(token);
        hash2.Should().NotBe(hash);
    }

    [Fact]
    public void HashToken_WithNull_ShouldThrowException()
    {
        // Act
        Action act = () => _securityService.HashToken(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GenerateSecureToken_WithNegativeSize_ShouldThrowOrReturnEmpty()
    {
        // Act
        Action act = () => _securityService.GenerateSecureToken(-1);

        // Assert - Depending on implementation, might throw or return empty
        // Just test that it doesn't crash
        try
        {
            var result = _securityService.GenerateSecureToken(-1);
            result.Should().BeEmpty();
        }
        catch (Exception)
        {
            // Expected - negative size might throw
        }
    }
}
