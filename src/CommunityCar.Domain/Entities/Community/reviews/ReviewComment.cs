using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.reviews;

public class ReviewComment : BaseEntity
{
    public Guid ReviewId { get; private set; }
    public virtual Review Review { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public string Content { get; private set; } = string.Empty;

    private ReviewComment() { }

    public ReviewComment(Guid reviewId, Guid userId, string content)
    {
        Guard.Against.Empty(reviewId, nameof(reviewId));
        Guard.Against.Empty(userId, nameof(userId));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));

        ReviewId = reviewId;
        UserId = userId;
        Content = content;
    }

    public void Update(string content)
    {
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Content = content;
    }
}
