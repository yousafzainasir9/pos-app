using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using POS.Infrastructure.Data;
using POS.Infrastructure.Data.Interceptors;
using POS.Infrastructure.Data.Seeders;
using POS.Infrastructure.Services;
using POS.Migrator.Services;
using System.IO;

var webApiProjectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
    "..", "..", "..", "..", "..", "src", "POS.WebAPI"));

if (Directory.Exists(webApiProjectPath))
{
    Directory.SetCurrentDirectory(webApiProjectPath);
}
else
{
    Console.WriteLine($"Warning: Expected WebAPI project directory not found at {webApiProjectPath}. Relative paths may not resolve correctly.");
}

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true)
                     .AddJsonFile("appsettings.Development.json", optional: true)
                     .AddEnvironmentVariables();

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("The DefaultConnection connection string is not configured.");
}

builder.Services.AddDbContext<POSDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        b => b.MigrationsAssembly(typeof(POSDbContext).Assembly.FullName)));

builder.Services.AddScoped<ICurrentUserService, MigratorCurrentUserService>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>();
builder.Services.AddScoped<AuditableEntitySaveChangesInterceptor>();
builder.Services.AddScoped<DatabaseSeeder>();

using var host = builder.Build();

await using var scope = host.Services.CreateAsyncScope();
var serviceProvider = scope.ServiceProvider;
var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Migrator");

try
{
    var dbContext = serviceProvider.GetRequiredService<POSDbContext>();
    
    // Check if we should refresh the database (default to true for development)
    var refreshDatabase = builder.Configuration.GetValue<bool>("RefreshDatabase", true);
    
    if (refreshDatabase)
    {
        logger.LogWarning("RefreshDatabase is enabled. Dropping and recreating the database...");
        
        // Drop the database if it exists
        logger.LogInformation("Checking if database exists...");
        if (await dbContext.Database.CanConnectAsync())
        {
            logger.LogWarning("Dropping existing database...");
            await dbContext.Database.EnsureDeletedAsync();
            logger.LogInformation("Database dropped successfully.");
        }
        else
        {
            logger.LogInformation("Database does not exist.");
        }
        
        // Create the database
        logger.LogInformation("Creating new database...");
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("Database created successfully.");
        
        // Apply migrations
        logger.LogInformation("Applying migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Migrations applied successfully.");
    }
    else
    {
        logger.LogInformation("RefreshDatabase is disabled. Applying migrations only...");
        
        // Just apply migrations without dropping the database
        logger.LogInformation("Starting database migration...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrated successfully.");
    }
    
    // Always seed the database
    var seeder = serviceProvider.GetRequiredService<DatabaseSeeder>();
    logger.LogInformation("Starting database seeding...");
    await seeder.SeedAsync(refreshDatabase);
    logger.LogInformation("Database seeded successfully.");
    
    // Display summary
    Console.WriteLine();
    Console.WriteLine("========================================");
    Console.WriteLine("   DATABASE SETUP COMPLETED SUCCESSFULLY");
    Console.WriteLine("========================================");
    if (refreshDatabase)
    {
        Console.WriteLine("✅ Database dropped and recreated");
    }
    Console.WriteLine("✅ Migrations applied");
    Console.WriteLine("✅ Seed data inserted");
    Console.WriteLine();
    Console.WriteLine("Default Users:");
    Console.WriteLine("  Admin    - Username: admin     | Password: Admin123!    | PIN: 9999");
    Console.WriteLine("  Manager  - Username: manager   | Password: Manager123!  | PIN: 1234");
    Console.WriteLine("  Cashier  - Username: cashier1  | Password: Cashier123!  | PIN: 1111");
    Console.WriteLine();
    Console.WriteLine("You can now run the POS.WebAPI project.");
    Console.WriteLine("========================================");
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred while setting up the database.");
    Console.WriteLine();
    Console.WriteLine("❌ DATABASE SETUP FAILED");
    Console.WriteLine($"Error: {ex.Message}");
    
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
    }
    
    Environment.ExitCode = 1;
}
