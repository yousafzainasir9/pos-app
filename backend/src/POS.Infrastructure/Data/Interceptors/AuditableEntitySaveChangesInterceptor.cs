using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using POS.Domain.Common;
using POS.Domain.Entities.Audit;
using System.Text.Json;

namespace POS.Infrastructure.Data.Interceptors;

public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditableEntitySaveChangesInterceptor(
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        IHttpContextAccessor httpContextAccessor)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var auditLogs = new List<AuditLog>();

        var entries = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            // Update audit fields
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOn = _dateTimeService.Now;
                entry.Entity.CreatedByUserId = _currentUserService.UserId;
                
                if (entry.Entity is AuditableEntity auditableEntity)
                {
                    auditableEntity.CreatedBy = _currentUserService.Username;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedOn = _dateTimeService.Now;
                entry.Entity.ModifiedByUserId = _currentUserService.UserId;
                
                if (entry.Entity is AuditableEntity auditableEntity)
                {
                    auditableEntity.ModifiedBy = _currentUserService.Username;
                }
            }

            // Create audit log entry (skip AuditLog and SecurityLog entities to avoid recursion)
            var entityType = entry.Entity.GetType();
            if (entityType != typeof(AuditLog) && entityType != typeof(SecurityLog))
            {
                var auditLog = CreateAuditLog(entry);
                if (auditLog != null)
                {
                    auditLogs.Add(auditLog);
                }
            }
        }

        // Add audit logs to context
        if (auditLogs.Any() && context is Data.POSDbContext posContext)
        {
            posContext.AuditLogs.AddRange(auditLogs);
        }
    }

    private AuditLog? CreateAuditLog(EntityEntry entry)
    {
        try
        {
            var entityType = entry.Entity.GetType().Name;
            var entityId = GetEntityId(entry);
            var action = GetAction(entry.State);

            var auditLog = new AuditLog
            {
                Timestamp = _dateTimeService.UtcNow,
                UserId = _currentUserService.UserId,
                UserName = _currentUserService.Username,
                Action = action,
                EntityName = entityType,
                EntityId = entityId,
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString(),
                StoreId = GetStoreId(entry)
            };

            if (entry.State == EntityState.Modified)
            {
                var oldValues = GetOldValues(entry);
                var newValues = GetNewValues(entry);
                
                auditLog.OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null;
                auditLog.NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null;
                auditLog.Details = $"{entityType} updated";
            }
            else if (entry.State == EntityState.Added)
            {
                var newValues = GetNewValues(entry);
                auditLog.NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null;
                auditLog.Details = $"{entityType} created";
            }
            else if (entry.State == EntityState.Deleted)
            {
                var oldValues = GetOldValues(entry);
                auditLog.OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null;
                auditLog.Details = $"{entityType} deleted";
            }

            return auditLog;
        }
        catch
        {
            // If there's an error creating the audit log, don't break the main operation
            return null;
        }
    }

    private string GetAction(EntityState state)
    {
        return state switch
        {
            EntityState.Added => "CREATE",
            EntityState.Modified => "UPDATE",
            EntityState.Deleted => "DELETE",
            _ => "UNKNOWN"
        };
    }

    private string? GetEntityId(EntityEntry entry)
    {
        var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
        return idProperty?.CurrentValue?.ToString();
    }

    private long? GetStoreId(EntityEntry entry)
    {
        var storeIdProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "StoreId");
        if (storeIdProperty?.CurrentValue != null && long.TryParse(storeIdProperty.CurrentValue.ToString(), out var storeId))
        {
            return storeId;
        }
        return null;
    }

    private Dictionary<string, object?>? GetOldValues(EntityEntry entry)
    {
        if (entry.State == EntityState.Added) return null;

        var values = new Dictionary<string, object?>();
        foreach (var property in entry.Properties)
        {
            // Skip navigation properties and audit fields
            if (ShouldIncludeProperty(property.Metadata.Name))
            {
                values[property.Metadata.Name] = property.OriginalValue;
            }
        }
        return values.Any() ? values : null;
    }

    private Dictionary<string, object?>? GetNewValues(EntityEntry entry)
    {
        if (entry.State == EntityState.Deleted) return null;

        var values = new Dictionary<string, object?>();
        foreach (var property in entry.Properties)
        {
            // Skip navigation properties and audit fields
            if (ShouldIncludeProperty(property.Metadata.Name))
            {
                values[property.Metadata.Name] = property.CurrentValue;
            }
        }
        return values.Any() ? values : null;
    }

    private bool ShouldIncludeProperty(string propertyName)
    {
        // Exclude audit metadata fields to reduce noise
        var excludedProperties = new[]
        {
            "CreatedOn", "CreatedBy", "CreatedByUserId",
            "ModifiedOn", "ModifiedBy", "ModifiedByUserId",
            "DeletedOn", "DeletedBy", "DeletedByUserId",
            "IsDeleted"
        };

        return !excludedProperties.Contains(propertyName);
    }
}

public interface ICurrentUserService
{
    long? UserId { get; }
    string? Username { get; }
    string? Email { get; }
}

public interface IDateTimeService
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}
