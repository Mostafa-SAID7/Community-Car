using CommunityCar.Domain.Base.Interfaces;
using MediatR;

namespace CommunityCar.Domain.Events.Community;

/// <summary>
/// Domain event raised when a comment is soft-deleted
/// </summary>
public class CommentDeletedEvent : IDomainEvent, INotification
{
    public Guid CommentId { get; }
    public Guid EntityId { get; }
    public string EntityType { get; }
    public Guid UserId { get; }
    public string Content { get; }
    public int NewCommentCount { get; }
    public DateTimeOffset OccurredOn { get; }

    public CommentDeletedEvent(
        Guid commentId,
        Guid entityId,
        string entityType,
        Guid userId,
        string content,
        int newCommentCount)
    {
        CommentId = commentId;
        EntityId = entityId;
        EntityType = entityType;
        UserId = userId;
        Content = content;
        NewCommentCount = newCommentCount;
        OccurredOn = DateTimeOffset.UtcNow;
    }
}
