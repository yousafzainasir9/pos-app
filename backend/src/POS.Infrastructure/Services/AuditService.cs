using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Audit;
using POS.Application.Interfaces;
using POS.Domain.Entities.Audit;
using POS.Infrastructure.Data;
using System.Text;

namespace POS.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly POSDbContext _context;

    public AuditService(POSDbContext context)
    {
        _context = context;
    }

    #region Audit Logging

    public async Task LogAuditAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        AuditLogSearchRequest request, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .Include(a => a.Store)
            .AsQueryable();

        // Apply filters
        if (request.StartDate.HasValue)
            query = query.Where(a => a.Timestamp >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(a => a.Timestamp <= request.EndDate.Value);

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);

        if (!string.IsNullOrEmpty(request.Action))
            query = query.Where(a => a.Action.Contains(request.Action));

        if (!string.IsNullOrEmpty(request.EntityName))
            query = query.Where(a => a.EntityName.Contains(request.EntityName));

        if (request.StoreId.HasValue)
            query = query.Where(a => a.StoreId == request.StoreId.Value);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                UserId = a.UserId,
                UserName = a.UserName,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                Details = a.Details,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                StoreId = a.StoreId,
                StoreName = a.Store != null ? a.Store.Name : null
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<AuditLogDto?> GetAuditLogByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Include(a => a.Store)
            .Where(a => a.Id == id)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                UserId = a.UserId,
                UserName = a.UserName,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                Details = a.Details,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                StoreId = a.StoreId,
                StoreName = a.Store != null ? a.Store.Name : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    #endregion

    #region Security Logging

    public async Task LogSecurityEventAsync(SecurityLog securityLog, CancellationToken cancellationToken = default)
    {
        await _context.SecurityLogs.AddAsync(securityLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<SecurityLogDto>> GetSecurityLogsAsync(
        SecurityLogSearchRequest request, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.SecurityLogs
            .Include(s => s.Store)
            .AsQueryable();

        // Apply filters
        if (request.StartDate.HasValue)
            query = query.Where(s => s.Timestamp >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(s => s.Timestamp <= request.EndDate.Value);

        if (request.UserId.HasValue)
            query = query.Where(s => s.UserId == request.UserId.Value);

        if (request.EventType.HasValue)
            query = query.Where(s => s.EventType == request.EventType.Value);

        if (request.Severity.HasValue)
            query = query.Where(s => s.Severity == request.Severity.Value);

        if (request.Success.HasValue)
            query = query.Where(s => s.Success == request.Success.Value);

        if (request.StoreId.HasValue)
            query = query.Where(s => s.StoreId == request.StoreId.Value);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .OrderByDescending(s => s.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SecurityLogDto
            {
                Id = s.Id,
                Timestamp = s.Timestamp,
                EventType = s.EventType,
                EventTypeName = s.EventType.ToString(),
                Severity = s.Severity,
                SeverityName = s.Severity.ToString(),
                UserId = s.UserId,
                UserName = s.UserName,
                Description = s.Description,
                IpAddress = s.IpAddress,
                UserAgent = s.UserAgent,
                Success = s.Success,
                Metadata = s.Metadata,
                StoreId = s.StoreId,
                StoreName = s.Store != null ? s.Store.Name : null
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<SecurityLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<SecurityLogDto?> GetSecurityLogByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.SecurityLogs
            .Include(s => s.Store)
            .Where(s => s.Id == id)
            .Select(s => new SecurityLogDto
            {
                Id = s.Id,
                Timestamp = s.Timestamp,
                EventType = s.EventType,
                EventTypeName = s.EventType.ToString(),
                Severity = s.Severity,
                SeverityName = s.Severity.ToString(),
                UserId = s.UserId,
                UserName = s.UserName,
                Description = s.Description,
                IpAddress = s.IpAddress,
                UserAgent = s.UserAgent,
                Success = s.Success,
                Metadata = s.Metadata,
                StoreId = s.StoreId,
                StoreName = s.Store != null ? s.Store.Name : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    #endregion

    #region Statistics & Analytics

    public async Task<AuditStatisticsDto> GetAuditStatisticsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.Now.AddDays(-days);
        var last24Hours = DateTime.Now.AddHours(-24);
        var last7Days = DateTime.Now.AddDays(-7);

        var statistics = new AuditStatisticsDto
        {
            TotalAuditLogs = await _context.AuditLogs.CountAsync(cancellationToken),
            TotalSecurityLogs = await _context.SecurityLogs.CountAsync(cancellationToken),
            
            FailedLoginAttempts24h = await _context.SecurityLogs
                .Where(s => s.Timestamp >= last24Hours && 
                           s.EventType == SecurityEventType.LoginFailed)
                .CountAsync(cancellationToken),
            
            UnauthorizedAccess24h = await _context.SecurityLogs
                .Where(s => s.Timestamp >= last24Hours && 
                           s.EventType == SecurityEventType.UnauthorizedAccess)
                .CountAsync(cancellationToken),
            
            CriticalEvents7d = await _context.SecurityLogs
                .Where(s => s.Timestamp >= last7Days && 
                           s.Severity == SecuritySeverity.Critical)
                .CountAsync(cancellationToken),
            
            TopUsers = await _context.AuditLogs
                .Where(a => a.Timestamp >= startDate && a.UserId.HasValue)
                .GroupBy(a => new { a.UserId, a.UserName })
                .Select(g => new TopUserActivityDto
                {
                    UserId = g.Key.UserId!.Value,
                    UserName = g.Key.UserName ?? "Unknown",
                    ActivityCount = g.Count()
                })
                .OrderByDescending(u => u.ActivityCount)
                .Take(10)
                .ToListAsync(cancellationToken),
            
            ActivityByDay = await _context.AuditLogs
                .Where(a => a.Timestamp >= startDate)
                .GroupBy(a => a.Timestamp.Date)
                .Select(g => new ActivityByDayDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(a => a.Date)
                .ToListAsync(cancellationToken),
            
            SecurityEventCounts = await _context.SecurityLogs
                .Where(s => s.Timestamp >= startDate)
                .GroupBy(s => s.EventType)
                .Select(g => new SecurityEventCountDto
                {
                    EventType = g.Key,
                    EventTypeName = g.Key.ToString(),
                    Count = g.Count()
                })
                .OrderByDescending(e => e.Count)
                .ToListAsync(cancellationToken)
        };

        return statistics;
    }

    public async Task<List<AuditLogDto>> GetRecentActivityAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Include(a => a.Store)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                UserId = a.UserId,
                UserName = a.UserName,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Details = a.Details,
                IpAddress = a.IpAddress,
                StoreId = a.StoreId,
                StoreName = a.Store != null ? a.Store.Name : null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SecurityLogDto>> GetRecentSecurityEventsAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        return await _context.SecurityLogs
            .Include(s => s.Store)
            .OrderByDescending(s => s.Timestamp)
            .Take(count)
            .Select(s => new SecurityLogDto
            {
                Id = s.Id,
                Timestamp = s.Timestamp,
                EventType = s.EventType,
                EventTypeName = s.EventType.ToString(),
                Severity = s.Severity,
                SeverityName = s.Severity.ToString(),
                UserId = s.UserId,
                UserName = s.UserName,
                Description = s.Description,
                IpAddress = s.IpAddress,
                Success = s.Success,
                StoreId = s.StoreId,
                StoreName = s.Store != null ? s.Store.Name : null
            })
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Export

    public async Task<byte[]> ExportAuditLogsAsync(AuditLogSearchRequest request, CancellationToken cancellationToken = default)
    {
        var logs = await GetAuditLogsAsync(new AuditLogSearchRequest
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UserId = request.UserId,
            Action = request.Action,
            EntityName = request.EntityName,
            StoreId = request.StoreId,
            Page = 1,
            PageSize = 10000 // Large page for export
        }, cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("Timestamp,User,Action,Entity,EntityId,Store,IpAddress,Details");

        foreach (var log in logs.Items)
        {
            csv.AppendLine($"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{log.UserName}\",\"{log.Action}\",\"{log.EntityName}\",\"{log.EntityId}\",\"{log.StoreName}\",\"{log.IpAddress}\",\"{log.Details}\"");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportSecurityLogsAsync(SecurityLogSearchRequest request, CancellationToken cancellationToken = default)
    {
        var logs = await GetSecurityLogsAsync(new SecurityLogSearchRequest
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UserId = request.UserId,
            EventType = request.EventType,
            Severity = request.Severity,
            Success = request.Success,
            StoreId = request.StoreId,
            Page = 1,
            PageSize = 10000 // Large page for export
        }, cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("Timestamp,EventType,Severity,User,Description,IpAddress,Success,Store");

        foreach (var log in logs.Items)
        {
            csv.AppendLine($"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{log.EventTypeName}\",\"{log.SeverityName}\",\"{log.UserName}\",\"{log.Description}\",\"{log.IpAddress}\",\"{log.Success}\",\"{log.StoreName}\"");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    #endregion
}
