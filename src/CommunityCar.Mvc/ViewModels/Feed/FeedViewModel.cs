namespace CommunityCar.Mvc.ViewModels.Feed;

public class FeedViewModel
{
    public List<FeedItemViewModel> Items { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; } = 20;
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public FeedFilterViewModel Filters { get; set; } = new();
    public bool IsEmpty => !Items.Any();
    public int StartItemNumber => (CurrentPage - 1) * PageSize + 1;
    public int EndItemNumber => Math.Min(CurrentPage * PageSize, TotalCount);

    public IEnumerable<int> GetPageNumbers(int maxPages = 10)
    {
        int startPage = Math.Max(1, CurrentPage - maxPages / 2);
        int endPage = Math.Min(TotalPages, startPage + maxPages - 1);

        if (endPage - startPage < maxPages - 1)
        {
            startPage = Math.Max(1, endPage - maxPages + 1);
        }

        return Enumerable.Range(startPage, endPage - startPage + 1);
    }

    public string GetSummaryMessage()
    {
        if (IsEmpty)
        {
            return Filters.HasActiveFilters
                ? "No items found matching your filters"
                : "No items to display";
        }

        return $"Showing {StartItemNumber}-{EndItemNumber} of {TotalCount} items";
    }
}
