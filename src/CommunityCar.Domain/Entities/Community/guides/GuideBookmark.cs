using CommunityCar.Domain.Base;
using CommunityCar.Domain.Utilities;
using CommunityCar.Domain.Entities.Identity.Users;

namespace CommunityCar.Domain.Entities.Community.guides;

public class GuideBookmark : BaseEntity
{
    public Guid GuideId { get; private set; }
    public virtual Guide Guide { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public string? Notes { get; private set; }

    private GuideBookmark() { }

    public GuideBookmark(Guid guideId, Guid userId, string? notes = null)
    {
        Guard.Against.Empty(guideId, nameof(guideId));
        Guard.Against.Empty(userId, nameof(userId));

        GuideId = guideId;
        UserId = userId;
        Notes = notes;
    }

    public void UpdateNotes(string? notes) => Notes = notes;
}
