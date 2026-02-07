namespace CommunityCar.Domain.DTOs.Dashboard;

public class TrendMetricDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public double PreviousValue { get; set; }
    public double ChangePercentage { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
    public string TimePeriod { get; set; } = "Daily";
    public DateTimeOffset LastCalculated { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class TrendOverviewDto
{
    public int TotalMetrics { get; set; }
    public int TrendingUp { get; set; }
    public int TrendingDown { get; set; }
    public int Stable { get; set; }
    public List<TrendMetricDto> TopGrowingMetrics { get; set; } = new();
    public List<TrendMetricDto> TopDecliningMetrics { get; set; } = new();
    public Dictionary<string, int> MetricsByCategory { get; set; } = new();
}

public class TrendAnalysisDto
{
    public string MetricName { get; set; } = string.Empty;
    public Dictionary<string, double> HistoricalData { get; set; } = new();
    public double AverageValue { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public string OverallTrend { get; set; } = string.Empty;
    public double GrowthRate { get; set; }
}

public class TrendComparisonDto
{
    public string Period { get; set; } = string.Empty;
    public Dictionary<string, double> Metrics { get; set; } = new();
}

public class TrendForecastDto
{
    public string MetricName { get; set; } = string.Empty;
    public Dictionary<string, double> ForecastData { get; set; } = new();
    public double ConfidenceLevel { get; set; }
    public string ForecastMethod { get; set; } = "Linear";
}
