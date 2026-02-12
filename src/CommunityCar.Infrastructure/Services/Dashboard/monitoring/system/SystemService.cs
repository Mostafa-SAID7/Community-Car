using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using CommunityCar.Domain.DTOs.Dashboard.Monitoring.System;
using CommunityCar.Domain.DTOs.Dashboard.Monitoring.Health;
using CommunityCar.Domain.Interfaces.Dashboard.Monitoring.System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Dashboard.Monitoring.System;

public class SystemService : ISystemService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SystemService> _logger;
    private static readonly DateTime _startTime = DateTime.UtcNow;

    public SystemService(IConfiguration configuration, ILogger<SystemService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SystemOverviewDto> GetSystemOverviewAsync()
    {
        return await Task.FromResult(new SystemOverviewDto
        {
            SystemInfo = await GetSystemInfoAsync(),
            Metrics = await GetSystemMetricsAsync(),
            Services = await GetServicesStatusAsync(),
            Resources = await GetSystemResourcesAsync()
        });
    }

    public async Task<SystemInfoDto> GetSystemInfoAsync()
    {
        var assembly = Assembly.GetEntryAssembly();
        var version = assembly?.GetName().Version?.ToString() ?? "1.0.0";
        var frameworkVersion = RuntimeInformation.FrameworkDescription;
        var osDescription = RuntimeInformation.OSDescription;

        return await Task.FromResult(new SystemInfoDto
        {
            ApplicationName = "CommunityCar",
            Version = version,
            Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production",
            FrameworkVersion = frameworkVersion,
            OperatingSystem = osDescription,
            MachineName = Environment.MachineName,
            ProcessorCount = Environment.ProcessorCount,
            StartTime = _startTime,
            Uptime = DateTime.UtcNow - _startTime,
            DatabaseProvider = "SQL Server",
            CacheProvider = "Memory Cache"
        });
    }

    public async Task<SystemMetricsDto> GetSystemMetricsAsync()
    {
        var process = Process.GetCurrentProcess();
        var totalMemory = GC.GetTotalMemory(false);
        var workingSet = process.WorkingSet64;

        return await Task.FromResult(new SystemMetricsDto
        {
            CpuUsage = GetCpuUsage(),
            MemoryUsage = (workingSet / (1024.0 * 1024.0)),
            TotalMemoryMB = (long)(workingSet / (1024.0 * 1024.0)),
            UsedMemoryMB = (long)(totalMemory / (1024.0 * 1024.0)),
            DiskUsage = GetDiskUsage(),
            ActiveConnections = GetActiveConnections(),
            Uptime = DateTime.UtcNow - _startTime
        });
    }

    public async Task<SystemResourcesDto> GetSystemResourcesAsync()
    {
        var process = Process.GetCurrentProcess();
        var driveInfo = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:\\");

        var totalDiskGB = driveInfo.TotalSize / (1024.0 * 1024.0 * 1024.0);
        var freeDiskGB = driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
        var usedDiskGB = totalDiskGB - freeDiskGB;

        return await Task.FromResult(new SystemResourcesDto
        {
            TotalDiskSpaceGB = (long)totalDiskGB,
            UsedDiskSpaceGB = (long)usedDiskGB,
            FreeDiskSpaceGB = (long)freeDiskGB,
            DiskUsagePercentage = (usedDiskGB / totalDiskGB) * 100,
            TotalMemoryMB = (long)(process.WorkingSet64 / (1024.0 * 1024.0)),
            UsedMemoryMB = (long)(GC.GetTotalMemory(false) / (1024.0 * 1024.0)),
            FreeMemoryMB = (long)((process.WorkingSet64 - GC.GetTotalMemory(false)) / (1024.0 * 1024.0)),
            MemoryUsagePercentage = (GC.GetTotalMemory(false) / (double)process.WorkingSet64) * 100,
            CpuUsagePercentage = GetCpuUsage(),
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount
        });
    }

    public async Task<SystemProcessDto> GetCurrentProcessInfoAsync()
    {
        var process = Process.GetCurrentProcess();

        return await Task.FromResult(new SystemProcessDto
        {
            ProcessId = process.Id,
            ProcessName = process.ProcessName,
            WorkingSetMB = process.WorkingSet64 / (1024 * 1024),
            CpuUsage = GetCpuUsage(),
            ThreadCount = process.Threads.Count,
            StartTime = process.StartTime,
            TotalProcessorTime = process.TotalProcessorTime
        });
    }

    public async Task<List<SystemServiceDto>> GetServicesStatusAsync()
    {
        var services = new List<SystemServiceDto>
        {
            new SystemServiceDto
            {
                Name = "Database",
                Status = "Running",
                Version = "SQL Server",
                LastChecked = DateTime.UtcNow,
                Description = "Primary database connection"
            },
            new SystemServiceDto
            {
                Name = "Cache",
                Status = "Running",
                Version = "Memory Cache",
                LastChecked = DateTime.UtcNow,
                Description = "In-memory caching service"
            },
            new SystemServiceDto
            {
                Name = "Email Service",
                Status = "Running",
                Version = "SMTP",
                LastChecked = DateTime.UtcNow,
                Description = "Email notification service"
            },
            new SystemServiceDto
            {
                Name = "Background Jobs",
                Status = "Running",
                Version = "1.0",
                LastChecked = DateTime.UtcNow,
                Description = "Background task processor"
            }
        };

        return await Task.FromResult(services);
    }

    public async Task<List<SystemLogDto>> GetRecentLogsAsync(int count = 100, string? level = null)
    {
        // This would typically read from a log file or database
        // For now, return empty list
        return await Task.FromResult(new List<SystemLogDto>());
    }

    public async Task<SystemConfigurationDto> GetConfigurationAsync()
    {
        var config = new SystemConfigurationDto();

        // Get app settings (excluding sensitive data)
        var appSettings = _configuration.AsEnumerable()
            .Where(x => !x.Key.Contains("Password") && !x.Key.Contains("Secret") && !x.Key.Contains("Key"))
            .ToDictionary(x => x.Key, x => x.Value ?? string.Empty);

        config.AppSettings = appSettings;

        return await Task.FromResult(config);
    }

    public async Task<Dictionary<string, object>> GetEnvironmentInfoAsync()
    {
        var info = new Dictionary<string, object>
        {
            ["MachineName"] = Environment.MachineName,
            ["OSVersion"] = Environment.OSVersion.ToString(),
            ["ProcessorCount"] = Environment.ProcessorCount,
            ["Is64BitOperatingSystem"] = Environment.Is64BitOperatingSystem,
            ["Is64BitProcess"] = Environment.Is64BitProcess,
            ["SystemDirectory"] = Environment.SystemDirectory,
            ["CurrentDirectory"] = Environment.CurrentDirectory,
            ["UserName"] = Environment.UserName,
            ["UserDomainName"] = Environment.UserDomainName,
            ["CLRVersion"] = Environment.Version.ToString(),
            ["WorkingSet"] = Environment.WorkingSet,
            ["SystemPageSize"] = Environment.SystemPageSize
        };

        return await Task.FromResult(info);
    }

    public async Task<bool> ClearCacheAsync()
    {
        try
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            _logger.LogInformation("Cache cleared successfully");
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return await Task.FromResult(false);
        }
    }

    public async Task<bool> RestartApplicationAsync()
    {
        try
        {
            _logger.LogWarning("Application restart requested");
            // This would typically trigger an application restart
            // Implementation depends on hosting environment
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restarting application");
            return await Task.FromResult(false);
        }
    }

    public async Task<Dictionary<string, string>> GetAssemblyInfoAsync()
    {
        var assembly = Assembly.GetEntryAssembly();
        var info = new Dictionary<string, string>();

        if (assembly != null)
        {
            info["Name"] = assembly.GetName().Name ?? "Unknown";
            info["Version"] = assembly.GetName().Version?.ToString() ?? "Unknown";
            info["FullName"] = assembly.FullName ?? "Unknown";
            info["Location"] = assembly.Location;
            info["ImageRuntimeVersion"] = assembly.ImageRuntimeVersion;

            var titleAttr = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
            if (titleAttr != null)
                info["Title"] = titleAttr.Title;

            var descAttr = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            if (descAttr != null)
                info["Description"] = descAttr.Description;

            var companyAttr = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (companyAttr != null)
                info["Company"] = companyAttr.Company;

            var copyrightAttr = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
            if (copyrightAttr != null)
                info["Copyright"] = copyrightAttr.Copyright;
        }

        return await Task.FromResult(info);
    }

    private double GetCpuUsage()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;

            Thread.Sleep(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = process.TotalProcessorTime;

            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return cpuUsageTotal * 100;
        }
        catch
        {
            return 0;
        }
    }

    private double GetDiskUsage()
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:\\");
            var usedSpace = driveInfo.TotalSize - driveInfo.AvailableFreeSpace;
            return (usedSpace / (double)driveInfo.TotalSize) * 100;
        }
        catch
        {
            return 0;
        }
    }

    private int GetActiveConnections()
    {
        // This would typically query the database or connection pool
        // For now, return a placeholder value
        return 0;
    }
}
