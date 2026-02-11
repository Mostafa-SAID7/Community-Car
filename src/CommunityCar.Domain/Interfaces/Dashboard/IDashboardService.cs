using CommunityCar.Domain.Models;

namespace CommunityCar.Domain.Interfaces.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummary> GetSummaryAsync();
    Task<IEnumerable<KPIValue>> GetWeeklyActivityAsync();
    Task<IEnumerable<KPIValue>> GetContentDistributionAsync();
    Task<IEnumerable<KPIValue>> GetUserGrowthAsync();
    Task<IEnumerable<KPIValue>> GetTopContentTypesAsync();
    Task<Dictionary<string, int>> GetEngagementMetricsAsync();
}
