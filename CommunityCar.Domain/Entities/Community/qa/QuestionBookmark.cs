using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;

namespace CommunityCar.Domain.Entities.Community.qa;

public class QuestionBookmark : BaseEntity
{
    public Guid QuestionId { get; private set; }
    public virtual Question Question { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public string? Notes { get; private set; }

    private QuestionBookmark() { }

    public QuestionBookmark(Guid questionId, Guid userId, string? notes = null)
    {
        Guard.Against.Empty(questionId, nameof(questionId));
        Guard.Against.Empty(userId, nameof(userId));

        QuestionId = questionId;
        UserId = userId;
        Notes = notes;
    }

    public void UpdateNotes(string? notes) => Notes = notes;
}
