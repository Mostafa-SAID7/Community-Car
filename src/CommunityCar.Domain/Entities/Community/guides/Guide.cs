using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.guides;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.guides;

public class Guide : AggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public GuideStatus Status { get; set; }
    public GuideDifficulty Difficulty { get; set; }
    
    public Guid AuthorId { get; set; }
    public virtual ApplicationUser Author { get; set; } = null!;
    
    // Metadata
    public int EstimatedTimeMinutes { get; set; }
    public string? Tags { get; set; } // JSON array
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    
    // Engagement
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int BookmarkCount { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    
    // SEO
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    
    // Navigation collections
    public virtual ICollection<GuideStep> Steps { get; set; } = new List<GuideStep>();
    public virtual ICollection<GuideComment> Comments { get; set; } = new List<GuideComment>();

    private Guide() { }

    public Guide(
        string title,
        string content,
        string summary,
        string category,
        Guid authorId,
        GuideDifficulty difficulty,
        int estimatedTimeMinutes)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Guard.Against.NullOrWhiteSpace(summary, nameof(summary));
        Guard.Against.NullOrWhiteSpace(category, nameof(category));
        Guard.Against.Empty(authorId, nameof(authorId));

        Title = title;
        Content = content;
        Summary = summary;
        Category = category;
        AuthorId = authorId;
        Difficulty = difficulty;
        EstimatedTimeMinutes = estimatedTimeMinutes;
        Status = GuideStatus.Draft;
        Slug = SlugHelper.GenerateSlug(title);
    }

    public void Update(string title, string content, string summary, string category, GuideDifficulty difficulty, int estimatedTimeMinutes)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Guard.Against.NullOrWhiteSpace(summary, nameof(summary));
        Guard.Against.NullOrWhiteSpace(category, nameof(category));

        Title = title;
        Content = content;
        Summary = summary;
        Category = category;
        Difficulty = difficulty;
        EstimatedTimeMinutes = estimatedTimeMinutes;
        Slug = SlugHelper.GenerateSlug(title);
    }

    public void Publish() => Status = GuideStatus.Published;
    public void Archive() => Status = GuideStatus.Archived;
    public void SubmitForReview() => Status = GuideStatus.UnderReview;
    
    public void IncrementViews() => ViewCount++;
    public void IncrementLikes() => LikeCount++;
    public void DecrementLikes() => LikeCount = Math.Max(0, LikeCount - 1);
    public void IncrementBookmarks() => BookmarkCount++;
    public void DecrementBookmarks() => BookmarkCount = Math.Max(0, BookmarkCount - 1);
    
    public void UpdateRating(double newRating, int newCount)
    {
        AverageRating = newRating;
        RatingCount = newCount;
    }
}
