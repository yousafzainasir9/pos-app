using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using POS.Infrastructure.Data;
using POS.Infrastructure.Data.Interceptors;
using POS.Infrastructure.Data.Seeders;
using System.Diagnostics;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json", optional: false);
        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
        
        // Register the interceptor and its dependencies
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<ICurrentUserService, MigratorUserService>();
        services.AddScoped<IDateTimeService, MigratorDateTimeService>();
        
        services.AddDbContext<POSDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        services.AddScoped<DatabaseSeeder>();
        
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<POSDbContext>();
    var configuration = services.GetRequiredService<IConfiguration>();
    var seeder = services.GetRequiredService<DatabaseSeeder>();

    try
    {
        Console.WriteLine("========================================");
        Console.WriteLine("   POS DATABASE MIGRATOR & SEEDER");
        Console.WriteLine("========================================");
        Console.WriteLine();

        // Check if we should drop and recreate
        var refreshDatabase = configuration.GetValue<bool>("RefreshDatabase", false);
        
        if (refreshDatabase)
        {
            Console.WriteLine("⚠️  RefreshDatabase is enabled - This will DROP and recreate the database!");
            Console.Write("Are you sure you want to continue? (y/n): ");
            var confirm = Console.ReadLine();
            
            if (confirm?.ToLower() != "y")
            {
                Console.WriteLine("Migration cancelled.");
                return;
            }
            
            Console.WriteLine("📦 Dropping existing database...");
            await context.Database.EnsureDeletedAsync();
            Console.WriteLine("✅ Database dropped");
        }

        // Ensure database exists
        Console.WriteLine("📦 Ensuring database exists...");
        await context.Database.EnsureCreatedAsync();
        
        // Check for pending migrations
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        var pendingCount = pendingMigrations.Count();
        
        if (pendingCount > 0)
        {
            Console.WriteLine($"📝 Found {pendingCount} pending migration(s):");
            foreach (var migration in pendingMigrations)
            {
                Console.WriteLine($"   - {migration}");
            }
            
            Console.WriteLine("📦 Applying migrations...");
            await context.Database.MigrateAsync();
            Console.WriteLine("✅ Migrations applied successfully");
        }
        else
        {
            Console.WriteLine("✅ Database is up to date - no pending migrations");
            
            // Check if model has changes that need migration
            Console.WriteLine("\n🔍 Checking for model changes...");
            Console.WriteLine("   To generate a new migration if models have changed, run:");
            Console.WriteLine("   cd src\\POS.Infrastructure");
            Console.WriteLine("   dotnet ef migrations add YourMigrationName -s ..\\POS.WebAPI");
            Console.WriteLine();
        }

        // Seed data
        var seedData = configuration.GetValue<bool>("SeedData", true);
        if (seedData)
        {
            Console.WriteLine("🌱 Seeding database...");
            await seeder.SeedAsync(refreshDatabase);
            Console.WriteLine("✅ Seed data inserted successfully");
        }

        // Display statistics
        Console.WriteLine("\n📊 Database Statistics:");
        Console.WriteLine($"  • Stores:        {await context.Stores.CountAsync()}");
        Console.WriteLine($"  • Users:         {await context.Users.CountAsync()}");
        Console.WriteLine($"  • Categories:    {await context.Categories.CountAsync()}");
        Console.WriteLine($"  • Subcategories: {await context.Subcategories.CountAsync()}");
        Console.WriteLine($"  • Products:      {await context.Products.CountAsync()}");
        Console.WriteLine($"  • Customers:     {await context.Customers.CountAsync()}");
        Console.WriteLine($"  • Suppliers:     {await context.Suppliers.CountAsync()}");
        Console.WriteLine($"  • Orders:        {await context.Orders.CountAsync()}");
        Console.WriteLine($"  • Payments:      {await context.Payments.CountAsync()}");
        Console.WriteLine($"  • Inventory:     {await context.InventoryTransactions.CountAsync()} transactions");

        Console.WriteLine("\n========================================");
        Console.WriteLine("   DATABASE SETUP COMPLETED ✅");
        Console.WriteLine("========================================");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
        Console.WriteLine($"\n❌ ERROR: {ex.Message}");
        
        if (ex.InnerException != null)
        {
            Console.WriteLine($"   Inner: {ex.InnerException.Message}");
        }
        
        Console.WriteLine("\n💡 Troubleshooting Tips:");
        Console.WriteLine("1. Check your connection string in appsettings.json");
        Console.WriteLine("2. Ensure SQL Server is running");
        Console.WriteLine("3. Verify you have proper permissions");
        Console.WriteLine("4. If models changed, generate migration first:");
        Console.WriteLine("   cd src\\POS.Infrastructure");
        Console.WriteLine("   dotnet ef migrations add YourMigrationName -s ..\\POS.WebAPI");
        
        Environment.Exit(1);
    }
}

// Helper to create a migration (this won't work at runtime, but shows the command)
void ShowMigrationHelp()
{
    Console.WriteLine("\n📝 To create a new migration:");
    Console.WriteLine("   1. Open a terminal in the backend folder");
    Console.WriteLine("   2. Run these commands:");
    Console.WriteLine("      cd src\\POS.Infrastructure");
    Console.WriteLine("      dotnet ef migrations add YourMigrationName -s ..\\POS.WebAPI");
    Console.WriteLine("   3. Then run this migrator again to apply it");
}

// Simple implementations for the migrator context
public class MigratorUserService : ICurrentUserService
{
    public long? UserId => 1; // System user
    public string? Username => "System";
    public string? Email => "system@pos.com";
}

public class MigratorDateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
