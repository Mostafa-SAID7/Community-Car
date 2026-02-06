using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Dashboard.health;

namespace CommunityCar.Domain.Entities.Dashboard.health;

public class HealthCheck : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public string? Description { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public DateTime CheckedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
