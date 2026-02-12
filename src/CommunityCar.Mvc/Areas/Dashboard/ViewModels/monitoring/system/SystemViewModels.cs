using CommunityCar.Domain.DTOs.Dashboard.Monitoring.System;
using CommunityCar.Domain.DTOs.Dashboard.Monitoring.Health;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels.Monitoring.System;

public class SystemIndexViewModel
{
    public SystemOverviewDto Overview { get; set; } = new();
    public List<SystemLogDto> RecentLogs { get; set; } = new();
    public Dictionary<string, string> AssemblyInfo { get; set; } = new();
}

public class SystemInfoViewModel
{
    public SystemInfoDto SystemInfo { get; set; } = new();
    public Dictionary<string, object> EnvironmentInfo { get; set; } = new();
    public Dictionary<string, string> AssemblyInfo { get; set; } = new();
}

public class SystemResourcesViewModel
{
    public SystemResourcesDto Resources { get; set; } = new();
    public SystemProcessDto ProcessInfo { get; set; } = new();
    public List<SystemMetric> MetricHistory { get; set; } = new();
}

public class SystemServicesViewModel
{
    public List<SystemServiceDto> Services { get; set; } = new();
    public DateTime LastChecked { get; set; }
}

public class SystemLogsViewModel
{
    public List<SystemLogDto> Logs { get; set; } = new();
    public string? FilterLevel { get; set; }
    public int TotalCount { get; set; }
}

public class SystemConfigurationViewModel
{
    public SystemConfigurationDto Configuration { get; set; } = new();
    public bool ShowSensitiveData { get; set; }
}

public class SystemMetric
{
    public DateTime Timestamp { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
}
