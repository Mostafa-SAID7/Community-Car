using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Community.Common;

namespace CommunityCar.Domain.Entities.Community.qa;

public class QuestionTag : BaseEntity
{
    public Guid QuestionId { get; private set; }
    public virtual Question Question { get; private set; } = null!;
    
    public Guid TagId { get; private set; }
    public virtual Tag Tag { get; private set; } = null!;

    private QuestionTag() { }

    public QuestionTag(Guid questionId, Guid tagId)
    {
        Guard.Against.Empty(questionId, nameof(questionId));
        Guard.Against.Empty(tagId, nameof(tagId));

        QuestionId = questionId;
        TagId = tagId;
    }
}

