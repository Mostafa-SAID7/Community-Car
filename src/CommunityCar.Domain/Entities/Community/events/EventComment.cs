using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.events;

public class EventComment : BaseEntity
{
    public Guid EventId { get; private set; }
    public virtual CommunityEvent Event { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public string Content { get; private set; } = string.Empty;

    private EventComment() { }

    public EventComment(Guid eventId, Guid userId, string content)
    {
        Guard.Against.Empty(eventId, nameof(eventId));
        Guard.Against.Empty(userId, nameof(userId));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));

        EventId = eventId;
        UserId = userId;
        Content = content;
    }

    public void Update(string content)
    {
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Content = content;
    }
}
