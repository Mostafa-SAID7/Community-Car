using CommunityCar.Domain.Base.Interfaces;
using MediatR;

namespace CommunityCar.Domain.Events.Community;

/// <summary>
/// Domain event raised when a soft-deleted vote is resurrected
/// </summary>
public class VoteResurrectedEvent : IDomainEvent, INotification
{
    public Guid VoteId { get; }
    public Guid EntityId { get; }
    public string EntityType { get; } // "Question" or "Answer"
    public Guid UserId { get; }
    public bool IsUpvote { get; }
    public bool DirectionChanged { get; }
    public int ScoreDelta { get; }
    public int NewScore { get; }
    public DateTimeOffset OccurredOn { get; }

    public VoteResurrectedEvent(
        Guid voteId,
        Guid entityId,
        string entityType,
        Guid userId,
        bool isUpvote,
        bool directionChanged,
        int scoreDelta,
        int newScore)
    {
        VoteId = voteId;
        EntityId = entityId;
        EntityType = entityType;
        UserId = userId;
        IsUpvote = isUpvote;
        DirectionChanged = directionChanged;
        ScoreDelta = scoreDelta;
        NewScore = newScore;
        OccurredOn = DateTimeOffset.UtcNow;
    }
}
