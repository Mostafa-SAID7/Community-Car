using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Enums.Dashboard.health;

namespace CommunityCar.Web.Areas.Dashboard.ViewModels;

public class HealthViewModel
{
    public SystemHealthDto SystemHealth { get; set; } = new();
    public string StatusBadgeClass => SystemHealth.OverallStatus switch
    {
        HealthStatus.Healthy => "badge bg-success",
        HealthStatus.Degraded => "badge bg-warning",
        HealthStatus.Unhealthy => "badge bg-danger",
        _ => "badge bg-secondary"
    };

    public string StatusText => SystemHealth.OverallStatus switch
    {
        HealthStatus.Healthy => "All Systems Operational",
        HealthStatus.Degraded => "Degraded Performance",
        HealthStatus.Unhealthy => "System Issues Detected",
        _ => "Unknown Status"
    };
}

public class HealthCheckViewModel
{
    public string Name { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public string? Description { get; set; }
    public string ResponseTimeMs => $"{ResponseTime.TotalMilliseconds:F2} ms";
    public TimeSpan ResponseTime { get; set; }
    public DateTime CheckedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public string StatusIcon => Status switch
    {
        HealthStatus.Healthy => "✓",
        HealthStatus.Degraded => "⚠",
        HealthStatus.Unhealthy => "✗",
        _ => "?"
    };

    public string StatusClass => Status switch
    {
        HealthStatus.Healthy => "text-success",
        HealthStatus.Degraded => "text-warning",
        HealthStatus.Unhealthy => "text-danger",
        _ => "text-secondary"
    };
}
