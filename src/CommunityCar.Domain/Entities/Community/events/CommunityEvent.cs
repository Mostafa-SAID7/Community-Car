using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Community.events;

public class CommunityEvent : AggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public Guid OrganizerId { get; set; }
}
