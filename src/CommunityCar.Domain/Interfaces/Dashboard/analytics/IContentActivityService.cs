using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Dashboard.Analytics;

namespace CommunityCar.Domain.Interfaces.Dashboard.Analytics;

public interface IContentActivityService
{
    Task<PagedResult<ContentActivityDto>> GetContentActivitiesAsync(
        int page, 
        int pageSize, 
        string? userId = null, 
        string? contentType = null);
    
    Task<ContentActivityDto?> GetContentActivityByIdAsync(int id);
    
    Task<PagedResult<ContentActivityDto>> GetUserContentActivitiesAsync(
        string userId, 
        int page, 
        int pageSize);
    
    Task<ContentActivityStatistics> GetContentActivityStatisticsAsync(
        DateTime startDate, 
        DateTime endDate);
    
    Task<bool> DeleteContentActivityAsync(int id);
    
    Task<int> BulkDeleteContentActivitiesAsync(int[] ids);
    
    Task<byte[]> ExportContentActivitiesAsync(
        DateTime startDate, 
        DateTime endDate, 
        string? format = "csv");
    
    Task LogActivityAsync(
        string userId, 
        string contentType, 
        string contentId, 
        string contentTitle, 
        string action, 
        string? ipAddress = null, 
        string? userAgent = null, 
        Dictionary<string, object>? metadata = null);
}
