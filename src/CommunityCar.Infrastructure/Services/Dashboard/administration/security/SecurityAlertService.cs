using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Dashboard.Administration.Security;
using CommunityCar.Domain.Entities.Dashboard.security;
using CommunityCar.Domain.Enums.Dashboard.security;
using CommunityCar.Domain.Exceptions;
using CommunityCar.Domain.Interfaces.Dashboard.Administration.Security;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Dashboard.Administration.Security;

public class SecurityAlertService : ISecurityAlertService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SecurityAlertService> _logger;

    public SecurityAlertService(ApplicationDbContext context, ILogger<SecurityAlertService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<SecurityAlertDto>> GetAlertsAsync(SecurityAlertFilterDto filter)
    {
        try
        {
            var query = _context.SecurityAlerts.AsNoTracking();

            if (filter.Severity.HasValue)
            {
                query = query.Where(a => a.Severity == filter.Severity.Value);
            }

            if (filter.AlertType.HasValue)
            {
                query = query.Where(a => a.AlertType == filter.AlertType.Value);
            }

            if (filter.IsResolved.HasValue)
            {
                query = query.Where(a => a.IsResolved == filter.IsResolved.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(a => a.Title.Contains(filter.SearchTerm) ||
                                        a.Description.Contains(filter.SearchTerm) ||
                                        (a.AffectedUserName != null && a.AffectedUserName.Contains(filter.SearchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(filter.IpAddress))
            {
                query = query.Where(a => a.IpAddress == filter.IpAddress);
            }

            if (!string.IsNullOrWhiteSpace(filter.UserName))
            {
                query = query.Where(a => a.AffectedUserName == filter.UserName);
            }

            if (filter.StartDate.HasValue)
            {
                var startDateOffset = new DateTimeOffset(filter.StartDate.Value, TimeSpan.Zero);
                query = query.Where(a => a.DetectedAt >= startDateOffset);
            }

            if (filter.EndDate.HasValue)
            {
                var endDateOffset = new DateTimeOffset(filter.EndDate.Value, TimeSpan.Zero);
                query = query.Where(a => a.DetectedAt <= endDateOffset);
            }

            // Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "title" => filter.SortDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
                "severity" => filter.SortDescending ? query.OrderByDescending(a => a.Severity) : query.OrderBy(a => a.Severity),
                "alerttype" => filter.SortDescending ? query.OrderByDescending(a => a.AlertType) : query.OrderBy(a => a.AlertType),
                "isresolved" => filter.SortDescending ? query.OrderByDescending(a => a.IsResolved) : query.OrderBy(a => a.IsResolved),
                _ => filter.SortDescending ? query.OrderByDescending(a => a.DetectedAt) : query.OrderBy(a => a.DetectedAt)
            };

            var totalCount = await query.CountAsync();

            var alerts = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = alerts.Select(a => MapToDto(a)).ToList();

            return new PagedResult<SecurityAlertDto>(dtos, totalCount, filter.Page, filter.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security alerts");
            throw;
        }
    }

    public async Task<SecurityAlertDto?> GetAlertByIdAsync(Guid id)
    {
        try
        {
            var alert = await _context.SecurityAlerts.FindAsync(id);
            return alert != null ? MapToDto(alert) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security alert {Id}", id);
            throw;
        }
    }

    public async Task<SecurityAlert> CreateAlertAsync(CreateSecurityAlertDto dto)
    {
        try
        {
            var alert = new SecurityAlert(
                dto.Title,
                dto.Severity,
                dto.AlertType,
                dto.Description,
                dto.Source,
                dto.IpAddress,
                dto.UserAgent,
                dto.AffectedUserId,
                dto.AffectedUserName);

            _context.SecurityAlerts.Add(alert);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created security alert {Id} - {Title}", alert.Id, alert.Title);
            return alert;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating security alert");
            throw;
        }
    }

    public async Task<SecurityAlert> UpdateAlertAsync(UpdateSecurityAlertDto dto)
    {
        try
        {
            var alert = await _context.SecurityAlerts.FindAsync(dto.Id);
            if (alert == null)
                throw new NotFoundException($"Security alert with ID {dto.Id} not found.");

            alert.Update(dto.Title, dto.Description, dto.Severity, dto.AlertType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated security alert {Id} - {Title}", alert.Id, alert.Title);
            return alert;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating security alert {Id}", dto.Id);
            throw;
        }
    }

    public async Task DeleteAlertAsync(Guid id)
    {
        try
        {
            var alert = await _context.SecurityAlerts.FindAsync(id);
            if (alert == null)
                throw new NotFoundException($"Security alert with ID {id} not found.");

            _context.SecurityAlerts.Remove(alert);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted security alert {Id} - {Title}", id, alert.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting security alert {Id}", id);
            throw;
        }
    }

    public async Task ResolveAlertAsync(Guid id, Guid resolvedById, string resolvedByName, string? resolutionNotes = null)
    {
        try
        {
            var alert = await _context.SecurityAlerts.FindAsync(id);
            if (alert == null)
                throw new NotFoundException($"Security alert with ID {id} not found.");

            alert.Resolve(resolvedById, resolvedByName, resolutionNotes);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Resolved security alert {Id} by {ResolvedBy}", id, resolvedByName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving security alert {Id}", id);
            throw;
        }
    }

    public async Task ReopenAlertAsync(Guid id)
    {
        try
        {
            var alert = await _context.SecurityAlerts.FindAsync(id);
            if (alert == null)
                throw new NotFoundException($"Security alert with ID {id} not found.");

            alert.Reopen();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Reopened security alert {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reopening security alert {Id}", id);
            throw;
        }
    }

    public async Task<SecurityAlertStatisticsDto> GetStatisticsAsync(int days = 30)
    {
        try
        {
            var startDate = DateTimeOffset.UtcNow.AddDays(-days);
            var alerts = await _context.SecurityAlerts
                .AsNoTracking()
                .Where(a => a.DetectedAt >= startDate)
                .ToListAsync();

            var stats = new SecurityAlertStatisticsDto
            {
                TotalAlerts = alerts.Count,
                UnresolvedAlerts = alerts.Count(a => !a.IsResolved),
                ResolvedAlerts = alerts.Count(a => a.IsResolved),
                CriticalAlerts = alerts.Count(a => a.Severity == SecuritySeverity.Critical),
                HighAlerts = alerts.Count(a => a.Severity == SecuritySeverity.High),
                MediumAlerts = alerts.Count(a => a.Severity == SecuritySeverity.Medium),
                LowAlerts = alerts.Count(a => a.Severity == SecuritySeverity.Low),
                AlertsByType = alerts
                    .GroupBy(a => a.AlertType.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                AlertsBySeverity = alerts
                    .GroupBy(a => a.Severity.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                RecentAlerts = alerts
                    .OrderByDescending(a => a.DetectedAt)
                    .Take(10)
                    .Select(a => MapToDto(a))
                    .ToList(),
                CriticalUnresolved = alerts
                    .Where(a => !a.IsResolved && a.Severity == SecuritySeverity.Critical)
                    .OrderByDescending(a => a.DetectedAt)
                    .Take(10)
                    .Select(a => MapToDto(a))
                    .ToList()
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security alert statistics");
            throw;
        }
    }

    public async Task<List<SecurityAlertDto>> GetUnresolvedAlertsAsync(int count = 10)
    {
        try
        {
            var alerts = await _context.SecurityAlerts
                .AsNoTracking()
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.Severity)
                .ThenByDescending(a => a.DetectedAt)
                .Take(count)
                .ToListAsync();

            return alerts.Select(a => MapToDto(a)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unresolved alerts");
            return new List<SecurityAlertDto>();
        }
    }

    public async Task<List<SecurityAlertDto>> GetCriticalAlertsAsync()
    {
        try
        {
            var alerts = await _context.SecurityAlerts
                .AsNoTracking()
                .Where(a => a.Severity == SecuritySeverity.Critical && !a.IsResolved)
                .OrderByDescending(a => a.DetectedAt)
                .ToListAsync();

            return alerts.Select(a => MapToDto(a)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving critical alerts");
            return new List<SecurityAlertDto>();
        }
    }

    public async Task<List<SecurityAlertDto>> GetAlertsByUserAsync(Guid userId)
    {
        try
        {
            var alerts = await _context.SecurityAlerts
                .AsNoTracking()
                .Where(a => a.AffectedUserId == userId)
                .OrderByDescending(a => a.DetectedAt)
                .ToListAsync();

            return alerts.Select(a => MapToDto(a)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alerts for user {UserId}", userId);
            return new List<SecurityAlertDto>();
        }
    }

    public async Task<Dictionary<string, int>> GetAlertTrendsAsync(int days = 30)
    {
        try
        {
            var startDate = DateTimeOffset.UtcNow.AddDays(-days);
            var alerts = await _context.SecurityAlerts
                .AsNoTracking()
                .Where(a => a.DetectedAt >= startDate)
                .ToListAsync();

            var trends = alerts
                .GroupBy(a => a.DetectedAt.Date.ToString("yyyy-MM-dd"))
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count());

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alert trends");
            return new Dictionary<string, int>();
        }
    }

    public async Task<SecuritySummaryDto> GetSummaryAsync()
    {
        try
        {
            var allAlerts = await _context.SecurityAlerts.AsNoTracking().ToListAsync();
            var now = DateTimeOffset.UtcNow;

            var resolvedAlerts = allAlerts.Where(a => a.IsResolved && a.ResolvedAt.HasValue).ToList();
            var resolutionTimes = resolvedAlerts
                .Select(a => a.ResolvedAt!.Value - a.DetectedAt)
                .ToList();

            var summary = new SecuritySummaryDto
            {
                TotalAlerts = allAlerts.Count,
                UnresolvedAlerts = allAlerts.Count(a => !a.IsResolved),
                CriticalAlerts = allAlerts.Count(a => a.Severity == SecuritySeverity.Critical && !a.IsResolved),
                AlertsToday = allAlerts.Count(a => a.DetectedAt.Date == now.Date),
                AlertsThisWeek = allAlerts.Count(a => a.DetectedAt >= now.AddDays(-7)),
                AlertsThisMonth = allAlerts.Count(a => a.DetectedAt >= now.AddMonths(-1)),
                ResolutionRate = allAlerts.Count > 0 ? (double)resolvedAlerts.Count / allAlerts.Count * 100 : 0,
                AverageResolutionTime = resolutionTimes.Any() 
                    ? TimeSpan.FromTicks((long)resolutionTimes.Average(t => t.Ticks))
                    : TimeSpan.Zero
            };

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security summary");
            return new SecuritySummaryDto();
        }
    }

    private SecurityAlertDto MapToDto(SecurityAlert alert)
    {
        return new SecurityAlertDto
        {
            Id = alert.Id,
            Slug = alert.Slug,
            Title = alert.Title,
            Severity = alert.Severity,
            AlertType = alert.AlertType,
            Description = alert.Description,
            Source = alert.Source,
            IpAddress = alert.IpAddress,
            UserAgent = alert.UserAgent,
            AffectedUserId = alert.AffectedUserId,
            AffectedUserName = alert.AffectedUserName,
            IsResolved = alert.IsResolved,
            ResolvedById = alert.ResolvedById,
            ResolvedByName = alert.ResolvedByName,
            ResolvedAt = alert.ResolvedAt,
            ResolutionNotes = alert.ResolutionNotes,
            DetectedAt = alert.DetectedAt,
            CreatedAt = alert.CreatedAt
        };
    }
}
