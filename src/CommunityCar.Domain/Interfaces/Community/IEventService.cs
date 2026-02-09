using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.events;
using CommunityCar.Domain.Enums.Community.events;

namespace CommunityCar.Domain.Interfaces.Community;

public interface IEventService
{
    Task<CommunityEvent> CreateEventAsync(
        string title, 
        string description, 
        DateTimeOffset startTime, 
        DateTimeOffset endTime, 
        string location,
        Guid organizerId,
        EventCategory category,
        int maxAttendees = 0,
        bool isOnline = false);
    
    Task<CommunityEvent> UpdateEventAsync(
        Guid eventId, 
        string title, 
        string description, 
        DateTimeOffset startTime, 
        DateTimeOffset endTime, 
        string location,
        EventCategory category,
        int maxAttendees,
        bool isOnline);
    
    Task DeleteEventAsync(Guid eventId);
    Task<EventDto?> GetEventByIdAsync(Guid eventId, Guid? currentUserId = null);
    Task<EventDto?> GetEventBySlugAsync(string slug, Guid? currentUserId = null);
    Task<PagedResult<EventDto>> GetEventsAsync(
        QueryParameters parameters, 
        EventCategory? category = null, 
        EventStatus? status = null,
        bool? isFeatured = null,
        bool? isUpcoming = null,
        Guid? currentUserId = null);
    
    Task<PagedResult<EventDto>> GetUserEventsAsync(Guid userId, QueryParameters parameters, bool asOrganizer = false);
    Task<PagedResult<EventDto>> GetUpcomingEventsAsync(QueryParameters parameters, Guid? currentUserId = null);
    Task<PagedResult<EventDto>> GetFeaturedEventsAsync(QueryParameters parameters, Guid? currentUserId = null);
    
    Task PublishEventAsync(Guid eventId, Guid userId);
    Task CancelEventAsync(Guid eventId, Guid userId);
    Task CompleteEventAsync(Guid eventId, Guid userId);
    
    Task<EventAttendee> JoinEventAsync(Guid eventId, Guid userId, AttendeeStatus status);
    Task UpdateAttendanceAsync(Guid eventId, Guid userId, AttendeeStatus status);
    Task LeaveEventAsync(Guid eventId, Guid userId);
    Task<PagedResult<EventAttendeeDto>> GetEventAttendeesAsync(Guid eventId, QueryParameters parameters, AttendeeStatus? status = null);
    
    Task<EventComment> AddCommentAsync(Guid eventId, Guid userId, string content);
    Task<EventComment> UpdateCommentAsync(Guid commentId, Guid userId, string content);
    Task DeleteCommentAsync(Guid commentId, Guid userId);
    Task<PagedResult<EventCommentDto>> GetEventCommentsAsync(Guid eventId, QueryParameters parameters, Guid? currentUserId = null);
    
    Task SetEventImageAsync(Guid eventId, string imageUrl);
}
