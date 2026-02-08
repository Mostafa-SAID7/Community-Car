using CommunityCar.Domain.Base.Interfaces;
using MediatR;

namespace CommunityCar.Domain.Events.Community;

/// <summary>
/// Domain event raised when a new vote is created
/// </summary>
public class VoteCreatedEvent : IDomainEvent, INotification
{
    public Guid VoteId { get; }
    public Guid EntityId { get; }
    public string EntityType { get; } // "Question" or "Answer"
    public Guid UserId { get; }
    public bool IsUpvote { get; }
    public int ScoreDelta { get; }
    public int NewScore { get; }
    public DateTimeOffset OccurredOn { get; }

    public VoteCreatedEvent(
        Guid voteId,
        Guid entityId,
        string entityType,
        Guid userId,
        bool isUpvote,
        int scoreDelta,
        int newScore)
    {
        VoteId = voteId;
        EntityId = entityId;
        EntityType = entityType;
        UserId = userId;
        IsUpvote = isUpvote;
        ScoreDelta = scoreDelta;
        NewScore = newScore;
        OccurredOn = DateTimeOffset.UtcNow;
    }
}
