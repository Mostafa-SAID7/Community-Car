using CommunityCar.Domain.Base.Interfaces;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Entities.Dashboard.security;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CommunityCar.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditableEntityInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ProcessEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ProcessEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ProcessEntities(DbContext? context)
    {
        if (context == null) return;

        var now = DateTimeOffset.UtcNow;
        var user = _currentUserService.UserName ?? "System";
        var auditLogs = new List<AuditLog>();

        // 1. First pass: update auditable properties and identify logs
        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            if (entry.Entity is IAuditable auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAt = now;
                    auditable.CreatedBy = user;
                }
                else if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
                {
                    auditable.ModifiedAt = now;
                    auditable.ModifiedBy = user;
                }
            }

            if (entry.Entity is ISoftDelete softDelete && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDelete.IsDeleted = true;
                softDelete.DeletedAt = now;
                softDelete.DeletedBy = user;

                var log = CreateAuditLogEntry(context, entry, "SoftDelete", user, now);
                if (log != null) auditLogs.Add(log);
            }
            else if (entry.State == EntityState.Modified)
            {
                var log = CreateAuditLogEntry(context, entry, "Update", user, now);
                if (log != null) auditLogs.Add(log);
            }
            else if (entry.State == EntityState.Added)
            {
                var log = CreateAuditLogEntry(context, entry, "Create", user, now);
                if (log != null) auditLogs.Add(log);
            }
        }

        // 2. Second pass: add audit logs to context
        if (auditLogs.Any())
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }
    }

    private AuditLog? CreateAuditLogEntry(DbContext context, Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string action, string user, DateTimeOffset now)
    {
        if (entry.Entity is AuditLog || entry.Entity is SecurityAlert) return null;

        var auditLog = new AuditLog
        {
            UserId = _currentUserService.UserId,
            UserName = user,
            EntityName = entry.Entity.GetType().Name,
            EntityId = entry.Property("Id").CurrentValue?.ToString() ?? "0",
            Action = action,
            CreatedAt = now,
            CreatedBy = user
        };

        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();
        var affectedColumns = new List<string>();

        foreach (var property in entry.Properties)
        {
            string propertyName = property.Metadata.Name;
            if (property.Metadata.IsPrimaryKey()) continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    newValues[propertyName] = property.CurrentValue;
                    affectedColumns.Add(propertyName);
                    break;

                case EntityState.Deleted:
                    oldValues[propertyName] = property.OriginalValue;
                    affectedColumns.Add(propertyName);
                    break;

                case EntityState.Modified:
                    if (property.IsModified)
                    {
                        oldValues[propertyName] = property.OriginalValue;
                        newValues[propertyName] = property.CurrentValue;
                        affectedColumns.Add(propertyName);
                    }
                    break;
            }
        }

        auditLog.OldValues = oldValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(oldValues);
        auditLog.NewValues = newValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(newValues);
        auditLog.AffectedColumns = affectedColumns.Count == 0 ? null : string.Join(",", affectedColumns);

        return auditLog;
    }
}

public static class EntityEntryExtensions
{
    public static bool HasChangedOwnedEntities(this Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry) 
        => entry.References.Any(r => 
            r.TargetEntry != null && 
            r.TargetEntry.Metadata.IsOwned() && 
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}
