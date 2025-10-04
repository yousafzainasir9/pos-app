namespace POS.Application.DTOs;

public class AuditLogDto
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
}

public class RecentActivityDto
{
    public DateTime Timestamp { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
