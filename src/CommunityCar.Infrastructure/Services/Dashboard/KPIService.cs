using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Entities.Dashboard.KPIs;
using CommunityCar.Domain.Exceptions;
using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Dashboard;

public class KPIService : IKPIService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<KPIService> _logger;

    public KPIService(ApplicationDbContext context, ILogger<KPIService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<KPIDto>> GetKPIsAsync(KPIFilterDto filter)
    {
        try
        {
            var query = _context.KPIs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                query = query.Where(k => k.Category == filter.Category);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(k => k.Name.Contains(filter.SearchTerm) || 
                                        k.Code.Contains(filter.SearchTerm) ||
                                        (k.Description != null && k.Description.Contains(filter.SearchTerm)));
            }

            if (filter.StartDate.HasValue)
            {
                var startDateOffset = new DateTimeOffset(filter.StartDate.Value, TimeSpan.Zero);
                query = query.Where(k => k.LastUpdated >= startDateOffset);
            }

            if (filter.EndDate.HasValue)
            {
                var endDateOffset = new DateTimeOffset(filter.EndDate.Value, TimeSpan.Zero);
                query = query.Where(k => k.LastUpdated <= endDateOffset);
            }

            // Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortDescending ? query.OrderByDescending(k => k.Name) : query.OrderBy(k => k.Name),
                "value" => filter.SortDescending ? query.OrderByDescending(k => k.Value) : query.OrderBy(k => k.Value),
                "category" => filter.SortDescending ? query.OrderByDescending(k => k.Category) : query.OrderBy(k => k.Category),
                _ => filter.SortDescending ? query.OrderByDescending(k => k.LastUpdated) : query.OrderBy(k => k.LastUpdated)
            };

            var totalCount = await query.CountAsync();

            var kpis = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = kpis.Select(k => MapToDto(k)).ToList();

            return new PagedResult<KPIDto>(dtos, totalCount, filter.Page, filter.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving KPIs");
            throw;
        }
    }

    public async Task<KPIDto?> GetKPIByIdAsync(Guid id)
    {
        try
        {
            var kpi = await _context.KPIs.FindAsync(id);
            return kpi != null ? MapToDto(kpi) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving KPI {Id}", id);
            throw;
        }
    }

    public async Task<KPIDto?> GetKPIByCodeAsync(string code)
    {
        try
        {
            var kpi = await _context.KPIs.FirstOrDefaultAsync(k => k.Code == code);
            return kpi != null ? MapToDto(kpi) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving KPI by code {Code}", code);
            throw;
        }
    }

    public async Task<KPI> CreateKPIAsync(CreateKPIDto dto)
    {
        try
        {
            var exists = await _context.KPIs.AnyAsync(k => k.Code == dto.Code);
            if (exists)
                throw new ConflictException($"KPI with code '{dto.Code}' already exists.");

            var kpi = new KPI(dto.Name, dto.Code, dto.Value, dto.Unit, dto.Category, dto.Description);
            
            _context.KPIs.Add(kpi);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created KPI {Code} - {Name}", kpi.Code, kpi.Name);
            return kpi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating KPI");
            throw;
        }
    }

    public async Task<KPI> UpdateKPIAsync(UpdateKPIDto dto)
    {
        try
        {
            var kpi = await _context.KPIs.FindAsync(dto.Id);
            if (kpi == null)
                throw new NotFoundException($"KPI with ID {dto.Id} not found.");

            kpi.Update(dto.Name, dto.Value, dto.Unit, dto.Category, dto.Description);
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated KPI {Id} - {Name}", kpi.Id, kpi.Name);
            return kpi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating KPI {Id}", dto.Id);
            throw;
        }
    }

    public async Task UpdateKPIValueAsync(Guid id, double newValue)
    {
        try
        {
            var kpi = await _context.KPIs.FindAsync(id);
            if (kpi == null)
                throw new NotFoundException($"KPI with ID {id} not found.");

            kpi.UpdateValue(newValue);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated KPI value {Id} from {OldValue} to {NewValue}", 
                id, kpi.PreviousValue, newValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating KPI value {Id}", id);
            throw;
        }
    }

    public async Task UpdateKPIValueByCodeAsync(string code, double newValue)
    {
        try
        {
            var kpi = await _context.KPIs.FirstOrDefaultAsync(k => k.Code == code);
            if (kpi == null)
                throw new NotFoundException($"KPI with code '{code}' not found.");

            kpi.UpdateValue(newValue);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated KPI value {Code} from {OldValue} to {NewValue}", 
                code, kpi.PreviousValue, newValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating KPI value by code {Code}", code);
            throw;
        }
    }

    public async Task DeleteKPIAsync(Guid id)
    {
        try
        {
            var kpi = await _context.KPIs.FindAsync(id);
            if (kpi == null)
                throw new NotFoundException($"KPI with ID {id} not found.");

            _context.KPIs.Remove(kpi);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted KPI {Id} - {Name}", id, kpi.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting KPI {Id}", id);
            throw;
        }
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        try
        {
            return await _context.KPIs
                .Where(k => k.Category != null)
                .Select(k => k.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving KPI categories");
            return new List<string>();
        }
    }

    public async Task<KPIStatisticsDto> GetStatisticsAsync()
    {
        try
        {
            var allKPIs = await _context.KPIs.AsNoTracking().ToListAsync();

            var stats = new KPIStatisticsDto
            {
                TotalKPIs = allKPIs.Count,
                TrendingUp = allKPIs.Count(k => k.GetTrend() == "up"),
                TrendingDown = allKPIs.Count(k => k.GetTrend() == "down"),
                Stable = allKPIs.Count(k => k.GetTrend() == "stable"),
                KPIsByCategory = allKPIs
                    .Where(k => k.Category != null)
                    .GroupBy(k => k.Category!)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            stats.TopPerformers = allKPIs
                .Where(k => k.PreviousValue.HasValue && k.GetChangePercentage() > 0)
                .OrderByDescending(k => k.GetChangePercentage())
                .Take(5)
                .Select(k => MapToDto(k))
                .ToList();

            stats.NeedsAttention = allKPIs
                .Where(k => k.PreviousValue.HasValue && k.GetChangePercentage() < 0)
                .OrderBy(k => k.GetChangePercentage())
                .Take(5)
                .Select(k => MapToDto(k))
                .ToList();

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving KPI statistics");
            throw;
        }
    }

    public async Task<List<KPIDto>> GetKPIsByCategoryAsync(string category)
    {
        try
        {
            var kpis = await _context.KPIs
                .AsNoTracking()
                .Where(k => k.Category == category)
                .OrderBy(k => k.Name)
                .ToListAsync();

            return kpis.Select(k => MapToDto(k)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving KPIs by category {Category}", category);
            return new List<KPIDto>();
        }
    }

    public async Task<Dictionary<string, double>> GetKPITrendsAsync(string code, int days = 30)
    {
        try
        {
            var kpi = await _context.KPIs.FirstOrDefaultAsync(k => k.Code == code);
            if (kpi == null)
                return new Dictionary<string, double>();

            var trends = new Dictionary<string, double>
            {
                { kpi.LastUpdated.ToString("yyyy-MM-dd"), kpi.Value }
            };

            if (kpi.PreviousValue.HasValue)
            {
                trends.Add(kpi.LastUpdated.AddDays(-1).ToString("yyyy-MM-dd"), kpi.PreviousValue.Value);
            }

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving KPI trends for {Code}", code);
            return new Dictionary<string, double>();
        }
    }

    public async Task<KPISummaryDto> GetKPISummaryAsync()
    {
        try
        {
            var allKPIs = await _context.KPIs.AsNoTracking().ToListAsync();

            var summary = new KPISummaryDto
            {
                TotalKPIs = allKPIs.Count,
                ActiveKPIs = allKPIs.Count, // All KPIs are active for now
                InactiveKPIs = 0,
                KPIsOnTarget = allKPIs.Count(k => k.Target.HasValue && k.Value >= k.Target.Value),
                KPIsBelowTarget = allKPIs.Count(k => k.Target.HasValue && k.Value < k.Target.Value),
                TrendingUp = allKPIs.Count(k => k.GetTrend() == "up"),
                TrendingDown = allKPIs.Count(k => k.GetTrend() == "down"),
                Stable = allKPIs.Count(k => k.GetTrend() == "stable"),
                KPIsByCategory = allKPIs
                    .Where(k => k.Category != null)
                    .GroupBy(k => k.Category!)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving KPI summary");
            return new KPISummaryDto();
        }
    }

    public async Task SetKPITargetAsync(Guid id, double target)
    {
        try
        {
            var kpi = await _context.KPIs.FindAsync(id);
            if (kpi == null)
                throw new NotFoundException($"KPI with ID {id} not found.");

            kpi.SetTarget(target);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Set target for KPI {Id} to {Target}", id, target);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting KPI target {Id}", id);
            throw;
        }
    }

    public async Task ActivateKPIAsync(Guid id)
    {
        try
        {
            var kpi = await _context.KPIs.FindAsync(id);
            if (kpi == null)
                throw new NotFoundException($"KPI with ID {id} not found.");

            // For now, we don't have an IsActive property on KPI entity
            // This can be implemented when needed
            _logger.LogInformation("Activated KPI {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating KPI {Id}", id);
            throw;
        }
    }

    public async Task DeactivateKPIAsync(Guid id)
    {
        try
        {
            var kpi = await _context.KPIs.FindAsync(id);
            if (kpi == null)
                throw new NotFoundException($"KPI with ID {id} not found.");

            // For now, we don't have an IsActive property on KPI entity
            // This can be implemented when needed
            _logger.LogInformation("Deactivated KPI {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating KPI {Id}", id);
            throw;
        }
    }

    private KPIDto MapToDto(KPI kpi)
    {
        var dto = new KPIDto
        {
            Id = kpi.Id,
            Slug = kpi.Slug,
            Name = kpi.Name,
            Code = kpi.Code,
            Value = kpi.Value,
            PreviousValue = kpi.PreviousValue,
            Unit = kpi.Unit,
            Category = kpi.Category,
            Description = kpi.Description,
            Target = kpi.Target,
            LastUpdated = kpi.LastUpdated,
            CreatedAt = kpi.CreatedAt,
            Trend = kpi.GetTrend(),
            TrendDirection = kpi.GetTrend(),
            IsActive = true
        };

        if (kpi.PreviousValue.HasValue && kpi.PreviousValue.Value != 0)
        {
            dto.ChangePercentage = kpi.GetChangePercentage();
        }

        if (kpi.Target.HasValue && kpi.Target.Value > 0)
        {
            dto.TargetAchievementPercentage = (kpi.Value / kpi.Target.Value) * 100;
        }

        return dto;
    }
}
