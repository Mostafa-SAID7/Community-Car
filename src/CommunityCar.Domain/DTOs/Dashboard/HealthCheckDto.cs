using CommunityCar.Domain.Enums.Dashboard.health;

namespace CommunityCar.Domain.DTOs.Dashboard;

public class HealthCheckDto
{
    public string Name { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public string? Description { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public DateTime CheckedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SystemHealthDto
{
    public HealthStatus OverallStatus { get; set; }
    public List<HealthCheckDto> Checks { get; set; } = new();
    public SystemMetricsDto Metrics { get; set; } = new();
    public DateTime LastChecked { get; set; }
}

public class SystemMetricsDto
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public long TotalMemoryMB { get; set; }
    public long UsedMemoryMB { get; set; }
    public double DiskUsage { get; set; }
    public int ActiveConnections { get; set; }
    public TimeSpan Uptime { get; set; }
}
