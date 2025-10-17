using FluentAssertions;
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
using POS.WebAPI.Tests.Helpers;
using POS.WebAPI.Controllers;
using Xunit;

namespace POS.WebAPI.Tests.Controllers;

/// <summary>
/// Tests for AuthController - PIN login for POS and mobile
/// </summary>
public class AuthControllerPinLoginTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ISecurityService> _mockSecurityService;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;
    private readonly UnitOfWork _unitOfWork;

    public AuthControllerPinLoginTests()
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
    public async Task PinLogin_WithValidPinAndStoreId_ShouldSucceed()
    {
        // Arrange
        var user = new User
        {
            Id = 200,
            Username = "pintest",
            Email = "pintest@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Pin",
            LastName = "Test",
            Role = UserRole.Cashier,
            Pin = "1234",
            IsActive = true,
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new PinLoginRequestDto
        {
            Pin = "1234",
            StoreId = 1
        };

        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("test-refresh-token");
        _mockSecurityService.Setup(s => s.HashToken(It.IsAny<string>()))
            .Returns("hashed-refresh-token");

        // Act
        var result = await _controller.PinLogin(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<LoginResponseDto>;
        
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data!.Token.Should().NotBeNullOrEmpty();
        response.Data.User.Username.Should().Be("pintest");
    }

    [Fact]
    public async Task PinLogin_WithInvalidPin_ShouldThrowException()
    {
        // Arrange
        var request = new PinLoginRequestDto
        {
            Pin = "0000", // Non-existent PIN
            StoreId = 1
        };

        // Act
        Func<Task> act = async () => await _controller.PinLogin(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task PinLogin_WithWrongStoreId_ShouldThrowException()
    {
        // Arrange
        var user = new User
        {
            Id = 201,
            Username = "wrongstore",
            Email = "wrongstore@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Wrong",
            LastName = "Store",
            Role = UserRole.Cashier,
            Pin = "5555",
            IsActive = true,
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new PinLoginRequestDto
        {
            Pin = "5555",
            StoreId = 2 // Wrong store
        };

        // Act
        Func<Task> act = async () => await _controller.PinLogin(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task PinLogin_CustomerWithNoStoreId_ShouldSucceed()
    {
        // Arrange
        var customer = new User
        {
            Id = 202,
            Username = "customerpintest",
            Email = "customerpin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Customer",
            LastName = "Pin",
            Role = UserRole.Customer,
            Pin = "7777",
            IsActive = true,
            StoreId = null // Customer has no store
        };
        await _unitOfWork.Repository<User>().AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        var request = new PinLoginRequestDto
        {
            Pin = "7777",
            StoreId = 0 // No store for customer
        };

        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("test-token");
        _mockSecurityService.Setup(s => s.HashToken(It.IsAny<string>()))
            .Returns("hashed-token");

        // Act
        var result = await _controller.PinLogin(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<LoginResponseDto>;
        
        response!.Success.Should().BeTrue();
        response.Data!.User.Role.Should().Be("Customer");
    }

    [Fact]
    public async Task PinLogin_WithActiveShift_ShouldReturnShiftInfo()
    {
        // Arrange
        var user = new User
        {
            Id = 203,
            Username = "shifttest",
            Email = "shift@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Shift",
            LastName = "Test",
            Role = UserRole.Cashier,
            Pin = "3333",
            IsActive = true,
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var shift = new Shift
        {
            ShiftNumber = "SHIFT001",
            UserId = user.Id,
            StoreId = 1,
            StartTime = DateTime.Now.AddHours(-2),
            Status = ShiftStatus.Open,
            StartingCash = 100m
        };
        await _unitOfWork.Repository<Shift>().AddAsync(shift);
        await _unitOfWork.SaveChangesAsync();

        var request = new PinLoginRequestDto
        {
            Pin = "3333",
            StoreId = 1
        };

        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("test-token");
        _mockSecurityService.Setup(s => s.HashToken(It.IsAny<string>()))
            .Returns("hashed-token");

        // Act
        var result = await _controller.PinLogin(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<LoginResponseDto>;
        
        response!.Data!.User.HasActiveShift.Should().BeTrue();
        response.Data.User.ActiveShiftId.Should().Be(shift.Id);
    }

    [Fact]
    public async Task PinLogin_WithNoActiveShift_ShouldReturnNoShiftInfo()
    {
        // Arrange
        var user = new User
        {
            Id = 204,
            Username = "noshifttest",
            Email = "noshift@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "NoShift",
            LastName = "Test",
            Role = UserRole.Cashier,
            Pin = "4444",
            IsActive = true,
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new PinLoginRequestDto
        {
            Pin = "4444",
            StoreId = 1
        };

        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("test-token");
        _mockSecurityService.Setup(s => s.HashToken(It.IsAny<string>()))
            .Returns("hashed-token");

        // Act
        var result = await _controller.PinLogin(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<LoginResponseDto>;
        
        response!.Data!.User.HasActiveShift.Should().BeFalse();
        response.Data.User.ActiveShiftId.Should().BeNull();
    }

    [Fact]
    public async Task PinLogin_ShouldLogSecurityEvent()
    {
        // Arrange
        var user = new User
        {
            Id = 205,
            Username = "pinaudit",
            Email = "pinaudit@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "PinAudit",
            LastName = "Test",
            Role = UserRole.Cashier,
            Pin = "6666",
            IsActive = true,
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new PinLoginRequestDto
        {
            Pin = "6666",
            StoreId = 1
        };

        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("test-token");
        _mockSecurityService.Setup(s => s.HashToken(It.IsAny<string>()))
            .Returns("hashed-token");

        // Act
        await _controller.PinLogin(request);

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
    public async Task PinLogin_WithInactiveUser_ShouldThrowException()
    {
        // Arrange
        var user = new User
        {
            Id = 206,
            Username = "inactivepin",
            Email = "inactivepin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Inactive",
            LastName = "Pin",
            Role = UserRole.Cashier,
            Pin = "8888",
            IsActive = false, // Inactive
            StoreId = 1
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new PinLoginRequestDto
        {
            Pin = "8888",
            StoreId = 1
        };

        // Act
        Func<Task> act = async () => await _controller.PinLogin(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task PinLogin_ShouldUpdateLastLoginAt()
    {
        // Arrange
        var user = new User
        {
            Id = 207,
            Username = "pinlastlogin",
            Email = "pinlastlogin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "PinLastLogin",
            LastName = "Test",
            Role = UserRole.Cashier,
            Pin = "2222",
            IsActive = true,
            StoreId = 1,
            LastLoginAt = null
        };
        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var request = new PinLoginRequestDto
        {
            Pin = "2222",
            StoreId = 1
        };

        _mockSecurityService.Setup(s => s.GenerateSecureToken(It.IsAny<int>()))
            .Returns("test-token");
        _mockSecurityService.Setup(s => s.HashToken(It.IsAny<string>()))
            .Returns("hashed-token");

        var beforeLogin = DateTime.Now;

        // Act
        await _controller.PinLogin(request);
        var afterLogin = DateTime.Now;

        // Assert
        _context.ChangeTracker.Clear();
        var updatedUser = await _unitOfWork.Repository<User>().GetByIdAsync(user.Id);
        updatedUser!.LastLoginAt.Should().NotBeNull();
        updatedUser.LastLoginAt.Should().BeOnOrAfter(beforeLogin);
        updatedUser.LastLoginAt.Should().BeOnOrBefore(afterLogin);
    }

    [Fact]
    public async Task PinLogin_FailedAttempt_ShouldLogFailedEvent()
    {
        // Arrange
        var request = new PinLoginRequestDto
        {
            Pin = "0000", // Non-existent PIN
            StoreId = 1
        };

        // Act
        try
        {
            await _controller.PinLogin(request);
        }
        catch
        {
            // Expected to throw
        }

        // Assert
        _mockAuditService.Verify(
            s => s.LogSecurityEventAsync(It.Is<SecurityLog>(log =>
                log.EventType == SecurityEventType.LoginFailed &&
                log.Success == false
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
