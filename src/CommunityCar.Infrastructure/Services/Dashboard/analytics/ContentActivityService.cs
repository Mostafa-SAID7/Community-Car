using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Dashboard.Analytics;
using CommunityCar.Domain.Interfaces.Dashboard.Analytics;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CommunityCar.Infrastructure.Services.Dashboard.Analytics;

public class ContentActivityService : IContentActivityService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ContentActivityService> _logger;

    public ContentActivityService(
        ApplicationDbContext context,
        ILogger<ContentActivityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<ContentActivityDto>> GetContentActivitiesAsync(
        int page, 
        int pageSize, 
        string? userId = null, 
        string? contentType = null)
    {
        try
        {
            var query = _context.Set<ContentActivity>().AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(a => a.UserId == userId);
            }

            if (!string.IsNullOrEmpty(contentType))
            {
                query = query.Where(a => a.ContentType == contentType);
            }

            var totalCount = await query.CountAsync();

            var activities = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ContentActivityDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.UserName,
                    ContentType = a.ContentType,
                    ContentId = a.ContentId,
                    ContentTitle = a.ContentTitle,
                    Action = a.Action,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    CreatedAt = a.CreatedAt,
                    Metadata = a.Metadata
                })
                .ToListAsync();

            return new PagedResult<ContentActivityDto>
            {
                Items = activities,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting content activities");
            throw;
        }
    }

    public async Task<ContentActivityDto?> GetContentActivityByIdAsync(int id)
    {
        try
        {
            return await _context.Set<ContentActivity>()
                .Where(a => a.Id == id)
                .Select(a => new ContentActivityDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.UserName,
                    ContentType = a.ContentType,
                    ContentId = a.ContentId,
                    ContentTitle = a.ContentTitle,
                    Action = a.Action,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    CreatedAt = a.CreatedAt,
                    Metadata = a.Metadata
                })
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting content activity by ID: {Id}", id);
            throw;
        }
    }

    public async Task<PagedResult<ContentActivityDto>> GetUserContentActivitiesAsync(
        string userId, 
        int page, 
        int pageSize)
    {
        return await GetContentActivitiesAsync(page, pageSize, userId);
    }

    public async Task<ContentActivityStatistics> GetContentActivityStatisticsAsync(
        DateTime startDate, 
        DateTime endDate)
    {
        try
        {
            var activities = await _context.Set<ContentActivity>()
                .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
                .ToListAsync();

            var statistics = new ContentActivityStatistics
            {
                TotalActivities = activities.Count,
                UniqueUsers = activities.Select(a => a.UserId).Distinct().Count(),
                TotalViews = activities.Count(a => a.Action == "View"),
                TotalCreates = activities.Count(a => a.Action == "Create"),
                TotalUpdates = activities.Count(a => a.Action == "Update"),
                TotalDeletes = activities.Count(a => a.Action == "Delete"),
                ActivitiesByContentType = activities
                    .GroupBy(a => a.ContentType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ActivitiesByAction = activities
                    .GroupBy(a => a.Action)
                    .ToDictionary(g => g.Key, g => g.Count()),
                TopUsers = activities
                    .GroupBy(a => new { a.UserId, a.UserName })
                    .Select(g => new TopUserActivity
                    {
                        UserId = g.Key.UserId,
                        UserName = g.Key.UserName,
                        ActivityCount = g.Count()
                    })
                    .OrderByDescending(u => u.ActivityCount)
                    .Take(10)
                    .ToList(),
                TopContent = activities
                    .Where(a => a.Action == "View")
                    .GroupBy(a => new { a.ContentType, a.ContentId, a.ContentTitle })
                    .Select(g => new TopContentActivity
                    {
                        ContentType = g.Key.ContentType,
                        ContentId = g.Key.ContentId,
                        ContentTitle = g.Key.ContentTitle,
                        ViewCount = g.Count()
                    })
                    .OrderByDescending(c => c.ViewCount)
                    .Take(10)
                    .ToList()
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting content activity statistics");
            throw;
        }
    }

    public async Task<bool> DeleteContentActivityAsync(int id)
    {
        try
        {
            var activity = await _context.Set<ContentActivity>().FindAsync(id);
            if (activity == null)
                return false;

            _context.Set<ContentActivity>().Remove(activity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting content activity: {Id}", id);
            throw;
        }
    }

    public async Task<int> BulkDeleteContentActivitiesAsync(int[] ids)
    {
        try
        {
            var activities = await _context.Set<ContentActivity>()
                .Where(a => ids.Contains(a.Id))
                .ToListAsync();

            _context.Set<ContentActivity>().RemoveRange(activities);
            await _context.SaveChangesAsync();
            return activities.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting content activities");
            throw;
        }
    }

    public async Task<byte[]> ExportContentActivitiesAsync(
        DateTime startDate, 
        DateTime endDate, 
        string? format = "csv")
    {
        try
        {
            var activities = await _context.Set<ContentActivity>()
                .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            if (format?.ToLower() == "csv")
            {
                var csv = new StringBuilder();
                csv.AppendLine("Id,UserId,UserName,ContentType,ContentId,ContentTitle,Action,IpAddress,CreatedAt");

                foreach (var activity in activities)
                {
                    csv.AppendLine($"{activity.Id},{activity.UserId},{activity.UserName},{activity.ContentType},{activity.ContentId},\"{activity.ContentTitle}\",{activity.Action},{activity.IpAddress},{activity.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                }

                return Encoding.UTF8.GetBytes(csv.ToString());
            }

            // Default to JSON
            var json = System.Text.Json.JsonSerializer.Serialize(activities, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            return Encoding.UTF8.GetBytes(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting content activities");
            throw;
        }
    }

    public async Task LogActivityAsync(
        string userId, 
        string contentType, 
        string contentId, 
        string contentTitle, 
        string action, 
        string? ipAddress = null, 
        string? userAgent = null, 
        Dictionary<string, object>? metadata = null)
    {
        try
        {
            var activity = new ContentActivity
            {
                UserId = userId,
                UserName = await GetUserNameAsync(userId),
                ContentType = contentType,
                ContentId = contentId,
                ContentTitle = contentTitle,
                Action = action,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<ContentActivity>().Add(activity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging content activity");
            // Don't throw - logging should not break the main flow
        }
    }

    private async Task<string> GetUserNameAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.UserName ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}

// Entity class for ContentActivity
public class ContentActivity
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string ContentId { get; set; } = string.Empty;
    public string ContentTitle { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
