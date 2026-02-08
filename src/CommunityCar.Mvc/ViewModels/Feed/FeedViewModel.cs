namespace CommunityCar.Mvc.ViewModels.Feed;

public class FeedViewModel
{
    public List<FeedItemViewModel> Items { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    
    public FeedFilterViewModel Filters { get; set; } = new();
}

public class FeedFilterViewModel
{
    public FeedItemType? ContentType { get; set; }
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public DateFilterType DateFilter { get; set; } = DateFilterType.All;
    public FeedSortType SortBy { get; set; } = FeedSortType.Recent;
}

public class FeedItemViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public FeedItemType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string TypeIcon { get; set; } = string.Empty;
    public string TypeColor { get; set; } = string.Empty;
    
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int InteractionCount { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
    
    public string ActionUrl { get; set; } = string.Empty;
    public string ActionText { get; set; } = "View";
    
    // Type-specific properties
    public bool? IsResolved { get; set; } // Questions
    public int? AnswerCount { get; set; } // Questions
    public DateTimeOffset? EventStartTime { get; set; } // Events
    public string? EventLocation { get; set; } // Events
    public int? Rating { get; set; } // Reviews
    public bool? IsRecommended { get; set; } // Reviews
    public int? MemberCount { get; set; } // Groups
    public bool? IsPrivate { get; set; } // Groups
    public bool? IsUserMember { get; set; } // Groups
}

public enum FeedItemType
{
    Post,
    Question,
    Event,
    News,
    Guide,
    Review,
    Group
}

public enum DateFilterType
{
    All,
    Today,
    ThisWeek,
    ThisMonth
}

public enum FeedSortType
{
    Recent,
    Popular,
    Trending
}
