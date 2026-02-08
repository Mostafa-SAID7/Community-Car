using AutoMapper;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.events;
using CommunityCar.Domain.Enums.Community.events;
using CommunityCar.Domain.Exceptions;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

public class EventService : IEventService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<EventService> _logger;

    public EventService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<EventService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CommunityEvent> CreateEventAsync(
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
        var communityEvent = new CommunityEvent(
            title,
            description,
            startTime,
            endTime,
            location,
            organizerId,
            category,
            maxAttendees,
            isOnline);

        // Ensure slug uniqueness
        var baseSlug = communityEvent.Slug;
        var slug = baseSlug;
        var counter = 1;
        
        while (await _context.Set<CommunityEvent>().AnyAsync(e => e.Slug == slug && !e.IsDeleted))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }
        
        // Set the unique slug directly (Slug property is public in BaseEntity)
        communityEvent.Slug = slug;

        // Auto-publish the event so it appears in the index immediately
        communityEvent.Publish();

        _context.Set<CommunityEvent>().Add(communityEvent);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Event created and published: {EventId} by user {UserId}", communityEvent.Id, organizerId);
        return communityEvent;
    }

    public async Task<CommunityEvent> UpdateEventAsync(
        Guid eventId,
        string title,
        string description,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        string location,
        EventCategory category,
        int maxAttendees,
        bool isOnline)
    {
        var communityEvent = await _context.Set<CommunityEvent>()
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (communityEvent == null)
            throw new NotFoundException("Event not found");

        communityEvent.Update(title, description, startTime, endTime, location, category, maxAttendees, isOnline);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Event updated: {EventId}", eventId);
        return communityEvent;
    }

    public async Task DeleteEventAsync(Guid eventId)
    {
        var communityEvent = await _context.Set<CommunityEvent>()
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (communityEvent == null)
            throw new NotFoundException("Event not found");

        _context.Set<CommunityEvent>().Remove(communityEvent);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Event deleted: {EventId}", eventId);
    }

    public async Task<EventDto?> GetEventByIdAsync(Guid eventId, Guid? currentUserId = null)
    {
        var communityEvent = await _context.Set<CommunityEvent>()
            .Include(e => e.Organizer)
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (communityEvent == null)
            return null;

        return MapToDto(communityEvent, currentUserId);
    }

    public async Task<EventDto?> GetEventBySlugAsync(string slug, Guid? currentUserId = null)
    {
        var communityEvent = await _context.Set<CommunityEvent>()
            .Include(e => e.Organizer)
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Slug == slug);

        if (communityEvent == null)
            return null;

        return MapToDto(communityEvent, currentUserId);
    }

    public async Task<PagedResult<EventDto>> GetEventsAsync(
        QueryParameters parameters,
        EventCategory? category = null,
        EventStatus? status = null,
        bool? isFeatured = null,
        bool? isUpcoming = null,
        Guid? currentUserId = null)
    {
        var query = _context.Set<CommunityEvent>()
            .Include(e => e.Organizer)
            .Include(e => e.Attendees)
            .AsQueryable();

        if (category.HasValue)
            query = query.Where(e => e.Category == category.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);
        else
            query = query.Where(e => e.Status == EventStatus.Published);

        if (isFeatured.HasValue)
            query = query.Where(e => e.IsFeatured == isFeatured.Value);

        if (isUpcoming == true)
            query = query.Where(e => e.StartTime > DateTimeOffset.UtcNow);

        query = query.OrderBy(e => e.StartTime);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = items.Select(e => MapToDto(e, currentUserId)).ToList();

        return new PagedResult<EventDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<EventDto>> GetUserEventsAsync(Guid userId, QueryParameters parameters, bool asOrganizer = false)
    {
        var query = _context.Set<CommunityEvent>()
            .Include(e => e.Organizer)
            .Include(e => e.Attendees)
            .AsQueryable();

        if (asOrganizer)
        {
            query = query.Where(e => e.OrganizerId == userId);
        }
        else
        {
            query = query.Where(e => e.Attendees.Any(a => a.UserId == userId && a.Status == AttendeeStatus.Going));
        }

        query = query.OrderByDescending(e => e.StartTime);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = items.Select(e => MapToDto(e, userId)).ToList();

        return new PagedResult<EventDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<EventDto>> GetUpcomingEventsAsync(QueryParameters parameters, Guid? currentUserId = null)
    {
        return await GetEventsAsync(parameters, status: EventStatus.Published, isUpcoming: true, currentUserId: currentUserId);
    }

    public async Task<PagedResult<EventDto>> GetFeaturedEventsAsync(QueryParameters parameters, Guid? currentUserId = null)
    {
        return await GetEventsAsync(parameters, status: EventStatus.Published, isFeatured: true, currentUserId: currentUserId);
    }

    public async Task PublishEventAsync(Guid eventId, Guid userId)
    {
        var communityEvent = await _context.Set<CommunityEvent>()
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (communityEvent == null)
            throw new NotFoundException("Event not found");

        if (communityEvent.OrganizerId != userId)
            throw new ForbiddenException("Only the organizer can publish this event");

        communityEvent.Publish();
        await _context.SaveChangesAsync();

        _logger.LogInformation("Event published: {EventId}", eventId);
    }

    public async Task CancelEventAsync(Guid eventId, Guid userId)
    {
        var communityEvent = await _context.Set<CommunityEvent>()
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (communityEvent == null)
            throw new NotFoundException("Event not found");

        if (communityEvent.OrganizerId != userId)
            throw new ForbiddenException("Only the organizer can cancel this event");

        communityEvent.Cancel();
        await _context.SaveChangesAsync();

        _logger.LogInformation("Event cancelled: {EventId}", eventId);
    }

    public async Task CompleteEventAsync(Guid eventId, Guid userId)
    {
        var communityEvent = await _context.Set<CommunityEvent>()
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (communityEvent == null)
            throw new NotFoundException("Event not found");

        if (communityEvent.OrganizerId != userId)
            throw new ForbiddenException("Only the organizer can complete this event");

        communityEvent.Complete();
        await _context.SaveChangesAsync();

        _logger.LogInformation("Event completed: {EventId}", eventId);
    }

    public async Task<EventAttendee> JoinEventAsync(Guid eventId, Guid userId, AttendeeStatus status)
    {
        var communityEvent = await _context.Set<CommunityEvent>()
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (communityEvent == null)
            throw new NotFoundException("Event not found");

        var existingAttendee = communityEvent.Attendees.FirstOrDefault(a => a.UserId == userId);
        if (existingAttendee != null)
        {
            existingAttendee.UpdateStatus(status);
        }
        else
        {
            if (status == AttendeeStatus.Going && !communityEvent.CanAcceptMoreAttendees())
                throw new ConflictException("Event is full");

            var attendee = new EventAttendee(eventId, userId, status);
            _context.Set<EventAttendee>().Add(attendee);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("User {UserId} joined event {EventId} with status {Status}", userId, eventId, status);

        return existingAttendee ?? await _context.Set<EventAttendee>()
            .FirstAsync(a => a.EventId == eventId && a.UserId == userId);
    }

    public async Task UpdateAttendanceAsync(Guid eventId, Guid userId, AttendeeStatus status)
    {
        var attendee = await _context.Set<EventAttendee>()
            .FirstOrDefaultAsync(a => a.EventId == eventId && a.UserId == userId);

        if (attendee == null)
            throw new NotFoundException("Attendance record not found");

        attendee.UpdateStatus(status);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated attendance for event {EventId} to {Status}", userId, eventId, status);
    }

    public async Task LeaveEventAsync(Guid eventId, Guid userId)
    {
        var attendee = await _context.Set<EventAttendee>()
            .FirstOrDefaultAsync(a => a.EventId == eventId && a.UserId == userId);

        if (attendee == null)
            throw new NotFoundException("Attendance record not found");

        _context.Set<EventAttendee>().Remove(attendee);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} left event {EventId}", userId, eventId);
    }

    public async Task<PagedResult<EventAttendeeDto>> GetEventAttendeesAsync(
        Guid eventId,
        QueryParameters parameters,
        AttendeeStatus? status = null)
    {
        var query = _context.Set<EventAttendee>()
            .Include(a => a.User)
            .Where(a => a.EventId == eventId);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        query = query.OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = items.Select(a => new EventAttendeeDto
        {
            Id = a.Id,
            EventId = a.EventId,
            UserId = a.UserId,
            UserName = a.User.UserName ?? "Unknown",
            UserAvatar = a.User.ProfilePictureUrl,
            Status = a.Status,
            StatusName = a.Status.ToString(),
            Notes = a.Notes,
            CreatedAt = a.CreatedAt
        }).ToList();

        return new PagedResult<EventAttendeeDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<EventComment> AddCommentAsync(Guid eventId, Guid userId, string content)
    {
        var comment = new EventComment(eventId, userId, content);
        _context.Set<EventComment>().Add(comment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Comment added to event {EventId} by user {UserId}", eventId, userId);
        return comment;
    }

    public async Task<EventComment> UpdateCommentAsync(Guid commentId, Guid userId, string content)
    {
        var comment = await _context.Set<EventComment>()
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new NotFoundException("Comment not found");

        if (comment.UserId != userId)
            throw new ForbiddenException("You can only edit your own comments");

        comment.Update(content);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} updated", commentId);
        return comment;
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _context.Set<EventComment>()
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new NotFoundException("Comment not found");

        if (comment.UserId != userId)
            throw new ForbiddenException("You can only delete your own comments");

        _context.Set<EventComment>().Remove(comment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} deleted", commentId);
    }

    public async Task<PagedResult<EventCommentDto>> GetEventCommentsAsync(
        Guid eventId,
        QueryParameters parameters,
        Guid? currentUserId = null)
    {
        var query = _context.Set<EventComment>()
            .Include(c => c.User)
            .Where(c => c.EventId == eventId)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = items.Select(c => new EventCommentDto
        {
            Id = c.Id,
            EventId = c.EventId,
            UserId = c.UserId,
            UserName = c.User.UserName ?? "Unknown",
            UserAvatar = c.User.ProfilePictureUrl,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.ModifiedAt,
            IsAuthor = currentUserId.HasValue && c.UserId == currentUserId.Value
        }).ToList();

        return new PagedResult<EventCommentDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    private EventDto MapToDto(CommunityEvent communityEvent, Guid? currentUserId)
    {
        var currentUserAttendance = currentUserId.HasValue
            ? communityEvent.Attendees.FirstOrDefault(a => a.UserId == currentUserId.Value)
            : null;

        return new EventDto
        {
            Id = communityEvent.Id,
            Title = communityEvent.Title,
            Slug = communityEvent.Slug,
            Description = communityEvent.Description,
            StartTime = communityEvent.StartTime,
            EndTime = communityEvent.EndTime,
            Location = communityEvent.Location,
            Address = communityEvent.Address,
            Latitude = communityEvent.Latitude,
            Longitude = communityEvent.Longitude,
            OrganizerId = communityEvent.OrganizerId,
            OrganizerName = communityEvent.Organizer?.UserName ?? "Unknown",
            OrganizerAvatar = communityEvent.Organizer?.ProfilePictureUrl,
            Category = communityEvent.Category,
            CategoryName = communityEvent.Category.ToString(),
            Status = communityEvent.Status,
            StatusName = communityEvent.Status.ToString(),
            MaxAttendees = communityEvent.MaxAttendees,
            AttendeeCount = communityEvent.AttendeeCount,
            InterestedCount = communityEvent.InterestedCount,
            IsOnline = communityEvent.IsOnline,
            OnlineUrl = communityEvent.OnlineUrl,
            ImageUrl = communityEvent.ImageUrl,
            IsFeatured = communityEvent.IsFeatured,
            IsOrganizer = currentUserId.HasValue && communityEvent.OrganizerId == currentUserId.Value,
            CurrentUserStatus = currentUserAttendance?.Status,
            CanJoin = communityEvent.CanAcceptMoreAttendees(),
            CreatedAt = communityEvent.CreatedAt,
            UpdatedAt = communityEvent.ModifiedAt
        };
    }
}
