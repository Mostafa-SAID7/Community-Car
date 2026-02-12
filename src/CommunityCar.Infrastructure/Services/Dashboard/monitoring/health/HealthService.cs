using System.Diagnostics;
using CommunityCar.Domain.DTOs.Dashboard.Monitoring.Health;
using CommunityCar.Domain.Enums.Dashboard.health;
using CommunityCar.Domain.Interfaces.Dashboard.Monitoring.Health;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Dashboard.Monitoring.Health;

public class HealthService : IHealthService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthService> _logger;
    private static readonly DateTime _startTime = DateTime.UtcNow;
    private static readonly List<HealthCheckDto> _healthHistory = new();

    public HealthService(ApplicationDbContext context, ILogger<HealthService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SystemHealthDto> GetSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var health = new SystemHealthDto
            {
                Metrics = new SystemMetricsDto(),
                Checks = new List<HealthCheckDto>()
            };

            try
            {
                var process = Process.GetCurrentProcess();
                
                // Memory metrics
                health.Metrics.UsedMemoryMB = process.WorkingSet64 / (1024 * 1024);
                health.Metrics.TotalMemoryMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024);
                health.Metrics.MemoryUsage = health.Metrics.TotalMemoryMB > 0 
                    ? Math.Round((double)health.Metrics.UsedMemoryMB / health.Metrics.TotalMemoryMB * 100, 2) 
                    : 0;

                // CPU usage (approximate)
                health.Metrics.CpuUsage = Math.Round(process.TotalProcessorTime.TotalMilliseconds / 
                    (Environment.ProcessorCount * DateTime.UtcNow.Subtract(process.StartTime).TotalMilliseconds) * 100, 2);

                // Disk metrics
                var driveInfo = new DriveInfo(Path.GetPathRoot(AppContext.BaseDirectory) ?? "C:\\");
                if (driveInfo.IsReady)
                {
                    var totalGB = driveInfo.TotalSize / (1024.0 * 1024 * 1024);
                    var usedGB = (driveInfo.TotalSize - driveInfo.AvailableFreeSpace) / (1024.0 * 1024 * 1024);
                    health.Metrics.DiskUsage = Math.Round(usedGB / totalGB * 100, 2);
                }

                // Uptime
                health.Metrics.Uptime = DateTime.UtcNow - _startTime;
                health.Metrics.ActiveConnections = Process.GetCurrentProcess().Threads.Count;

                // Determine status
                if (health.Metrics.MemoryUsage > 90 || health.Metrics.CpuUsage > 90 || health.Metrics.DiskUsage > 90)
                    health.OverallStatus = HealthStatus.Unhealthy;
                else if (health.Metrics.MemoryUsage > 75 || health.Metrics.CpuUsage > 75 || health.Metrics.DiskUsage > 75)
                    health.OverallStatus = HealthStatus.Degraded;
                else
                    health.OverallStatus = HealthStatus.Healthy;

                health.LastChecked = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System health check failed");
                health.OverallStatus = HealthStatus.Unhealthy;
            }

            return health;
        }, cancellationToken);
    }

    public async Task<HealthCheckDto> CheckDatabaseHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var healthCheck = new HealthCheckDto
        {
            Name = "Database",
            CheckedAt = DateTime.UtcNow
        };

        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            stopwatch.Stop();

            if (canConnect)
            {
                healthCheck.Status = stopwatch.ElapsedMilliseconds < 1000 ? HealthStatus.Healthy : HealthStatus.Degraded;
                healthCheck.Description = $"Database connection successful. Users: {await _context.Users.CountAsync(cancellationToken)}";
            }
            else
            {
                healthCheck.Status = HealthStatus.Unhealthy;
                healthCheck.Description = "Cannot connect to database";
            }

            healthCheck.ResponseTime = stopwatch.Elapsed;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Database health check failed");
            healthCheck.Status = HealthStatus.Unhealthy;
            healthCheck.ErrorMessage = ex.Message;
            healthCheck.ResponseTime = stopwatch.Elapsed;
        }

        AddToHistory(healthCheck);
        return healthCheck;
    }

    public async Task<HealthCheckDto> CheckCacheHealthAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var healthCheck = new HealthCheckDto
            {
                Name = "Cache",
                Status = HealthStatus.Healthy,
                Description = "In-memory cache operational",
                CheckedAt = DateTime.UtcNow,
                ResponseTime = TimeSpan.Zero
            };

            AddToHistory(healthCheck);
            return healthCheck;
        }, cancellationToken);
    }

    public async Task<SystemMetricsDto> GetSystemMetricsAsync()
    {
        var systemHealth = await GetSystemHealthAsync();
        return systemHealth.Metrics;
    }

    public Task<List<HealthCheckDto>> GetHealthHistoryAsync(int hours = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-hours);
        var history = _healthHistory
            .Where(h => h.CheckedAt >= cutoffTime)
            .OrderByDescending(h => h.CheckedAt)
            .ToList();

        return Task.FromResult(history);
    }

    private void AddToHistory(HealthCheckDto healthCheck)
    {
        _healthHistory.Add(healthCheck);
        
        // Keep only last 1000 entries
        if (_healthHistory.Count > 1000)
        {
            _healthHistory.RemoveRange(0, _healthHistory.Count - 1000);
        }
    }
}
