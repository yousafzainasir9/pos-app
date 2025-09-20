using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using POS.Domain.Common;

namespace POS.Infrastructure.Data.Interceptors;

public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public AuditableEntitySaveChangesInterceptor(
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
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

        var entries = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
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
        }
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
