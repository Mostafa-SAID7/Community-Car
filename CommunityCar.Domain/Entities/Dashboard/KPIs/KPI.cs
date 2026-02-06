using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Dashboard.KPIs;

public class KPI : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
