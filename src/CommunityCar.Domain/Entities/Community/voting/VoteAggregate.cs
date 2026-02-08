using CommunityCar.Domain.Base;
using CommunityCar.Domain.Events.Community;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.voting;

/// <summary>
/// Aggregate Root for Vote entities - manages domain events and business rules
/// Supports Questions, Answers, Posts, and any voteable entity
/// </summary>
public class VoteAggregate : AggregateRoot
{
    public Guid EntityId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public bool IsUpvote { get; private set; }
    public int ScoreDelta { get; private set; }

    private VoteAggregate() { }

    public VoteAggregate(Guid entityId, string entityType, Guid userId, bool isUpvote)
    {
        Guard.Against.Empty(entityId, nameof(entityId));
        Guard.Against.NullOrWhiteSpace(entityType, nameof(entityType));
        Guard.Against.Empty(userId, nameof(userId));

        EntityId = entityId;
        EntityType = entityType;
        UserId = userId;
        IsUpvote = isUpvote;
        ScoreDelta = isUpvote ? 1 : -1;

        AddDomainEvent(new VoteCreatedEvent(
            Id,
            EntityId,
            EntityType,
            UserId,
            IsUpvote,
            ScoreDelta,
            0)); // NewScore will be calculated by handler
    }

    /// <summary>
    /// Toggles vote direction (upvote ↔ downvote)
    /// </summary>
    public void SwitchVote()
    {
        var oldIsUpvote = IsUpvote;
        IsUpvote = !IsUpvote;
        ScoreDelta = IsUpvote ? 2 : -2;
        ModifiedAt = DateTimeOffset.UtcNow;

        // Use existing VoteSwitchedEvent (record type)
        AddDomainEvent(new VoteSwitchedEvent(
            Id,
            EntityId,
            EntityType,
            UserId,
            oldIsUpvote,
            IsUpvote,
            ScoreDelta,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Soft deletes the vote (user removes their vote)
    /// </summary>
    public void Remove(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy?.ToString();
        ScoreDelta = IsUpvote ? -1 : 1;

        AddDomainEvent(new VoteRemovedEvent(
            Id,
            EntityId,
            EntityType,
            UserId,
            IsUpvote,
            ScoreDelta,
            0)); // NewScore will be calculated by handler
    }

    /// <summary>
    /// Resurrects a soft-deleted vote
    /// </summary>
    public void Resurrect(bool newIsUpvote)
    {
        var wasDeleted = IsDeleted;
        var oldIsUpvote = IsUpvote;
        var directionChanged = oldIsUpvote != newIsUpvote;

        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        ModifiedAt = DateTimeOffset.UtcNow;

        if (directionChanged)
        {
            IsUpvote = newIsUpvote;
        }
        
        ScoreDelta = newIsUpvote ? 1 : -1;

        AddDomainEvent(new VoteResurrectedEvent(
            Id,
            EntityId,
            EntityType,
            UserId,
            newIsUpvote,
            directionChanged,
            ScoreDelta,
            0)); // NewScore will be calculated by handler
    }
}
