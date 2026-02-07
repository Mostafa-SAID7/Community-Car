using System.ComponentModel.DataAnnotations;
using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Enums.Dashboard.security;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels;

public class SecurityIndexViewModel
{
    public List<SecurityAlertDto> Alerts { get; set; } = new();
    public SecurityFilterViewModel Filter { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
    public SecuritySummaryDto Summary { get; set; } = new();
}

public class SecurityFilterViewModel
{
    public SecuritySeverity? Severity { get; set; }
    public SecurityAlertType? AlertType { get; set; }
    public bool? IsResolved { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

public class SecurityDetailsViewModel
{
    public SecurityAlertDto Alert { get; set; } = new();
    public List<SecurityAlertDto> RelatedAlerts { get; set; } = new();
}

public class CreateSecurityAlertViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Severity is required")]
    public SecuritySeverity Severity { get; set; }

    [Required(ErrorMessage = "Alert type is required")]
    public SecurityAlertType AlertType { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Source cannot exceed 200 characters")]
    public string? Source { get; set; }

    [StringLength(50, ErrorMessage = "IP Address cannot exceed 50 characters")]
    public string? IpAddress { get; set; }

    [StringLength(500, ErrorMessage = "User Agent cannot exceed 500 characters")]
    public string? UserAgent { get; set; }

    public Guid? AffectedUserId { get; set; }

    [StringLength(256, ErrorMessage = "Affected User Name cannot exceed 256 characters")]
    public string? AffectedUserName { get; set; }
}

public class EditSecurityAlertViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Severity is required")]
    public SecuritySeverity Severity { get; set; }

    [Required(ErrorMessage = "Alert type is required")]
    public SecurityAlertType AlertType { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;
}

public class ResolveSecurityAlertViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public SecuritySeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset DetectedAt { get; set; }

    [StringLength(1000, ErrorMessage = "Resolution notes cannot exceed 1000 characters")]
    public string? ResolutionNotes { get; set; }
}

public class SecurityStatisticsViewModel
{
    public SecurityAlertStatisticsDto Statistics { get; set; } = new();
    public Dictionary<string, int> Trends { get; set; } = new();
}
