namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels;

public class PaginationViewModel
{
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}
