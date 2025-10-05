using POS.Application.DTOs.Audit;
using POS.Domain.Entities.Audit;

namespace POS.Application.Interfaces;

public interface IAuditService
{
    // Audit Logging
    Task LogAuditAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogSearchRequest request, CancellationToken cancellationToken = default);
    Task<AuditLogDto?> GetAuditLogByIdAsync(long id, CancellationToken cancellationToken = default);
    
    // Security Logging
    Task LogSecurityEventAsync(SecurityLog securityLog, CancellationToken cancellationToken = default);
    Task<PagedResult<SecurityLogDto>> GetSecurityLogsAsync(SecurityLogSearchRequest request, CancellationToken cancellationToken = default);
    Task<SecurityLogDto?> GetSecurityLogByIdAsync(long id, CancellationToken cancellationToken = default);
    
    // Statistics & Analytics
    Task<AuditStatisticsDto> GetAuditStatisticsAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<List<AuditLogDto>> GetRecentActivityAsync(int count = 20, CancellationToken cancellationToken = default);
    Task<List<SecurityLogDto>> GetRecentSecurityEventsAsync(int count = 20, CancellationToken cancellationToken = default);
    
    // Export
    Task<byte[]> ExportAuditLogsAsync(AuditLogSearchRequest request, CancellationToken cancellationToken = default);
    Task<byte[]> ExportSecurityLogsAsync(SecurityLogSearchRequest request, CancellationToken cancellationToken = default);
}
