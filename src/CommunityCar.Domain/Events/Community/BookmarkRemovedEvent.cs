using CommunityCar.Domain.Base.Interfaces;
using MediatR;

namespace CommunityCar.Domain.Events.Community;

/// <summary>
/// Domain event raised when a user removes a bookmark
/// </summary>
public class BookmarkRemovedEvent : IDomainEvent, INotification
{
    public Guid BookmarkId { get; }
    public Guid EntityId { get; }
    public string EntityType { get; }
    public Guid UserId { get; }
    public DateTimeOffset OccurredOn { get; }

    public BookmarkRemovedEvent(
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
