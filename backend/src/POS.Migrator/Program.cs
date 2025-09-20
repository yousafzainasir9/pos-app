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

await using var host = builder.Build();

await using var scope = host.Services.CreateAsyncScope();
var serviceProvider = scope.ServiceProvider;
var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Migrator");

try
{
    var dbContext = serviceProvider.GetRequiredService<POSDbContext>();
    logger.LogInformation("Starting database migration...");
    await dbContext.Database.MigrateAsync();
    logger.LogInformation("Database migrated successfully.");

    var seeder = serviceProvider.GetRequiredService<DatabaseSeeder>();
    logger.LogInformation("Starting database seeding...");
    await seeder.SeedAsync();
    logger.LogInformation("Database seeded successfully.");
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    Environment.ExitCode = 1;
}
