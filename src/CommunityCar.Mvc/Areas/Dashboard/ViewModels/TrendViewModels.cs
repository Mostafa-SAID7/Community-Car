using CommunityCar.Domain.DTOs.Dashboard;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels;

public class TrendIndexViewModel
{
    public KPISummaryDto KPISummary { get; set; } = new();
    public SecurityAlertStatisticsDto SecurityStatistics { get; set; } = new();
    public Dictionary<string, int> AuditStatistics { get; set; } = new();
    public int SelectedDays { get; set; } = 30;
}

public class KPITrendsViewModel
{
    public KPISummaryDto Summary { get; set; } = new();
    public List<KPIDto> AllKPIs { get; set; } = new();
    public int SelectedDays { get; set; } = 30;
}

public class SecurityTrendsViewModel
{
    public SecurityAlertStatisticsDto Statistics { get; set; } = new();
    public Dictionary<string, int> Trends { get; set; } = new();
    public int SelectedDays { get; set; } = 30;
}

public class ActivityTrendsViewModel
{
    public Dictionary<string, int> AuditStatistics { get; set; } = new();
    public int SelectedDays { get; set; } = 30;
}
