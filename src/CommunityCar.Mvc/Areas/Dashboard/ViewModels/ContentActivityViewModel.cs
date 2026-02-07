using CommunityCar.Domain.DTOs.Dashboard;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels;

public class ContentActivityViewModel
{
    public List<ContentActivityDto> Activities { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public string? UserId { get; set; }
    public string? ContentType { get; set; }
    
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

public class ContentActivityStatisticsViewModel
{
    public ContentActivityStatistics Statistics { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
