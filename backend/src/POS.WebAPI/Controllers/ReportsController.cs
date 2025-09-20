using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces;
using POS.Application.DTOs.Reports;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace POS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                if (endDate < startDate)
                {
                    return BadRequest(new { message = "End date must be after start date" });
                }

                var report = await _reportService.GetSalesReportAsync(startDate, endDate);
                return Ok(new { data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sales report");
                return StatusCode(500, new { message = "Failed to generate sales report" });
            }
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProductPerformance([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                if (endDate < startDate)
                {
                    return BadRequest(new { message = "End date must be after start date" });
                }

                var report = await _reportService.GetProductPerformanceAsync(startDate, endDate);
                return Ok(new { data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating product performance report");
                return StatusCode(500, new { message = "Failed to generate product performance report" });
            }
        }

        [HttpGet("shifts/current")]
        public async Task<IActionResult> GetCurrentShiftReport()
        {
            try
            {
                var userId = GetCurrentUserId();
                var report = await _reportService.GetCurrentShiftReportAsync(userId);
                
                if (report == null)
                {
                    return NotFound(new { message = "No active shift found" });
                }

                return Ok(new { data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating current shift report");
                return StatusCode(500, new { message = "Failed to generate shift report" });
            }
        }

        [HttpGet("shifts/{shiftId}")]
        public async Task<IActionResult> GetShiftReport(int shiftId)
        {
            try
            {
                var report = await _reportService.GetShiftReportAsync(shiftId);
                
                if (report == null)
                {
                    return NotFound(new { message = "Shift not found" });
                }

                return Ok(new { data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating shift report");
                return StatusCode(500, new { message = "Failed to generate shift report" });
            }
        }

        [HttpGet("export/{reportType}")]
        public async Task<IActionResult> ExportReport(string reportType, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                byte[] fileContent;
                string fileName;
                string contentType = "text/csv";

                switch (reportType.ToLower())
                {
                    case "sales":
                        fileContent = await _reportService.ExportSalesReportAsync(startDate, endDate);
                        fileName = $"sales-report-{startDate:yyyy-MM-dd}-to-{endDate:yyyy-MM-dd}.csv";
                        break;
                    case "products":
                        fileContent = await _reportService.ExportProductPerformanceAsync(startDate, endDate);
                        fileName = $"product-performance-{startDate:yyyy-MM-dd}-to-{endDate:yyyy-MM-dd}.csv";
                        break;
                    case "shifts":
                        fileContent = await _reportService.ExportShiftReportAsync(startDate, endDate);
                        fileName = $"shift-report-{startDate:yyyy-MM-dd}-to-{endDate:yyyy-MM-dd}.csv";
                        break;
                    default:
                        return BadRequest(new { message = "Invalid report type" });
                }

                return File(fileContent, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                return StatusCode(500, new { message = "Failed to export report" });
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = await _reportService.GetDashboardStatsAsync();
                return Ok(new { data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard stats");
                return StatusCode(500, new { message = "Failed to generate dashboard stats" });
            }
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public async Task<IActionResult> TestReport()
        {
            try
            {
                var today = DateTime.Today;
                var report = await _reportService.GetSalesReportAsync(today, today);
                return Ok(new { 
                    message = "Test successful",
                    date = today.ToString("yyyy-MM-dd"),
                    data = report 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test report");
                return StatusCode(500, new { 
                    message = "Test failed", 
                    error = ex.Message,
                    innerError = ex.InnerException?.Message 
                });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in token");
        }
    }
}
