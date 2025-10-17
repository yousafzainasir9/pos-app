using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace POS.WebAPI.Tests.Helpers;

public static class MockHttpContextFactory
{
    /// <summary>
    /// Creates a mock HttpContext with authenticated user
    /// </summary>
    public static HttpContext Create(ClaimsPrincipal user)
    {
        var context = new DefaultHttpContext
        {
            User = user
        };

        // Set up request context
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("localhost", 7000);
        context.Request.Path = "/api/test";

        // Set up connection info
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        return context;
    }

    /// <summary>
    /// Creates a mock HttpContext with admin user
    /// </summary>
    public static HttpContext CreateWithAdminUser(long userId = 1, long storeId = 1)
    {
        var user = TestClaimsPrincipalFactory.CreateAdminUser(userId, storeId);
        return Create(user);
    }

    /// <summary>
    /// Creates a mock HttpContext with cashier user
    /// </summary>
    public static HttpContext CreateWithCashierUser(long userId = 2, long storeId = 1)
    {
        var user = TestClaimsPrincipalFactory.CreateCashierUser(userId, storeId);
        return Create(user);
    }

    /// <summary>
    /// Creates a mock HttpContext with customer user
    /// </summary>
    public static HttpContext CreateWithCustomerUser(long userId = 3)
    {
        var user = TestClaimsPrincipalFactory.CreateCustomerUser(userId);
        return Create(user);
    }

    /// <summary>
    /// Creates a mock HttpContext with unauthenticated user
    /// </summary>
    public static HttpContext CreateUnauthenticated()
    {
        var user = TestClaimsPrincipalFactory.CreateUnauthenticatedUser();
        return Create(user);
    }
}
