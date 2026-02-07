using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.reviews;

public class ReviewReaction : BaseEntity
{
    public Guid ReviewId { get; private set; }
    public virtual Review Review { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public bool IsHelpful { get; private set; }

    private ReviewReaction() { }

    public ReviewReaction(Guid reviewId, Guid userId, bool isHelpful)
    {
        Guard.Against.Empty(reviewId, nameof(reviewId));
        Guard.Against.Empty(userId, nameof(userId));

        ReviewId = reviewId;
        UserId = userId;
        IsHelpful = isHelpful;
    }

    public void UpdateReaction(bool isHelpful) => IsHelpful = isHelpful;
}
