using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Dashboard;

namespace CommunityCar.Domain.Interfaces.Dashboard;

public interface IUserActivityService
{
    Task<PagedResult<UserActivityDto>> GetUserActivitiesAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? activityType = null);

    Task<UserActivityDto?> GetUserActivityByIdAsync(int id);

    Task<PagedResult<UserActivityDto>> GetActivitiesByUserIdAsync(
        string userId,
        int page,
        int pageSize);

    Task<UserActivitySummary> GetUserActivitySummaryAsync(string userId);

    Task<UserActivityStatistics> GetUserActivityStatisticsAsync(
        DateTime startDate,
        DateTime endDate);

    Task<List<TimelineActivity>> GetUserActivityTimelineAsync(
        string userId,
        DateTime startDate,
        DateTime endDate);

    Task<List<ActiveUserInfo>> GetActiveUsersAsync(int hours);

    Task<bool> DeleteUserActivityAsync(int id);

    Task<int> BulkDeleteUserActivitiesAsync(int[] ids);

    Task<byte[]> ExportUserActivitiesAsync(
        DateTime startDate,
        DateTime endDate,
        string? format = "csv");

    Task LogUserActivityAsync(
        string userId,
        string activityType,
        string description,
        string? ipAddress = null,
        string? userAgent = null,
        string? location = null,
        Dictionary<string, object>? metadata = null);
}
