using CommunityCar.Domain.Models;

namespace CommunityCar.Domain.Interfaces.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummary> GetSummaryAsync();
    Task<IEnumerable<KPIValue>> GetWeeklyActivityAsync();
}
