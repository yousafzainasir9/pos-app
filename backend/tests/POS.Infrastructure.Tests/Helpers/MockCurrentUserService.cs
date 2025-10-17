using Moq;
using POS.Infrastructure.Data.Interceptors;
using System.Security.Claims;

namespace POS.Infrastructure.Tests.Helpers;

public class MockCurrentUserService : ICurrentUserService
{
    public long? UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }

    public MockCurrentUserService(long? userId = null, string? username = null, string? email = null)
    {
        UserId = userId;
        Username = username;
        Email = email;
    }

    /// <summary>
    /// Creates a mock with admin user
    /// </summary>
    public static MockCurrentUserService CreateAdmin(long userId = 1)
    {
        return new MockCurrentUserService(userId, "testadmin", "admin@test.com");
    }

    /// <summary>
    /// Creates a mock with cashier user
    /// </summary>
    public static MockCurrentUserService CreateCashier(long userId = 2)
    {
        return new MockCurrentUserService(userId, "testcashier", "cashier@test.com");
    }

    /// <summary>
    /// Creates a mock with customer user
    /// </summary>
    public static MockCurrentUserService CreateCustomer(long userId = 3)
    {
        return new MockCurrentUserService(userId, "testcustomer", "customer@test.com");
    }

    /// <summary>
    /// Creates a Moq mock of ICurrentUserService
    /// </summary>
    public static Mock<ICurrentUserService> CreateMock(long? userId = null, string? username = null, string? email = null)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(s => s.UserId).Returns(userId);
        mock.Setup(s => s.Username).Returns(username);
        mock.Setup(s => s.Email).Returns(email);
        return mock;
    }
}
