using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using POS.Infrastructure.Data;
using POS.Infrastructure.Data.Interceptors;

namespace POS.Infrastructure.Tests.Helpers;

public static class InMemoryDbContextFactory
{
    /// <summary>
    /// Creates an in-memory POSDbContext for testing
    /// </summary>
    /// <param name="databaseName">Optional unique database name. If not provided, a GUID will be used.</param>
    /// <returns>A new POSDbContext instance with in-memory database</returns>
    public static POSDbContext Create(string? databaseName = null)
    {
        var dbName = databaseName ?? Guid.NewGuid().ToString();
        
        var options = new DbContextOptionsBuilder<POSDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .EnableSensitiveDataLogging()
            .Options;

        // Create mock services required by the interceptor
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.UserId).Returns(1);
        mockCurrentUserService.Setup(s => s.Username).Returns("test-user");
        mockCurrentUserService.Setup(s => s.Email).Returns("test@example.com");
        
        var mockDateTimeService = new Mock<IDateTimeService>();
        mockDateTimeService.Setup(s => s.Now).Returns(DateTime.Now);
        mockDateTimeService.Setup(s => s.UtcNow).Returns(DateTime.UtcNow);
        
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        
        // Create the interceptor with mocked dependencies
        var interceptor = new AuditableEntitySaveChangesInterceptor(
            mockCurrentUserService.Object,
            mockDateTimeService.Object,
            mockHttpContextAccessor.Object
        );
        
        var context = new POSDbContext(options, interceptor);
        
        // Ensure the database is created
        context.Database.EnsureCreated();
        
        return context;
    }

    /// <summary>
    /// Creates an in-memory POSDbContext with pre-seeded test data
    /// </summary>
    /// <returns>A new POSDbContext instance with test data</returns>
    public static POSDbContext CreateWithData()
    {
        var context = Create();
        TestDataSeeder.SeedTestData(context);
        return context;
    }
}
