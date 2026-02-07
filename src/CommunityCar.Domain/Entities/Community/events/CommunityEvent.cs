using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.events;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.events;

public class CommunityEvent : AggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public string? Address { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    
    public Guid OrganizerId { get; private set; }
    public virtual ApplicationUser Organizer { get; private set; } = null!;
    
    public EventCategory Category { get; private set; }
    public EventStatus Status { get; private set; }
    
    public int MaxAttendees { get; private set; }
    public bool IsOnline { get; private set; }
    public string? OnlineUrl { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsFeatured { get; private set; }
    
    public int AttendeeCount => Attendees?.Count(a => a.Status == AttendeeStatus.Going) ?? 0;
    public int InterestedCount => Attendees?.Count(a => a.Status == AttendeeStatus.Interested) ?? 0;
    
    public virtual ICollection<EventAttendee> Attendees { get; private set; } = new List<EventAttendee>();
    public virtual ICollection<EventComment> Comments { get; private set; } = new List<EventComment>();

    private CommunityEvent() { }

    public CommunityEvent(
        string title, 
        string description, 
        DateTimeOffset startTime, 
        DateTimeOffset endTime, 
        string location,
        Guid organizerId,
        EventCategory category,
        int maxAttendees = 0,
        bool isOnline = false)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));
        Guard.Against.NullOrWhiteSpace(location, nameof(location));
        Guard.Against.Empty(organizerId, nameof(organizerId));

        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time");

        Title = title;
        Slug = SlugHelper.GenerateSlug(title);
        Description = description;
        StartTime = startTime;
        EndTime = endTime;
        Location = location;
        OrganizerId = organizerId;
        Category = category;
        MaxAttendees = maxAttendees;
        IsOnline = isOnline;
        Status = EventStatus.Draft;
    }

    public void Update(
        string title, 
        string description, 
        DateTimeOffset startTime, 
        DateTimeOffset endTime, 
        string location,
        EventCategory category,
        int maxAttendees,
        bool isOnline)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));
        Guard.Against.NullOrWhiteSpace(location, nameof(location));

        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time");

        Title = title;
        Slug = SlugHelper.GenerateSlug(title);
        Description = description;
        StartTime = startTime;
        EndTime = endTime;
        Location = location;
        Category = category;
        MaxAttendees = maxAttendees;
        IsOnline = isOnline;
    }

    public void SetAddress(string address, decimal? latitude = null, decimal? longitude = null)
    {
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
    }

    public void SetOnlineUrl(string? url) => OnlineUrl = url;
    public void SetImageUrl(string? url) => ImageUrl = url;
    public void SetFeatured(bool isFeatured) => IsFeatured = isFeatured;
    
    public void Publish() => Status = EventStatus.Published;
    public void Cancel() => Status = EventStatus.Cancelled;
    public void Complete() => Status = EventStatus.Completed;
    public void StartEvent() => Status = EventStatus.InProgress;

    public bool CanAcceptMoreAttendees()
    {
        if (MaxAttendees <= 0) return true;
        return AttendeeCount < MaxAttendees;
    }
}
