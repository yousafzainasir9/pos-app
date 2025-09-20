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

Console.WriteLine("========================================");
Console.WriteLine("   Cookie Barrel POS - Database Setup");
Console.WriteLine("========================================");
Console.WriteLine();

var webApiProjectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
    "..", "..", "..", "..", "..", "src", "POS.WebAPI"));

if (Directory.Exists(webApiProjectPath))
{
    Directory.SetCurrentDirectory(webApiProjectPath);
    Console.WriteLine($"Working directory: {webApiProjectPath}");
}
else
{
    Console.WriteLine($"Warning: Expected WebAPI project directory not found at {webApiProjectPath}");
    Console.WriteLine("Relative paths may not resolve correctly.");
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

Console.WriteLine($"Database Server: {connectionString.Split(';')[0].Replace("Server=", "")}");
Console.WriteLine();

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
        Console.WriteLine("⚠️  Database refresh enabled - all data will be replaced!");
        Console.WriteLine();
        
        // Drop the database if it exists
        logger.LogInformation("Checking if database exists...");
        if (await dbContext.Database.CanConnectAsync())
        {
            logger.LogWarning("Dropping existing database...");
            Console.WriteLine("Dropping existing database...");
            await dbContext.Database.EnsureDeletedAsync();
            logger.LogInformation("Database dropped successfully.");
        }
        else
        {
            logger.LogInformation("Database does not exist.");
        }
        
        // Create the database
        logger.LogInformation("Creating new database...");
        Console.WriteLine("Creating new database...");
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("Database created successfully.");
        
        // Apply migrations
        logger.LogInformation("Applying migrations...");
        Console.WriteLine("Applying migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Migrations applied successfully.");
    }
    else
    {
        logger.LogInformation("RefreshDatabase is disabled. Applying migrations only...");
        Console.WriteLine("Update mode - keeping existing data");
        
        // Just apply migrations without dropping the database
        logger.LogInformation("Starting database migration...");
        Console.WriteLine("Applying migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrated successfully.");
    }
    
    // Always seed the database
    var seeder = serviceProvider.GetRequiredService<DatabaseSeeder>();
    logger.LogInformation("Starting database seeding...");
    Console.WriteLine();
    Console.WriteLine("Seeding database with initial data:");
    Console.WriteLine("  • Stores and Users");
    Console.WriteLine("  • Suppliers and Customers");
    Console.WriteLine("  • Categories and Products from JSON catalog");
    Console.WriteLine("  • Additional data from Excel files (if present)");
    Console.WriteLine("  • Custom data files from /data directory");
    Console.WriteLine();
    
    await seeder.SeedAsync(refreshDatabase);
    logger.LogInformation("Database seeded successfully.");
    
    // Get statistics
    var stats = new
    {
        Stores = await dbContext.Stores.CountAsync(),
        Users = await dbContext.Users.CountAsync(),
        Categories = await dbContext.Categories.CountAsync(),
        Subcategories = await dbContext.Subcategories.CountAsync(),
        Products = await dbContext.Products.CountAsync(),
        Customers = await dbContext.Customers.CountAsync(),
        Suppliers = await dbContext.Suppliers.CountAsync()
    };
    
    // Display summary
    Console.WriteLine();
    Console.WriteLine("========================================");
    Console.WriteLine("   DATABASE SETUP COMPLETED ✅");
    Console.WriteLine("========================================");
    if (refreshDatabase)
    {
        Console.WriteLine("✅ Database dropped and recreated");
    }
    Console.WriteLine("✅ Migrations applied");
    Console.WriteLine("✅ Seed data inserted");
    Console.WriteLine();
    Console.WriteLine("Database Statistics:");
    Console.WriteLine($"  • Stores:       {stats.Stores}");
    Console.WriteLine($"  • Users:        {stats.Users}");
    Console.WriteLine($"  • Categories:   {stats.Categories}");
    Console.WriteLine($"  • Subcategories: {stats.Subcategories}");
    Console.WriteLine($"  • Products:     {stats.Products}");
    Console.WriteLine($"  • Customers:    {stats.Customers}");
    Console.WriteLine($"  • Suppliers:    {stats.Suppliers}");
    Console.WriteLine();
    Console.WriteLine("Default Users:");
    Console.WriteLine("  Admin    - Username: admin     | Password: Admin123!    | PIN: 9999");
    Console.WriteLine("  Manager  - Username: manager   | Password: Manager123!  | PIN: 1234");
    Console.WriteLine("  Cashier  - Username: cashier1  | Password: Cashier123!  | PIN: 1111");
    Console.WriteLine("  Cashier  - Username: cashier2  | Password: Cashier123!  | PIN: 2222");
    Console.WriteLine();
    Console.WriteLine("Data Import:");
    Console.WriteLine("  Place additional JSON/XLSX files in: backend\\data\\");
    Console.WriteLine("  Supported: customers.json, suppliers.json, products.xlsx");
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

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
