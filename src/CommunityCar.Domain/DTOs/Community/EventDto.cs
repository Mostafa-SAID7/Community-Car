using CommunityCar.Domain.Enums.Community.events;

namespace CommunityCar.Domain.DTOs.Community;

public class EventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public DateTime StartDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    public Guid OrganizerId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public string? OrganizerAvatar { get; set; }
    
    public EventCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public EventStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    
    public int MaxAttendees { get; set; }
    public int AttendeeCount { get; set; }
    public int InterestedCount { get; set; }
    public int CommentCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsOnline { get; set; }
    public string? OnlineUrl { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    
    public bool IsOrganizer { get; set; }
    public AttendeeStatus? CurrentUserStatus { get; set; }
    public bool CanJoin { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
