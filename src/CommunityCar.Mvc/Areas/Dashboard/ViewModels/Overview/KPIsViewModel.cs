using CommunityCar.Domain.Models;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels.Overview;

public class KPIsViewModel
{
    public DashboardSummary Summary { get; set; } = null!;
    public List<ChartDataViewModel> ContentDistribution { get; set; } = new();
    public List<ChartDataViewModel> EngagementMetrics { get; set; } = new();
    public List<ChartDataViewModel> UserGrowth { get; set; } = new();
}

public class ChartDataViewModel
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    
    public ChartDataViewModel() { }
    
    public ChartDataViewModel(string label, double value)
    {
        Label = label;
        Value = value;
    }
}
