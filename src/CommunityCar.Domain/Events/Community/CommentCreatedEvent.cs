using CommunityCar.Domain.Base.Interfaces;
using MediatR;

namespace CommunityCar.Domain.Events.Community;

/// <summary>
/// Domain event raised when a comment is created
/// </summary>
public class CommentCreatedEvent : IDomainEvent, INotification
{
    public Guid CommentId { get; }
    public Guid EntityId { get; }
    public string EntityType { get; } // "Post", "News", "Guide", "Event", "Answer", etc.
    public Guid UserId { get; }
    public string Content { get; }
    public Guid? ParentCommentId { get; } // For replies
    public int NewCommentCount { get; }
    public DateTimeOffset OccurredOn { get; }

    public CommentCreatedEvent(
        Guid commentId,
        Guid entityId,
        string entityType,
        Guid userId,
        string content,
        Guid? parentCommentId,
        int newCommentCount)
    {
        CommentId = commentId;
        EntityId = entityId;
        EntityType = entityType;
        UserId = userId;
        Content = content;
        ParentCommentId = parentCommentId;
        NewCommentCount = newCommentCount;
        OccurredOn = DateTimeOffset.UtcNow;
    }
}
