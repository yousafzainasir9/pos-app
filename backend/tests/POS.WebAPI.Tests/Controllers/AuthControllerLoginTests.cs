using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
using POS.WebAPI.Tests.Helpers;
using POS.WebAPI.Controllers;
using POS.WebAPI.Tests.Helpers;
using Xunit;

namespace POS.WebAPI.Tests.Controllers;

/// <summary>
/// Tests for AuthController - Login with username and password
/// </summary>
public class AuthControllerLoginTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ISecurityService> _mockSecurityService;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;
    private readonly UnitOfWork _unitOfWork;

    public AuthControllerLoginTests()
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
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test123!");
        var user = new User
        {
            Id = 100,
            Username = "logintest",
            Email = "logintest@test.com",
            PasswordHash = hashedPassword,
            FirstName = "Login",
            LastName = "Test",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Username = "logintest",
            Password = "Test123!"
        };

        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("test-refresh-token");
        _mockSecurityService.Setup(s => s.HashToken(It.IsAny<string>()))
            .Returns("hashed-refresh-token");

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<LoginResponseDto>;
        
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Token.Should().NotBeNullOrEmpty();
        response.Data.RefreshToken.Should().NotBeNullOrEmpty();
        response.Data.User.Should().NotBeNull();
        response.Data.User.Username.Should().Be("logintest");
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ShouldThrowAuthenticationException()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Username = "nonexistent",
            Password = "Password123!"
        };

        // Act
        Func<Task> act = async () => await _controller.Login(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldThrowAuthenticationException()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");
        var user = new User
        {
            Id = 101,
            Username = "passwordtest",
            Email = "passwordtest@test.com",
            PasswordHash = hashedPassword,
            FirstName = "Password",
            LastName = "Test",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Username = "passwordtest",
            Password = "WrongPassword"
        };

        // Act
        Func<Task> act = async () => await _controller.Login(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Login_WithInactiveUser_ShouldThrowAuthenticationException()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test123!");
        var user = new User
        {
            Id = 102,
            Username = "inactiveuser",
            Email = "inactive@test.com",
            PasswordHash = hashedPassword,
            FirstName = "Inactive",
            LastName = "User",
            Role = UserRole.Cashier,
            IsActive = false, // Inactive user
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Username = "inactiveuser",
            Password = "Test123!"
        };

        // Act
        Func<Task> act = async () => await _controller.Login(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Login_ShouldUpdateLastLoginAt()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test123!");
        var user = new User
        {
            Id = 103,
            Username = "lastlogintest",
            Email = "lastlogin@test.com",
            PasswordHash = hashedPassword,
            FirstName = "LastLogin",
            LastName = "Test",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1,
            LastLoginAt = null
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Username = "lastlogintest",
            Password = "Test123!"
        };

        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("test-refresh-token");
        _mockSecurityService.Setup(s => s.HashToken(It.IsAny<string>()))
            .Returns("hashed-refresh-token");

        var beforeLogin = DateTime.Now;

        // Act
        await _controller.Login(request);
        var afterLogin = DateTime.Now;

        // Assert
        _context.ChangeTracker.Clear();
        var updatedUser = await _unitOfWork.Repository<User>().GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.LastLoginAt.Should().NotBeNull();
        updatedUser.LastLoginAt.Should().BeOnOrAfter(beforeLogin);
        updatedUser.LastLoginAt.Should().BeOnOrBefore(afterLogin);
    }

    [Fact]
    public async Task Login_ShouldGenerateRefreshToken()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test123!");
        var user = new User
        {
            Id = 104,
            Username = "refreshtest",
            Email = "refresh@test.com",
            PasswordHash = hashedPassword,
            FirstName = "Refresh",
            LastName = "Test",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Username = "refreshtest",
            Password = "Test123!"
        };

        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("generated-refresh-token");
        _mockSecurityService.Setup(s => s.HashToken("generated-refresh-token"))
            .Returns("hashed-refresh-token");

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<LoginResponseDto>;
        
        response!.Data!.RefreshToken.Should().Be("generated-refresh-token");
        
        // Verify refresh token was hashed and stored
        _mockSecurityService.Verify(s => s.GenerateSecureToken(It.IsAny<int>()), Times.Once);
        _mockSecurityService.Verify(s => s.HashToken("generated-refresh-token"), Times.Once);
    }

    [Fact]
    public async Task Login_ShouldStoreHashedRefreshToken()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test123!");
        var user = new User
        {
            Id = 105,
            Username = "hashtest",
            Email = "hash@test.com",
            PasswordHash = hashedPassword,
            FirstName = "Hash",
            LastName = "Test",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Username = "hashtest",
            Password = "Test123!"
        };

        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("plain-refresh-token");
        _mockSecurityService.Setup(s => s.HashToken("plain-refresh-token"))
            .Returns("secure-hashed-token");

        // Act
        await _controller.Login(request);

        // Assert
        _context.ChangeTracker.Clear();
        var updatedUser = await _unitOfWork.Repository<User>().GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.RefreshToken.Should().Be("secure-hashed-token");
        updatedUser.RefreshToken.Should().NotBe("plain-refresh-token"); // Should be hashed
    }

    [Fact]
    public async Task Login_ShouldSetRefreshTokenExpiry()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test123!");
        var user = new User
        {
            Id = 106,
            Username = "expirytest",
            Email = "expiry@test.com",
            PasswordHash = hashedPassword,
            FirstName = "Expiry",
            LastName = "Test",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Username = "expirytest",
            Password = "Test123!"
        };

        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("test-token");
        _mockSecurityService.Setup(s => s.HashToken(It.IsAny<string>()))
            .Returns("hashed-token");

        var beforeLogin = DateTime.Now;

        // Act
        await _controller.Login(request);
        var afterLogin = DateTime.Now;

        // Assert
        _context.ChangeTracker.Clear();
        var updatedUser = await _unitOfWork.Repository<User>().GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.RefreshTokenExpiryTime.Should().NotBeNull();
        updatedUser.RefreshTokenExpiryTime.Should().BeAfter(beforeLogin.AddDays(6)); // At least 6 days
        updatedUser.RefreshTokenExpiryTime.Should().BeBefore(afterLogin.AddDays(8)); // Less than 8 days
    }

    [Fact]
    public async Task Login_ShouldLogSecurityEvent()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test123!");
        var user = new User
        {
            Id = 107,
            Username = "audittest",
            Email = "audit@test.com",
            PasswordHash = hashedPassword,
            FirstName = "Audit",
            LastName = "Test",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Username = "audittest",
            Password = "Test123!"
        };

        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("test-token");
        _mockSecurityService.Setup(s => s.HashToken(It.IsAny<string>()))
            .Returns("hashed-token");

        // Act
        await _controller.Login(request);

        // Assert
        _mockAuditService.Verify(
            s => s.LogSecurityEventAsync(It.Is<SecurityLog>(log =>
                log.EventType == SecurityEventType.Login &&
                log.Success == true &&
                log.UserId == user.Id
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Login_WithAdminUser_ShouldReturnAdminRole()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        var admin = new User
        {
            Id = 108,
            Username = "admintest",
            Email = "admin@test.com",
            PasswordHash = hashedPassword,
            FirstName = "Admin",
            LastName = "Test",
            Role = UserRole.Admin,
            IsActive = true,
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(admin);
        await _unitOfWork.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Username = "admintest",
            Password = "Admin123!"
        };

        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("test-token");
        _mockSecurityService.Setup(s => s.HashToken(It.IsAny<string>()))
            .Returns("hashed-token");

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<LoginResponseDto>;
        
        response!.Data!.User.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_ShouldIncludeStoreInformation()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test123!");
        var user = new User
        {
            Id = 109,
            Username = "storetest",
            Email = "store@test.com",
            PasswordHash = hashedPassword,
            FirstName = "Store",
            LastName = "Test",
            Role = UserRole.Cashier,
            IsActive = true,
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Username = "storetest",
            Password = "Test123!"
        };

        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("test-token");
        _mockSecurityService.Setup(s => s.HashToken(It.IsAny<string>()))
            .Returns("hashed-token");

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<LoginResponseDto>;
        
        response!.Data!.User.StoreId.Should().Be(1);
        response.Data.User.StoreName.Should().NotBeNullOrEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
