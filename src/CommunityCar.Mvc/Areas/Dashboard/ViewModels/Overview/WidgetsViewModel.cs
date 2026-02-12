using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Models;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels.Overview;

public class WidgetsViewModel
{
    public DashboardSummary Summary { get; set; } = null!;
    public List<RecentActivityDto> RecentActivity { get; set; } = new();
    public List<ChartDataViewModel> ContentDistribution { get; set; } = new();
    public Dictionary<string, int> UsersByLocation { get; set; } = new();
}
