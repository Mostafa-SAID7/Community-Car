using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Dashboard.health;

public class SystemMetric : BaseEntity
{
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public string? Category { get; set; }
}
