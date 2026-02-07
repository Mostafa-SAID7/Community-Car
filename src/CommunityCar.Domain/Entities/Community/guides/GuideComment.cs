using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.guides;

public class GuideComment : BaseEntity
{
    public Guid GuideId { get; private set; }
    public virtual Guide Guide { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public string Content { get; private set; } = string.Empty;
    public int LikeCount { get; private set; }

    private GuideComment() { }

    public GuideComment(Guid guideId, Guid userId, string content)
    {
        Guard.Against.Empty(guideId, nameof(guideId));
        Guard.Against.Empty(userId, nameof(userId));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));

        GuideId = guideId;
        UserId = userId;
        Content = content;
    }

    public void Update(string content)
    {
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Content = content;
    }

    public void IncrementLikes() => LikeCount++;
    public void DecrementLikes() => LikeCount = Math.Max(0, LikeCount - 1);
}
