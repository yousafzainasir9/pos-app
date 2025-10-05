using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Audit;
using POS.Application.Interfaces;
using POS.Domain.Entities.Audit;

namespace POS.WebAPI.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>
    /// Get audit logs with filtering and pagination
    /// </summary>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<AuditLogDto>>>> GetAuditLogs(
        [FromQuery] AuditLogSearchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _auditService.GetAuditLogsAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResult<AuditLogDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get a specific audit log by ID
    /// </summary>
    [HttpGet("audit-logs/{id}")]
    [ProducesResponseType(typeof(ApiResponse<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AuditLogDto>>> GetAuditLog(
        long id,
        CancellationToken cancellationToken)
    {
        var log = await _auditService.GetAuditLogByIdAsync(id, cancellationToken);
        if (log == null)
            return NotFound(ApiResponse<AuditLogDto>.ErrorResponse(
                new ErrorResponse("AUDIT_001", "Audit log not found")));

        return Ok(ApiResponse<AuditLogDto>.SuccessResponse(log));
    }

    /// <summary>
    /// Get security logs with filtering and pagination
    /// </summary>
    [HttpGet("security-logs")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SecurityLogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SecurityLogDto>>>> GetSecurityLogs(
        [FromQuery] SecurityLogSearchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _auditService.GetSecurityLogsAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResult<SecurityLogDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get a specific security log by ID
    /// </summary>
    [HttpGet("security-logs/{id}")]
    [ProducesResponseType(typeof(ApiResponse<SecurityLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SecurityLogDto>>> GetSecurityLog(
        long id,
        CancellationToken cancellationToken)
    {
        var log = await _auditService.GetSecurityLogByIdAsync(id, cancellationToken);
        if (log == null)
            return NotFound(ApiResponse<SecurityLogDto>.ErrorResponse(
                new ErrorResponse("AUDIT_002", "Security log not found")));

        return Ok(ApiResponse<SecurityLogDto>.SuccessResponse(log));
    }

    /// <summary>
    /// Get audit and security statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<AuditStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuditStatisticsDto>>> GetStatistics(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var stats = await _auditService.GetAuditStatisticsAsync(days, cancellationToken);
        return Ok(ApiResponse<AuditStatisticsDto>.SuccessResponse(stats));
    }

    /// <summary>
    /// Get recent activity (audit logs)
    /// </summary>
    [HttpGet("recent-activity")]
    [ProducesResponseType(typeof(ApiResponse<List<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<AuditLogDto>>>> GetRecentActivity(
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        var activity = await _auditService.GetRecentActivityAsync(count, cancellationToken);
        return Ok(ApiResponse<List<AuditLogDto>>.SuccessResponse(activity));
    }

    /// <summary>
    /// Get recent security events
    /// </summary>
    [HttpGet("recent-security-events")]
    [ProducesResponseType(typeof(ApiResponse<List<SecurityLogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<SecurityLogDto>>>> GetRecentSecurityEvents(
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        var events = await _auditService.GetRecentSecurityEventsAsync(count, cancellationToken);
        return Ok(ApiResponse<List<SecurityLogDto>>.SuccessResponse(events));
    }

    /// <summary>
    /// Export audit logs to CSV
    /// </summary>
    [HttpPost("audit-logs/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportAuditLogs(
        [FromBody] AuditLogSearchRequest request,
        CancellationToken cancellationToken)
    {
        var csv = await _auditService.ExportAuditLogsAsync(request, cancellationToken);
        return File(csv, "text/csv", $"audit-logs-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    /// <summary>
    /// Export security logs to CSV
    /// </summary>
    [HttpPost("security-logs/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportSecurityLogs(
        [FromBody] SecurityLogSearchRequest request,
        CancellationToken cancellationToken)
    {
        var csv = await _auditService.ExportSecurityLogsAsync(request, cancellationToken);
        return File(csv, "text/csv", $"security-logs-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    /// <summary>
    /// Get all security event types (for filtering)
    /// </summary>
    [HttpGet("security-event-types")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<int, string>>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<Dictionary<int, string>>> GetSecurityEventTypes()
    {
        var eventTypes = Enum.GetValues<SecurityEventType>()
            .ToDictionary(e => (int)e, e => e.ToString());
        
        return Ok(ApiResponse<Dictionary<int, string>>.SuccessResponse(eventTypes));
    }

    /// <summary>
    /// Get all security severity levels (for filtering)
    /// </summary>
    [HttpGet("security-severity-levels")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<int, string>>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<Dictionary<int, string>>> GetSecuritySeverityLevels()
    {
        var severityLevels = Enum.GetValues<SecuritySeverity>()
            .ToDictionary(s => (int)s, s => s.ToString());
        
        return Ok(ApiResponse<Dictionary<int, string>>.SuccessResponse(severityLevels));
    }
}
