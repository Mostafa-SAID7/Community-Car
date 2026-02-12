using CommunityCar.Domain.Enums.Community.Feed;

namespace CommunityCar.Mvc.ViewModels.Feed;

public class FeedFilterViewModel
{
    public FeedItemType? ContentType { get; set; }
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public DateFilterType DateFilter { get; set; } = DateFilterType.All;
    public FeedSortType SortBy { get; set; } = FeedSortType.Recent;
    public Guid? AuthorId { get; set; }
    public string? Tag { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? FollowingOnly { get; set; }

    public bool HasActiveFilters =>
        ContentType.HasValue ||
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        !string.IsNullOrWhiteSpace(Category) ||
        DateFilter != DateFilterType.All ||
        AuthorId.HasValue ||
        !string.IsNullOrWhiteSpace(Tag) ||
        IsFeatured.HasValue ||
        FollowingOnly.HasValue;

    public int ActiveFilterCount
    {
        get
        {
            int count = 0;
            if (ContentType.HasValue) count++;
            if (!string.IsNullOrWhiteSpace(SearchTerm)) count++;
            if (!string.IsNullOrWhiteSpace(Category)) count++;
            if (DateFilter != DateFilterType.All) count++;
            if (AuthorId.HasValue) count++;
            if (!string.IsNullOrWhiteSpace(Tag)) count++;
            if (IsFeatured.HasValue) count++;
            if (FollowingOnly.HasValue) count++;
            return count;
        }
    }

    public void Reset()
    {
        ContentType = null;
        SearchTerm = null;
        Category = null;
        DateFilter = DateFilterType.All;
        SortBy = FeedSortType.Recent;
        AuthorId = null;
        Tag = null;
        IsFeatured = null;
        FollowingOnly = null;
    }
}
