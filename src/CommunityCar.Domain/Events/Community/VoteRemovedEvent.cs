using CommunityCar.Domain.Base.Interfaces;
using MediatR;

namespace CommunityCar.Domain.Events.Community;

/// <summary>
/// Domain event raised when a vote is soft-deleted (removed)
/// </summary>
public class VoteRemovedEvent : IDomainEvent, INotification
{
    public Guid VoteId { get; }
    public Guid EntityId { get; }
    public string EntityType { get; } // "Question" or "Answer"
    public Guid UserId { get; }
    public bool WasUpvote { get; }
    public int ScoreDelta { get; }
    public int NewScore { get; }
    public DateTimeOffset OccurredOn { get; }

    public VoteRemovedEvent(
        Guid voteId,
        Guid entityId,
        string entityType,
        Guid userId,
        bool wasUpvote,
        int scoreDelta,
        int newScore)
    {
        VoteId = voteId;
        EntityId = entityId;
        EntityType = entityType;
        UserId = userId;
        WasUpvote = wasUpvote;
        ScoreDelta = scoreDelta;
        NewScore = newScore;
        OccurredOn = DateTimeOffset.UtcNow;
    }
}
