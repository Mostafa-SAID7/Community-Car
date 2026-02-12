namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels.Overview;

public class TrendsViewModel
{
    public List<ChartDataViewModel> UserGrowth { get; set; } = new();
    public List<ChartDataViewModel> WeeklyActivity { get; set; } = new();
    public List<ChartDataViewModel> ContentDistribution { get; set; } = new();
    public string CurrentPeriod { get; set; } = "week";
}

public class TrendDataResponse
{
    public bool Success { get; set; }
    public List<double> Data { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public string? Message { get; set; }
}
