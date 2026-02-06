using CommunityCar.Domain.Base;
using CommunityCar.Domain.Utilities;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.qa;

namespace CommunityCar.Domain.Entities.Community.qa;

public class AnswerReaction : BaseEntity
{
    public Guid AnswerId { get; private set; }
    public virtual Answer Answer { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public ReactionType ReactionType { get; private set; }

    private AnswerReaction() { }

    public AnswerReaction(Guid answerId, Guid userId, ReactionType reactionType)
    {
        Guard.Against.Empty(answerId, nameof(answerId));
        Guard.Against.Empty(userId, nameof(userId));

        AnswerId = answerId;
        UserId = userId;
        ReactionType = reactionType;
    }

    public void ChangeReaction(ReactionType reactionType) => ReactionType = reactionType;
}
