using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Models;

namespace CommunityCar.Domain.Interfaces.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummary> GetSummaryAsync();
    Task<IEnumerable<KPIValue>> GetWeeklyActivityAsync();
    Task<IEnumerable<KPIValue>> GetActivityByPeriodAsync(string period);
    Task<IEnumerable<KPIValue>> GetContentDistributionAsync();
    Task<IEnumerable<KPIValue>> GetUserGrowthAsync();
    Task<IEnumerable<KPIValue>> GetTopContentTypesAsync();
    Task<Dictionary<string, int>> GetEngagementMetricsAsync();
    Task<Dictionary<string, int>> GetUsersByLocationAsync();
    Task<Dictionary<string, int>> GetActiveUsersByLocationAsync();
    Task<IEnumerable<RecentActivityDto>> GetRecentActivityAsync(int count = 10);
}
