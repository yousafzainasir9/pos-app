using POS.Domain.Entities.Audit;

namespace POS.Application.DTOs.Audit;

public class AuditLogDto
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public long? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public long? StoreId { get; set; }
    public string? StoreName { get; set; }
}

public class SecurityLogDto
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public SecurityEventType EventType { get; set; }
    public string EventTypeName { get; set; } = string.Empty;
    public SecuritySeverity Severity { get; set; }
    public string SeverityName { get; set; } = string.Empty;
    public long? UserId { get; set; }
    public string? UserName { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Success { get; set; }
    public string? Metadata { get; set; }
    public long? StoreId { get; set; }
    public string? StoreName { get; set; }
}

public class AuditLogSearchRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public long? UserId { get; set; }
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public long? StoreId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class SecurityLogSearchRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public long? UserId { get; set; }
    public SecurityEventType? EventType { get; set; }
    public SecuritySeverity? Severity { get; set; }
    public bool? Success { get; set; }
    public long? StoreId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class AuditStatisticsDto
{
    public int TotalAuditLogs { get; set; }
    public int TotalSecurityLogs { get; set; }
    public int FailedLoginAttempts24h { get; set; }
    public int UnauthorizedAccess24h { get; set; }
    public int CriticalEvents7d { get; set; }
    public List<TopUserActivityDto> TopUsers { get; set; } = new();
    public List<ActivityByDayDto> ActivityByDay { get; set; } = new();
    public List<SecurityEventCountDto> SecurityEventCounts { get; set; } = new();
}

public class TopUserActivityDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ActivityCount { get; set; }
}

public class ActivityByDayDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class SecurityEventCountDto
{
    public SecurityEventType EventType { get; set; }
    public string EventTypeName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
