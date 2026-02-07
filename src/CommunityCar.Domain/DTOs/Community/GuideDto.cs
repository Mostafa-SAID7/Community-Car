using CommunityCar.Domain.Enums.Community.guides;

namespace CommunityCar.Domain.DTOs.Community;

public class GuideDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public GuideStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public GuideDifficulty Difficulty { get; set; }
    public string DifficultyName { get; set; } = string.Empty;
    
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    
    public int EstimatedTimeMinutes { get; set; }
    public string? Tags { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int BookmarkCount { get; set; }
    public int CommentCount { get; set; }
    public int StepCount { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    
    public bool IsAuthor { get; set; }
    public bool IsLiked { get; set; }
    public bool IsBookmarked { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
