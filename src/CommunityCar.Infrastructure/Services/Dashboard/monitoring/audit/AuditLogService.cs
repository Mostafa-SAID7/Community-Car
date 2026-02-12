using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Dashboard.Monitoring.Audit;
using CommunityCar.Domain.Entities.Dashboard.security;
using CommunityCar.Domain.Interfaces.Dashboard.Monitoring.Audit;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Dashboard.Monitoring.Audit;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(ApplicationDbContext context, ILogger<AuditLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogFilterDto filter)
    {
        try
        {
            var query = _context.AuditLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.UserName))
            {
                query = query.Where(a => a.UserName != null && a.UserName.Contains(filter.UserName));
            }

            if (!string.IsNullOrWhiteSpace(filter.EntityName))
            {
                query = query.Where(a => a.EntityName == filter.EntityName);
            }

            if (!string.IsNullOrWhiteSpace(filter.Action))
            {
                query = query.Where(a => a.Action == filter.Action);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= filter.EndDate.Value);
            }

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.UserName,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    Action = a.Action,
                    Description = a.Description,
                    OldValues = a.OldValues,
                    NewValues = a.NewValues,
                    AffectedColumns = a.AffectedColumns,
                    CreatedAt = a.CreatedAt.DateTime
                })
                .ToListAsync();

            return new PagedResult<AuditLogDto>(logs, totalCount, filter.Page, filter.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            throw;
        }
    }

    public async Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id)
    {
        try
        {
            var log = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.UserName,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    Action = a.Action,
                    Description = a.Description,
                    OldValues = a.OldValues,
                    NewValues = a.NewValues,
                    AffectedColumns = a.AffectedColumns,
                    CreatedAt = a.CreatedAt.DateTime
                })
                .FirstOrDefaultAsync();

            return log;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log {Id}", id);
            throw;
        }
    }

    public async Task<List<string>> GetDistinctEntityNamesAsync()
    {
        try
        {
            return await _context.AuditLogs
                .Select(a => a.EntityName)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving distinct entity names");
            return new List<string>();
        }
    }

    public async Task<List<string>> GetDistinctActionsAsync()
    {
        try
        {
            return await _context.AuditLogs
                .Select(a => a.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving distinct actions");
            return new List<string>();
        }
    }

    public async Task<Dictionary<string, int>> GetAuditLogStatisticsAsync(int days = 30)
    {
        try
        {
            var startDate = DateTimeOffset.UtcNow.AddDays(-days);
            
            var stats = await _context.AuditLogs
                .Where(a => a.CreatedAt >= startDate)
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Action, x => x.Count);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log statistics");
            return new Dictionary<string, int>();
        }
    }

    public async Task LogActionAsync(Guid? userId, string userName, string entityName, string entityId, 
        string action, string? description = null, string? oldValues = null, string? newValues = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                Description = description,
                OldValues = oldValues,
                NewValues = newValues
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging audit action");
        }
    }
}
