using CommunityCar.Domain.Base;
using CommunityCar.Domain.Utilities;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.qa;

namespace CommunityCar.Domain.Entities.Community.guides;

public class GuideReaction : BaseEntity
{
    public Guid GuideId { get; private set; }
    public virtual Guide Guide { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public ReactionType ReactionType { get; private set; }

    private GuideReaction() { }

    public GuideReaction(Guid guideId, Guid userId, ReactionType reactionType)
    {
        Guard.Against.Empty(guideId, nameof(guideId));
        Guard.Against.Empty(userId, nameof(userId));

        GuideId = guideId;
        UserId = userId;
        ReactionType = reactionType;
    }

    public void ChangeReaction(ReactionType reactionType) => ReactionType = reactionType;
}
