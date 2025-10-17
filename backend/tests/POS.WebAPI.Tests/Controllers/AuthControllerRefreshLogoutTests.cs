using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using POS.Application.Common.Interfaces;
using POS.Application.Common.Models;
using POS.Application.DTOs.Auth;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Domain.Entities.Audit;
using POS.Domain.Enums;
using POS.Infrastructure.Data;
using POS.Infrastructure.Repositories;

using POS.WebAPI.Controllers;
using POS.WebAPI.Tests.Helpers;
using System.Security.Claims;
using Xunit;

namespace POS.WebAPI.Tests.Controllers;

/// <summary>
/// Tests for AuthController - Refresh token and logout functionality
/// </summary>
public class AuthControllerRefreshLogoutTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ISecurityService> _mockSecurityService;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;
    private readonly UnitOfWork _unitOfWork;

    public AuthControllerRefreshLogoutTests()
    {
        _context = InMemoryDbContextFactory.CreateWithData();
        _unitOfWork = new UnitOfWork(_context);
        
        _mockConfiguration = new Mock<IConfiguration>();
        _mockSecurityService = new Mock<ISecurityService>();
        _mockAuditService = new Mock<IAuditService>();
        _mockLogger = new Mock<ILogger<AuthController>>();

        // Setup JWT configuration
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(s => s["SecretKey"]).Returns("test-secret-key-that-is-at-least-32-characters-long-for-security-purposes");
        jwtSection.Setup(s => s["Issuer"]).Returns("TestIssuer");
        jwtSection.Setup(s => s["Audience"]).Returns("TestAudience");
        _mockConfiguration.Setup(c => c.GetSection("Jwt")).Returns(jwtSection.Object);

        _controller = new AuthController(
            _unitOfWork,
            _mockConfiguration.Object,
            _mockLogger.Object,
            _mockSecurityService.Object,
            _mockAuditService.Object
        );
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var user = new User
        {
            Id = 300,
            Username = "refreshtest",
            Email = "refresh@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Refresh",
            LastName = "Test",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1,
            RefreshToken = "stored-hashed-token",
            RefreshTokenExpiryTime = DateTime.Now.AddDays(7)
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = "plain-refresh-token"
        };

        _mockSecurityService.Setup(s => s.HashToken("plain-refresh-token"))
            .Returns("stored-hashed-token");
        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("new-refresh-token");
        _mockSecurityService.Setup(s => s.HashToken("new-refresh-token"))
            .Returns("new-hashed-token");

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<LoginResponseDto>;
        
        response!.Success.Should().BeTrue();
        response.Data!.Token.Should().NotBeNullOrEmpty();
        response.Data.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldThrowException()
    {
        // Arrange
        var request = new RefreshTokenRequestDto
        {
            RefreshToken = "invalid-token"
        };

        _mockSecurityService.Setup(s => s.HashToken("invalid-token"))
            .Returns("non-existent-hash");

        // Act
        Func<Task> act = async () => await _controller.RefreshToken(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ShouldThrowException()
    {
        // Arrange
        var user = new User
        {
            Id = 301,
            Username = "expiredrefresh",
            Email = "expiredrefresh@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Expired",
            LastName = "Refresh",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1,
            RefreshToken = "stored-hashed-token",
            RefreshTokenExpiryTime = DateTime.Now.AddDays(-1) // Expired
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = "plain-token"
        };

        _mockSecurityService.Setup(s => s.HashToken("plain-token"))
            .Returns("stored-hashed-token");

        // Act
        Func<Task> act = async () => await _controller.RefreshToken(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task RefreshToken_ShouldGenerateNewRefreshToken()
    {
        // Arrange
        var user = new User
        {
            Id = 302,
            Username = "newrefresh",
            Email = "newrefresh@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "New",
            LastName = "Refresh",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1,
            RefreshToken = "old-hashed-token",
            RefreshTokenExpiryTime = DateTime.Now.AddDays(7)
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = "old-plain-token"
        };

        _mockSecurityService.Setup(s => s.HashToken("old-plain-token"))
            .Returns("old-hashed-token");
        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("brand-new-token");
        _mockSecurityService.Setup(s => s.HashToken("brand-new-token"))
            .Returns("brand-new-hashed-token");

        // Act
        await _controller.RefreshToken(request);

        // Assert
        _context.ChangeTracker.Clear();
        var updatedUser = await _unitOfWork.Repository<User>().GetByIdAsync(user.Id);
        updatedUser!.RefreshToken.Should().Be("brand-new-hashed-token");
        updatedUser.RefreshToken.Should().NotBe("old-hashed-token");
    }

    [Fact]
    public async Task RefreshToken_ShouldUpdateRefreshTokenExpiry()
    {
        // Arrange
        var user = new User
        {
            Id = 303,
            Username = "updateexpiry",
            Email = "updateexpiry@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Update",
            LastName = "Expiry",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1,
            RefreshToken = "stored-token",
            RefreshTokenExpiryTime = DateTime.Now.AddDays(1) // Old expiry
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = "plain-token"
        };

        _mockSecurityService.Setup(s => s.HashToken("plain-token"))
            .Returns("stored-token");
        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("new-token");
        _mockSecurityService.Setup(s => s.HashToken("new-token"))
            .Returns("new-hashed-token");

        var beforeRefresh = DateTime.Now;

        // Act
        await _controller.RefreshToken(request);
        var afterRefresh = DateTime.Now;

        // Assert
        _context.ChangeTracker.Clear();
        var updatedUser = await _unitOfWork.Repository<User>().GetByIdAsync(user.Id);
        updatedUser!.RefreshTokenExpiryTime.Should().BeAfter(beforeRefresh.AddDays(6));
        updatedUser.RefreshTokenExpiryTime.Should().BeBefore(afterRefresh.AddDays(8));
    }

    [Fact]
    public async Task RefreshToken_ShouldLogSecurityEvent()
    {
        // Arrange
        var user = new User
        {
            Id = 304,
            Username = "refreshaudit",
            Email = "refreshaudit@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Refresh",
            LastName = "Audit",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1,
            RefreshToken = "stored-token",
            RefreshTokenExpiryTime = DateTime.Now.AddDays(7)
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = "plain-token"
        };

        _mockSecurityService.Setup(s => s.HashToken("plain-token"))
            .Returns("stored-token");
        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("new-token");
        _mockSecurityService.Setup(s => s.HashToken("new-token"))
            .Returns("new-hashed");

        // Act
        await _controller.RefreshToken(request);

        // Assert
        _mockAuditService.Verify(
            s => s.LogSecurityEventAsync(It.Is<SecurityLog>(log =>
                log.EventType == SecurityEventType.TokenRefreshed &&
                log.Success == true
            ), default),
            Times.Once
        );
    }

    [Fact]
    public async Task Logout_WithAuthenticatedUser_ShouldClearRefreshToken()
    {
        // Arrange
        var user = new User
        {
            Id = 305,
            Username = "logouttest",
            Email = "logout@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Logout",
            LastName = "Test",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1,
            RefreshToken = "some-refresh-token",
            RefreshTokenExpiryTime = DateTime.Now.AddDays(7)
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Setup authenticated user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _controller.Logout();

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();

        _context.ChangeTracker.Clear();
        var updatedUser = await _unitOfWork.Repository<User>().GetByIdAsync(user.Id);
        updatedUser!.RefreshToken.Should().BeNull();
        updatedUser.RefreshTokenExpiryTime.Should().BeNull();
    }

    [Fact]
    public async Task Logout_ShouldLogSecurityEvent()
    {
        // Arrange
        var user = new User
        {
            Id = 306,
            Username = "logoutaudit",
            Email = "logoutaudit@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Logout",
            LastName = "Audit",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1,
            RefreshToken = "token",
            RefreshTokenExpiryTime = DateTime.Now.AddDays(7)
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        await _controller.Logout();

        // Assert
        _mockAuditService.Verify(
            s => s.LogSecurityEventAsync(It.Is<SecurityLog>(log =>
                log.EventType == SecurityEventType.Logout &&
                log.Success == true &&
                log.UserId == user.Id
            ), default),
            Times.Once
        );
    }

    [Fact]
    public async Task Logout_WithNoAuthenticatedUser_ShouldStillReturnSuccess()
    {
        // Arrange - No authenticated user
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.Logout();

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
