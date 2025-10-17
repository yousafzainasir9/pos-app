using System.Security.Claims;

namespace POS.WebAPI.Tests.Helpers;

public static class TestClaimsPrincipalFactory
{
    /// <summary>
    /// Creates a ClaimsPrincipal for an admin user
    /// </summary>
    public static ClaimsPrincipal CreateAdminUser(long userId = 1, long storeId = 1)
    {
        return CreateUser(userId, "testadmin", "Admin", "admin@test.com", storeId);
    }

    /// <summary>
    /// Creates a ClaimsPrincipal for a manager user
    /// </summary>
    public static ClaimsPrincipal CreateManagerUser(long userId = 2, long storeId = 1)
    {
        return CreateUser(userId, "testmanager", "Manager", "manager@test.com", storeId);
    }

    /// <summary>
    /// Creates a ClaimsPrincipal for a cashier user
    /// </summary>
    public static ClaimsPrincipal CreateCashierUser(long userId = 3, long storeId = 1)
    {
        return CreateUser(userId, "testcashier", "Cashier", "cashier@test.com", storeId);
    }

    /// <summary>
    /// Creates a ClaimsPrincipal for a customer user
    /// </summary>
    public static ClaimsPrincipal CreateCustomerUser(long userId = 4)
    {
        return CreateUser(userId, "testcustomer", "Customer", "customer@test.com", null);
    }

    /// <summary>
    /// Creates a ClaimsPrincipal with custom claims
    /// </summary>
    public static ClaimsPrincipal CreateUser(
        long userId,
        string username,
        string role,
        string email,
        long? storeId = null,
        string firstName = "Test",
        string lastName = "User")
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.GivenName, firstName),
            new Claim(ClaimTypes.Surname, lastName),
            new Claim(ClaimTypes.Role, role)
        };

        if (storeId.HasValue)
        {
            claims.Add(new Claim("StoreId", storeId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Creates an unauthenticated ClaimsPrincipal
    /// </summary>
    public static ClaimsPrincipal CreateUnauthenticatedUser()
    {
        return new ClaimsPrincipal(new ClaimsIdentity());
    }
}
