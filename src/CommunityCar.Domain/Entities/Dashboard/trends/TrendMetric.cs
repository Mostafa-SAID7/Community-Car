using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Dashboard.trends;

public class TrendMetric : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public double PreviousValue { get; set; }
    public double ChangePercentage => PreviousValue != 0 ? (Value - PreviousValue) / PreviousValue * 100 : 0;
    public string TimePeriod { get; set; } = "Daily"; // Daily, Weekly, Monthly
    public DateTimeOffset LastCalculated { get; set; } = DateTimeOffset.UtcNow;
}
