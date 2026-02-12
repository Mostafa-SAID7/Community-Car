using CommunityCar.Domain.Enums.Community.Feed;

namespace CommunityCar.Mvc.ViewModels.Feed;

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
    
    public bool? IsResolved { get; set; }
    public int? AnswerCount { get; set; }
    public DateTimeOffset? EventStartTime { get; set; }
    public string? EventLocation { get; set; }
    public int? Rating { get; set; }
    public bool? IsRecommended { get; set; }
    public int? MemberCount { get; set; }
    public bool? IsPrivate { get; set; }
    public bool? IsUserMember { get; set; }

    public string GetTruncatedContent(int maxLength = 200)
    {
        if (string.IsNullOrEmpty(Content) || Content.Length <= maxLength)
            return Content;

        return Content.Substring(0, maxLength) + "...";
    }

    public List<string> GetTagsList()
    {
        if (string.IsNullOrWhiteSpace(Tags))
            return new List<string>();

        return Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(t => t.Trim())
                   .ToList();
    }

    public bool HasImage => !string.IsNullOrWhiteSpace(ImageUrl);
    public bool IsHighEngagement => InteractionCount > 100;

    public string GetEngagementClass()
    {
        if (InteractionCount > 500) return "engagement-high";
        if (InteractionCount > 100) return "engagement-medium";
        return "engagement-low";
    }
}
