using CommunityCar.Domain.Base;
using CommunityCar.Domain.Utilities;
using CommunityCar.Domain.Entities.Identity.Users;

namespace CommunityCar.Domain.Entities.Community.qa;

public class QuestionVote : BaseEntity
{
    public Guid QuestionId { get; private set; }
    public virtual Question Question { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public bool IsUpvote { get; private set; }

    private QuestionVote() { }

    public QuestionVote(Guid questionId, Guid userId, bool isUpvote)
    {
        Guard.Against.Empty(questionId, nameof(questionId));
        Guard.Against.Empty(userId, nameof(userId));

        QuestionId = questionId;
        UserId = userId;
        IsUpvote = isUpvote;
    }

    public void Toggle() => IsUpvote = !IsUpvote;
}
