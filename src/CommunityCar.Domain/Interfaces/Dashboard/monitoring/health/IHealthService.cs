using CommunityCar.Domain.DTOs.Dashboard.Monitoring.Health;

namespace CommunityCar.Domain.Interfaces.Dashboard.Monitoring.Health;

public interface IHealthService
{
    Task<SystemHealthDto> GetSystemHealthAsync(CancellationToken cancellationToken = default);
    Task<HealthCheckDto> CheckDatabaseHealthAsync(CancellationToken cancellationToken = default);
    Task<HealthCheckDto> CheckCacheHealthAsync(CancellationToken cancellationToken = default);
    Task<SystemMetricsDto> GetSystemMetricsAsync();
    Task<List<HealthCheckDto>> GetHealthHistoryAsync(int hours = 24);
}
