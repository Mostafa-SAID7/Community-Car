using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Enums.Dashboard.health;

namespace CommunityCar.Web.Areas.Dashboard.ViewModels;

public class HealthViewModel
{
    public HealthStatusDto HealthStatus { get; set; } = new();
    public SystemHealthDto SystemHealth { get; set; } = new();
    
    public string StatusBadgeClass => HealthStatus.Status switch
    {
        Domain.Enums.Dashboard.health.HealthStatus.Healthy => "badge bg-success",
        Domain.Enums.Dashboard.health.HealthStatus.Degraded => "badge bg-warning",
        Domain.Enums.Dashboard.health.HealthStatus.Unhealthy => "badge bg-danger",
        _ => "badge bg-secondary"
    };

    public string StatusText => HealthStatus.Status switch
    {
        Domain.Enums.Dashboard.health.HealthStatus.Healthy => "All Systems Operational",
        Domain.Enums.Dashboard.health.HealthStatus.Degraded => "Degraded Performance",
        Domain.Enums.Dashboard.health.HealthStatus.Unhealthy => "System Issues Detected",
        _ => "Unknown Status"
    };
    
    public string StatusIcon => HealthStatus.Status switch
    {
        Domain.Enums.Dashboard.health.HealthStatus.Healthy => "✓",
        Domain.Enums.Dashboard.health.HealthStatus.Degraded => "⚠",
        Domain.Enums.Dashboard.health.HealthStatus.Unhealthy => "✗",
        _ => "?"
    };
}

public class HealthCheckViewModel
{
    public string Name { get; set; } = string.Empty;
    public Domain.Enums.Dashboard.health.HealthStatus Status { get; set; }
    public string? Description { get; set; }
    public string ResponseTimeMs => $"{ResponseTime.TotalMilliseconds:F2} ms";
    public TimeSpan ResponseTime { get; set; }
    public DateTime CheckedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public string StatusIcon => Status switch
    {
        Domain.Enums.Dashboard.health.HealthStatus.Healthy => "✓",
        Domain.Enums.Dashboard.health.HealthStatus.Degraded => "⚠",
        Domain.Enums.Dashboard.health.HealthStatus.Unhealthy => "✗",
        _ => "?"
    };

    public string StatusClass => Status switch
    {
        Domain.Enums.Dashboard.health.HealthStatus.Healthy => "text-success",
        Domain.Enums.Dashboard.health.HealthStatus.Degraded => "text-warning",
        Domain.Enums.Dashboard.health.HealthStatus.Unhealthy => "text-danger",
        _ => "text-secondary"
    };
}
