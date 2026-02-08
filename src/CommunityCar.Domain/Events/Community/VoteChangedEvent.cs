using CommunityCar.Domain.Base.Interfaces;

namespace CommunityCar.Domain.Events.Community;

/// <summary>
/// Domain event raised when a vote direction is changed (up to down or down to up)
/// </summary>
public class VoteChangedEvent : IDomainEvent
{
    public Guid VoteId { get; }
    public Guid EntityId { get; }
    public string EntityType { get; } // "Question" or "Answer"
    public Guid UserId { get; }
    public bool OldIsUpvote { get; }
    public bool NewIsUpvote { get; }
    public int ScoreDelta { get; }
    public int NewScore { get; }
    public DateTimeOffset OccurredOn { get; }

    public VoteChangedEvent(
        Guid voteId,
        Guid entityId,
        string entityType,
        Guid userId,
        bool oldIsUpvote,
        bool newIsUpvote,
        int scoreDelta,
        int newScore)
    {
        VoteId = voteId;
        EntityId = entityId;
        EntityType = entityType;
        UserId = userId;
        OldIsUpvote = oldIsUpvote;
        NewIsUpvote = newIsUpvote;
        ScoreDelta = scoreDelta;
        NewScore = newScore;
        OccurredOn = DateTimeOffset.UtcNow;
    }
}
