using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;

namespace POS.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HealthController : ControllerBase
{
    private readonly POSDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HealthController> _logger;

    public HealthController(POSDbContext context, IConfiguration configuration, ILogger<HealthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Check database connection
            var canConnect = await _context.Database.CanConnectAsync();
            
            // Get database statistics if connected
            object? dbStats = null;
            if (canConnect)
            {
                try
                {
                    dbStats = new
                    {
                        Users = await _context.Users.CountAsync(),
                        Categories = await _context.Categories.CountAsync(),
                        Products = await _context.Products.CountAsync(),
                        Orders = await _context.Orders.CountAsync(),
                        Stores = await _context.Stores.CountAsync()
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not get database statistics");
                }
            }

            var response = new
            {
                Status = canConnect ? "Healthy" : "Unhealthy",
                Timestamp = DateTime.Now,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                Database = new
                {
                    Connected = canConnect,
                    Server = _configuration.GetConnectionString("DefaultConnection")?.Split(';')[0]?.Replace("Server=", ""),
                    DatabaseName = "POSDatabase",
                    Statistics = dbStats
                },
                Api = new
                {
                    Version = "1.0.0",
                    Name = "Cookie Barrel POS API"
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(500, new
            {
                Status = "Error",
                Message = "Health check failed",
                Error = ex.Message
            });
        }
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            // Check if database is ready for operations
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return StatusCode(503, new { Status = "Not Ready", Message = "Database connection unavailable" });
            }

            // Check if essential data exists
            var hasStores = await _context.Stores.AnyAsync();
            var hasUsers = await _context.Users.AnyAsync();

            if (!hasStores || !hasUsers)
            {
                return StatusCode(503, new { 
                    Status = "Not Ready", 
                    Message = "Database not seeded",
                    Details = new
                    {
                        HasStores = hasStores,
                        HasUsers = hasUsers
                    }
                });
            }

            return Ok(new { Status = "Ready", Message = "Application is ready to serve requests" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return StatusCode(503, new { Status = "Not Ready", Message = ex.Message });
        }
    }

    [HttpGet("live")]
    public IActionResult Live()
    {
        // Simple liveness check - if the API can respond, it's alive
        return Ok(new { Status = "Alive", Timestamp = DateTime.Now });
    }
}
