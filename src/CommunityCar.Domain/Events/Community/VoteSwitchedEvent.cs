using CommunityCar.Domain.Base.Interfaces;
using MediatR;

namespace CommunityCar.Domain.Events.Community;

/// <summary>
/// Domain event raised when a vote direction is switched (upvote ↔ downvote)
/// </summary>
public record VoteSwitchedEvent(
    Guid VoteId,
    Guid EntityId,
    string EntityType,
    Guid UserId,
    bool OldIsUpvote,
    bool NewIsUpvote,
    int ScoreDelta,
    DateTimeOffset OccurredOn) : IDomainEvent, INotification;
