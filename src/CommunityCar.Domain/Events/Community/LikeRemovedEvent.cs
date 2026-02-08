using CommunityCar.Domain.Base.Interfaces;
using MediatR;

namespace CommunityCar.Domain.Events.Community;

/// <summary>
/// Domain event raised when a user unlikes content
/// </summary>
public class LikeRemovedEvent : IDomainEvent, INotification
{
    public Guid LikeId { get; }
    public Guid EntityId { get; }
    public string EntityType { get; }
    public Guid UserId { get; }
    public int NewLikeCount { get; }
    public DateTimeOffset OccurredOn { get; }

    public LikeRemovedEvent(
        Guid likeId,
        Guid entityId,
        string entityType,
        Guid userId,
        int newLikeCount)
    {
        LikeId = likeId;
        EntityId = entityId;
        EntityType = entityType;
        UserId = userId;
        NewLikeCount = newLikeCount;
        OccurredOn = DateTimeOffset.UtcNow;
    }
}
