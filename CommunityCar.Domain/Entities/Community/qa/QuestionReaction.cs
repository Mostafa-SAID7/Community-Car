using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.qa;

namespace CommunityCar.Domain.Entities.Community.qa;

public class QuestionReaction : BaseEntity
{
    public Guid QuestionId { get; private set; }
    public virtual Question Question { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public ReactionType ReactionType { get; private set; }

    private QuestionReaction() { }

    public QuestionReaction(Guid questionId, Guid userId, ReactionType reactionType)
    {
        Guard.Against.Empty(questionId, nameof(questionId));
        Guard.Against.Empty(userId, nameof(userId));

        QuestionId = questionId;
        UserId = userId;
        ReactionType = reactionType;
    }

    public void ChangeReaction(ReactionType reactionType) => ReactionType = reactionType;
}
