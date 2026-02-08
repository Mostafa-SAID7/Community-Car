using CommunityCar.Domain.Base.Interfaces;
using MediatR;

namespace CommunityCar.Domain.Events.Community;

/// <summary>
/// Domain event raised when a comment is updated
/// </summary>
public class CommentUpdatedEvent : IDomainEvent, INotification
{
    public Guid CommentId { get; }
    public Guid EntityId { get; }
    public string EntityType { get; }
    public Guid UserId { get; }
    public string OldContent { get; }
    public string NewContent { get; }
    public DateTimeOffset OccurredOn { get; }

    public CommentUpdatedEvent(
        Guid commentId,
        Guid entityId,
        string entityType,
        Guid userId,
        string oldContent,
        string newContent)
    {
        CommentId = commentId;
        EntityId = entityId;
        EntityType = entityType;
        UserId = userId;
        OldContent = oldContent;
        NewContent = newContent;
        OccurredOn = DateTimeOffset.UtcNow;
    }
}
