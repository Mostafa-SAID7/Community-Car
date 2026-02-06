using CommunityCar.Domain.Base;
using CommunityCar.Domain.Utilities;
using CommunityCar.Domain.Entities.Identity.Users;

namespace CommunityCar.Domain.Entities.Community.qa;

public class QuestionShare : BaseEntity
{
    public Guid QuestionId { get; private set; }
    public virtual Question Question { get; private set; } = null!;
    
    public Guid SharedByUserId { get; private set; }
    public virtual ApplicationUser SharedByUser { get; private set; } = null!;
    
    public string? Platform { get; private set; } // e.g., "Twitter", "Facebook", "Email", "Copy Link"
    public string? SharedUrl { get; private set; }

    private QuestionShare() { }

    public QuestionShare(Guid questionId, Guid sharedByUserId, string? platform = null, string? sharedUrl = null)
    {
        Guard.Against.Empty(questionId, nameof(questionId));
        Guard.Against.Empty(sharedByUserId, nameof(sharedByUserId));

        QuestionId = questionId;
        SharedByUserId = sharedByUserId;
        Platform = platform;
        SharedUrl = sharedUrl;
    }
}
