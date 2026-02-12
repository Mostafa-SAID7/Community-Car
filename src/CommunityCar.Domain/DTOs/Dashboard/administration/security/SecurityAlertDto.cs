using CommunityCar.Domain.Enums.Dashboard.security;

namespace CommunityCar.Domain.DTOs.Dashboard.Administration.Security;

public class SecurityAlertDto
{
    public Guid Id { get; set; }
    public string? Slug { get; set; }
    public string Title { get; set; } = string.Empty;
    public SecuritySeverity Severity { get; set; }
    public string SeverityText { get; set; } = string.Empty;
    public SecurityAlertType AlertType { get; set; }
    public string AlertTypeText { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Guid? AffectedUserId { get; set; }
    public string? AffectedUserName { get; set; }
    public bool IsResolved { get; set; }
    public Guid? ResolvedById { get; set; }
    public string? ResolvedByName { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTimeOffset DetectedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class SecurityAlertFilterDto
{
    public SecuritySeverity? Severity { get; set; }
    public SecurityAlertType? AlertType { get; set; }
    public bool? IsResolved { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? IpAddress { get; set; }
    public string? UserName { get; set; }
    public string? SortBy { get; set; } = "DetectedAt";
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SecurityAlertStatisticsDto
{
    public int TotalAlerts { get; set; }
    public int UnresolvedAlerts { get; set; }
    public int ResolvedAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public int HighAlerts { get; set; }
    public int MediumAlerts { get; set; }
    public int LowAlerts { get; set; }
    public Dictionary<string, int> AlertsByType { get; set; } = new();
    public Dictionary<string, int> AlertsBySeverity { get; set; } = new();
    public List<SecurityAlertDto> RecentAlerts { get; set; } = new();
    public List<SecurityAlertDto> CriticalUnresolved { get; set; } = new();
}

public class CreateSecurityAlertDto
{
    public string Title { get; set; } = string.Empty;
    public SecuritySeverity Severity { get; set; }
    public SecurityAlertType AlertType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Guid? AffectedUserId { get; set; }
    public string? AffectedUserName { get; set; }
}

public class UpdateSecurityAlertDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public SecuritySeverity Severity { get; set; }
    public SecurityAlertType AlertType { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class ResolveSecurityAlertDto
{
    public Guid Id { get; set; }
    public Guid ResolvedById { get; set; }
    public string ResolvedByName { get; set; } = string.Empty;
    public string? ResolutionNotes { get; set; }
}

public class SecuritySummaryDto
{
    public int TotalAlerts { get; set; }
    public int UnresolvedAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public int AlertsToday { get; set; }
    public int AlertsThisWeek { get; set; }
    public int AlertsThisMonth { get; set; }
    public double ResolutionRate { get; set; }
    public TimeSpan AverageResolutionTime { get; set; }
}
