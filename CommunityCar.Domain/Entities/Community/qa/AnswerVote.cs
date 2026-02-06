using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;

namespace CommunityCar.Domain.Entities.Community.qa;

public class AnswerVote : BaseEntity
{
    public Guid AnswerId { get; private set; }
    public virtual Answer Answer { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public bool IsUpvote { get; private set; }

    private AnswerVote() { }

    public AnswerVote(Guid answerId, Guid userId, bool isUpvote)
    {
        Guard.Against.Empty(answerId, nameof(answerId));
        Guard.Against.Empty(userId, nameof(userId));

        AnswerId = answerId;
        UserId = userId;
        IsUpvote = isUpvote;
    }

    public void Toggle() => IsUpvote = !IsUpvote;
}
