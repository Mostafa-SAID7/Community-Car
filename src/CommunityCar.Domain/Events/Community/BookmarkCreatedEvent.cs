using CommunityCar.Domain.Base.Interfaces;
using MediatR;

namespace CommunityCar.Domain.Events.Community;

/// <summary>
/// Domain event raised when a user bookmarks content
/// </summary>
public class BookmarkCreatedEvent : IDomainEvent, INotification
{
    public Guid BookmarkId { get; }
    public Guid EntityId { get; }
    public string EntityType { get; } // "Question", "Post", "Guide", etc.
    public Guid UserId { get; }
    public DateTimeOffset OccurredOn { get; }

    public BookmarkCreatedEvent(
        Guid bookmarkId,
        Guid entityId,
        string entityType,
        Guid userId)
    {
        BookmarkId = bookmarkId;
        EntityId = entityId;
        EntityType = entityType;
        UserId = userId;
        OccurredOn = DateTimeOffset.UtcNow;
    }
}
