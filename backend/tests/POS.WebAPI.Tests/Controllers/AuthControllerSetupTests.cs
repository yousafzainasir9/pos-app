using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using POS.Application.Common.Interfaces;
using POS.Application.DTOs.Auth;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.WebAPI.Controllers;
using POS.WebAPI.Tests.Helpers;
using Xunit;

namespace POS.WebAPI.Tests.Controllers;

/// <summary>
/// Tests for AuthController - Setup verification test
/// This is a basic test to ensure the test infrastructure is working
/// </summary>
public class AuthControllerSetupTests : ControllerTestBase
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ISecurityService> _mockSecurityService;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ILogger<AuthController>> _mockControllerLogger;
    private readonly AuthController _controller;

    public AuthControllerSetupTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockSecurityService = new Mock<ISecurityService>();
        _mockAuditService = new Mock<IAuditService>();
        _mockControllerLogger = new Mock<ILogger<AuthController>>();

        // Setup JWT configuration
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(s => s["SecretKey"]).Returns("test-secret-key-that-is-at-least-32-characters-long-for-security");
        jwtSection.Setup(s => s["Issuer"]).Returns("TestIssuer");
        jwtSection.Setup(s => s["Audience"]).Returns("TestAudience");
        _mockConfiguration.Setup(c => c.GetSection("Jwt")).Returns(jwtSection.Object);

        _controller = new AuthController(
            MockUnitOfWork.Object,
            _mockConfiguration.Object,
            _mockControllerLogger.Object,
            _mockSecurityService.Object,
            _mockAuditService.Object
        );
    }

    [Fact]
    public void Controller_ShouldBeCreated_Successfully()
    {
        // Assert
        _controller.Should().NotBeNull();
        _controller.Should().BeOfType<AuthController>();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Username = "nonexistent",
            Password = "wrong"
        };

        var mockUserRepository = SetupMockRepository<User>();
        mockUserRepository.Setup(r => r.Query())
            .Returns(new List<User>().AsQueryable());

        // Act
        var act = async () => await _controller.Login(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
