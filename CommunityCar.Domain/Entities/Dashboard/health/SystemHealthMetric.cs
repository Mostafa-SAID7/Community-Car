using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Dashboard.health;

public class SystemHealthMetric : BaseEntity
{
    public string ComponentName { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public double ResponseTimeMs { get; set; }
    public string? StatusDetails { get; set; }
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
}
