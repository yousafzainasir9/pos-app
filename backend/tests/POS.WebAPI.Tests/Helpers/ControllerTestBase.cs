using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using POS.Application.Common.Interfaces;
using System.Security.Claims;

namespace POS.WebAPI.Tests.Helpers;

/// <summary>
/// Base class for controller tests with common setup
/// </summary>
public abstract class ControllerTestBase
{
    protected Mock<IUnitOfWork> MockUnitOfWork { get; }
    protected Mock<ILogger> MockLogger { get; }

    protected ControllerTestBase()
    {
        MockUnitOfWork = new Mock<IUnitOfWork>();
        MockLogger = new Mock<ILogger>();
    }

    /// <summary>
    /// Sets up controller context with authenticated user
    /// </summary>
    protected void SetupControllerContext<T>(
        T controller, 
        long userId, 
        string username, 
        string role,
        long? storeId = null) where T : ControllerBase
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        if (storeId.HasValue)
        {
            claims.Add(new Claim("StoreId", storeId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    /// <summary>
    /// Verify that repository was called to save changes
    /// </summary>
    protected void VerifySaveChanges(Times times)
    {
        MockUnitOfWork.Verify(u => u.SaveChangesAsync(default), times);
    }

    /// <summary>
    /// Setup mock repository for a given entity type
    /// </summary>
    protected Mock<IRepository<TEntity>> SetupMockRepository<TEntity>() where TEntity : class
    {
        var mockRepository = new Mock<IRepository<TEntity>>();
        MockUnitOfWork.Setup(u => u.Repository<TEntity>()).Returns(mockRepository.Object);
        return mockRepository;
    }
}
