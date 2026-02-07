namespace CommunityCar.Domain.DTOs.Dashboard;

public class SystemInfoDto
{
    public string ApplicationName { get; set; } = "CommunityCar";
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string FrameworkVersion { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public int ProcessorCount { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan Uptime { get; set; }
    public string DatabaseProvider { get; set; } = string.Empty;
    public string CacheProvider { get; set; } = string.Empty;
}

public class SystemOverviewDto
{
    public SystemInfoDto SystemInfo { get; set; } = new();
    public SystemMetricsDto Metrics { get; set; } = new();
    public List<SystemServiceDto> Services { get; set; } = new();
    public SystemResourcesDto Resources { get; set; } = new();
}

public class SystemServiceDto
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime? LastChecked { get; set; }
    public string? Description { get; set; }
}

public class SystemResourcesDto
{
    public long TotalDiskSpaceGB { get; set; }
    public long UsedDiskSpaceGB { get; set; }
    public long FreeDiskSpaceGB { get; set; }
    public double DiskUsagePercentage { get; set; }
    public long TotalMemoryMB { get; set; }
    public long UsedMemoryMB { get; set; }
    public long FreeMemoryMB { get; set; }
    public double MemoryUsagePercentage { get; set; }
    public double CpuUsagePercentage { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
}

public class SystemProcessDto
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public long WorkingSetMB { get; set; }
    public double CpuUsage { get; set; }
    public int ThreadCount { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan TotalProcessorTime { get; set; }
}

public class SystemLogDto
{
    public Guid Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Source { get; set; }
}

public class SystemConfigurationDto
{
    public Dictionary<string, string> AppSettings { get; set; } = new();
    public Dictionary<string, string> ConnectionStrings { get; set; } = new();
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
}
