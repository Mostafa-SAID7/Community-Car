using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.qa;

public class Answer : BaseEntity
{
    public Guid QuestionId { get; private set; }
    public virtual Question Question { get; private set; } = null!;
    
    public string Content { get; private set; } = string.Empty;
    public Guid AuthorId { get; private set; }
    public virtual ApplicationUser Author { get; private set; } = null!;
    
    public int VoteCount { get; private set; }
    public bool IsAccepted { get; private set; }
    
    public virtual ICollection<AnswerVote> Votes { get; private set; } = new List<AnswerVote>();
    public virtual ICollection<AnswerReaction> Reactions { get; private set; } = new List<AnswerReaction>();

    private Answer() { }

    public Answer(Guid questionId, string content, Guid authorId)
    {
        Guard.Against.Empty(questionId, nameof(questionId));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Guard.Against.Empty(authorId, nameof(authorId));

        QuestionId = questionId;
        Content = content;
        AuthorId = authorId;
        VoteCount = 0;
        IsAccepted = false;
    }

    public void Update(string content)
    {
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Content = content;
    }

    public void MarkAsAccepted() => IsAccepted = true;
    public void UnmarkAsAccepted() => IsAccepted = false;
    public void UpdateVoteCount(int delta) => VoteCount += delta;
}
