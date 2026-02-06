using CommunityCar.Domain.DTOs.Dashboard;

namespace CommunityCar.Web.Areas.Dashboard.ViewModels;

public class AuditLogIndexViewModel
{
    public List<AuditLogDto> AuditLogs { get; set; } = new();
    public AuditLogFilterViewModel Filter { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
    public AuditLogStatisticsViewModel Statistics { get; set; } = new();
}

public class AuditLogFilterViewModel
{
    public string? UserName { get; set; }
    public string? EntityName { get; set; }
    public string? Action { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string> AvailableEntities { get; set; } = new();
    public List<string> AvailableActions { get; set; } = new();
}

public class PaginationViewModel
{
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

public class AuditLogStatisticsViewModel
{
    public int TotalLogs { get; set; }
    public Dictionary<string, int> ActionCounts { get; set; } = new();
}

public class AuditLogDetailsViewModel
{
    public AuditLogDto AuditLog { get; set; } = new();
    public Dictionary<string, string>? OldValuesDict { get; set; }
    public Dictionary<string, string>? NewValuesDict { get; set; }
    public List<string>? AffectedColumnsList { get; set; }
}
