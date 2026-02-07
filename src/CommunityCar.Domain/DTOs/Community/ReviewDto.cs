using CommunityCar.Domain.Enums.Community.reviews;

namespace CommunityCar.Domain.DTOs.Community;

public class ReviewDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public ReviewType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public ReviewStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? ReviewerAvatar { get; set; }
    
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public bool IsRecommended { get; set; }
    
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public double HelpfulPercentage { get; set; }
    
    public string? ImageUrls { get; set; }
    
    public bool IsReviewer { get; set; }
    public bool? CurrentUserReaction { get; set; } // true = helpful, false = not helpful, null = no reaction
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
