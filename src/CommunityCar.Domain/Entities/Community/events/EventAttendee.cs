using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.events;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.events;

public class EventAttendee : BaseEntity
{
    public Guid EventId { get; private set; }
    public virtual CommunityEvent Event { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public AttendeeStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private EventAttendee() { }

    public EventAttendee(Guid eventId, Guid userId, AttendeeStatus status)
    {
        Guard.Against.Empty(eventId, nameof(eventId));
        Guard.Against.Empty(userId, nameof(userId));

        EventId = eventId;
        UserId = userId;
        Status = status;
    }

    public void UpdateStatus(AttendeeStatus status) => Status = status;
    public void UpdateNotes(string? notes) => Notes = notes;
}
