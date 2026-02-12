using CommunityCar.Domain.Enums.Dashboard.health;

namespace CommunityCar.Domain.DTOs.Dashboard.Monitoring.Health;

public class HealthStatusDto
{
    public HealthStatus Status { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public DatabaseHealthDto Database { get; set; } = new();
    public SystemHealthDto System { get; set; } = new();
    public IEnumerable<ServiceHealthDto> Services { get; set; } = new List<ServiceHealthDto>();
}

public class DatabaseHealthDto
{
    public bool IsConnected { get; set; }
    public long ResponseTimeMs { get; set; }
    public int TotalUsers { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalAnswers { get; set; }
    public HealthStatus Status { get; set; }
}

public class ServiceHealthDto
{
    public string Name { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public long ResponseTimeMs { get; set; }
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
}
