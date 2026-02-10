using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Entities.Community.groups;
using CommunityCar.Domain.Enums.Community.reviews;
using CommunityCar.Domain.Utilities;
using CommunityCar.Domain.ValueObjects;

namespace CommunityCar.Domain.Entities.Community.reviews;

public class Review : AggregateRoot
{
    public Guid EntityId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public ReviewType Type { get; private set; }
    public ReviewStatus Status { get; private set; }
    
    public Guid ReviewerId { get; private set; }
    public virtual ApplicationUser Reviewer { get; private set; } = null!;
    
    public Guid? GroupId { get; private set; }
    public virtual CommunityGroup? Group { get; private set; }
    
    private decimal _rating;
    public Rating Rating => ValueObjects.Rating.Create(_rating);
    public string Title { get; private set; } = string.Empty;
    public string Comment { get; private set; } = string.Empty;
    
    // Additional details
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public bool IsVerifiedPurchase { get; private set; }
    public bool IsRecommended { get; private set; }
    
    // Engagement metrics
    public int HelpfulCount { get; private set; }
    public int NotHelpfulCount { get; private set; }
    
    // Media
    public string? ImageUrls { get; private set; } // JSON array of image URLs
    
    // Moderation
    public string? ModerationNotes { get; private set; }
    public Guid? ModeratedBy { get; private set; }
    public DateTimeOffset? ModeratedAt { get; private set; }
    
    public virtual ICollection<ReviewReaction> Reactions { get; private set; } = new List<ReviewReaction>();
    public virtual ICollection<ReviewComment> Comments { get; private set; } = new List<ReviewComment>();

    private Review() { }

    public Review(
        Guid entityId,
        string entityType,
        ReviewType type,
        Guid reviewerId,
        decimal rating,
        string title,
        string comment,
        bool isVerifiedPurchase = false,
        bool isRecommended = true,
        Guid? groupId = null)
    {
        Guard.Against.Empty(entityId, nameof(entityId));
        Guard.Against.NullOrWhiteSpace(entityType, nameof(entityType));
        Guard.Against.Empty(reviewerId, nameof(reviewerId));
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(comment, nameof(comment));
        
        // Validate using Rating value object
        var ratingValue = ValueObjects.Rating.Create(rating);

        EntityId = entityId;
        EntityType = entityType;
        Type = type;
        ReviewerId = reviewerId;
        _rating = ratingValue.Value;
        Title = title;
        Comment = comment;
        IsVerifiedPurchase = isVerifiedPurchase;
        IsRecommended = isRecommended;
        GroupId = groupId;
        Status = ReviewStatus.Pending;
        Slug = SlugHelper.GenerateSlug(title);
    }

    public void Update(decimal rating, string title, string comment, bool isRecommended)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(comment, nameof(comment));
        
        // Validate using Rating value object
        var ratingValue = ValueObjects.Rating.Create(rating);

        _rating = ratingValue.Value;
        Title = title;
        Comment = comment;
        IsRecommended = isRecommended;
        Slug = SlugHelper.GenerateSlug(title);
    }

    public void SetProsAndCons(string? pros, string? cons)
    {
        Pros = pros;
        Cons = cons;
    }

    public void SetImages(string? imageUrls) => ImageUrls = imageUrls;

    public void Approve(Guid moderatorId)
    {
        Status = ReviewStatus.Approved;
        ModeratedBy = moderatorId;
        ModeratedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(Guid moderatorId, string reason)
    {
        Status = ReviewStatus.Rejected;
        ModeratedBy = moderatorId;
        ModeratedAt = DateTimeOffset.UtcNow;
        ModerationNotes = reason;
    }

    public void Flag(string reason)
    {
        Status = ReviewStatus.Flagged;
        ModerationNotes = reason;
    }

    public void MarkHelpful() => HelpfulCount++;
    public void MarkNotHelpful() => NotHelpfulCount++;
    
    public double HelpfulPercentage => 
        (HelpfulCount + NotHelpfulCount) > 0 
            ? (double)HelpfulCount / (HelpfulCount + NotHelpfulCount) * 100 
            : 0;
}
