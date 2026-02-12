using CommunityCar.Domain.DTOs.Dashboard.Monitoring.System;
using CommunityCar.Domain.DTOs.Dashboard.Monitoring.Health;

namespace CommunityCar.Domain.Interfaces.Dashboard.Monitoring.System;

public interface ISystemService
{
    Task<SystemOverviewDto> GetSystemOverviewAsync();
    Task<SystemInfoDto> GetSystemInfoAsync();
    Task<SystemMetricsDto> GetSystemMetricsAsync();
    Task<SystemResourcesDto> GetSystemResourcesAsync();
    Task<SystemProcessDto> GetCurrentProcessInfoAsync();
    Task<List<SystemServiceDto>> GetServicesStatusAsync();
    Task<List<SystemLogDto>> GetRecentLogsAsync(int count = 100, string? level = null);
    Task<SystemConfigurationDto> GetConfigurationAsync();
    Task<Dictionary<string, object>> GetEnvironmentInfoAsync();
    Task<bool> ClearCacheAsync();
    Task<bool> RestartApplicationAsync();
    Task<Dictionary<string, string>> GetAssemblyInfoAsync();
}
