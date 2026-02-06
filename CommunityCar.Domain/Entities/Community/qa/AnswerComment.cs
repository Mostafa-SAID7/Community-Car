using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;

namespace CommunityCar.Domain.Entities.Community.qa;

public class AnswerComment : BaseEntity
{
    public Guid AnswerId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public Guid? ParentCommentId { get; private set; }
    public int VoteCount { get; private set; }
    
    // Navigation properties
    public virtual Answer Answer { get; private set; } = null!;
    public virtual ApplicationUser Author { get; private set; } = null!;
    public virtual AnswerComment? ParentComment { get; private set; }
    public virtual ICollection<AnswerComment> Replies { get; private set; } = new List<AnswerComment>();

    private AnswerComment() { } // EF Core

    public AnswerComment(Guid answerId, Guid authorId, string content, Guid? parentCommentId = null)
    {
        AnswerId = answerId;
        AuthorId = authorId;
        Content = content;
        ParentCommentId = parentCommentId;
        VoteCount = 0;
    }

    public void Update(string content)
    {
        Content = content;
        ModifiedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateVoteCount(int delta)
    {
        VoteCount += delta;
    }
}
