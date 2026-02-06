namespace CommunityCar.Domain.DTOs.Dashboard;

public class KPISummaryDto
{
    public int TotalKPIs { get; set; }
    public int ActiveKPIs { get; set; }
    public int InactiveKPIs { get; set; }
    public int KPIsOnTarget { get; set; }
    public int KPIsBelowTarget { get; set; }
    public int TrendingUp { get; set; }
    public int TrendingDown { get; set; }
    public int Stable { get; set; }
    public Dictionary<string, int> KPIsByCategory { get; set; } = new();
}
