using CommunityCar.Domain.Base.Interfaces;
using MediatR;

namespace CommunityCar.Domain.Events.Community;

/// <summary>
/// Domain event raised when a user updates their rating
/// </summary>
public class RatingUpdatedEvent : IDomainEvent, INotification
{
    public Guid RatingId { get; }
    public Guid EntityId { get; }
    public string EntityType { get; }
    public Guid UserId { get; }
    public int OldRatingValue { get; }
    public int NewRatingValue { get; }
    public decimal NewAverageRating { get; }
    public DateTimeOffset OccurredOn { get; }

    public RatingUpdatedEvent(
        Guid ratingId,
        Guid entityId,
        string entityType,
        Guid userId,
        int oldRatingValue,
        int newRatingValue,
        decimal newAverageRating)
    {
        RatingId = ratingId;
        EntityId = entityId;
        EntityType = entityType;
        UserId = userId;
        OldRatingValue = oldRatingValue;
        NewRatingValue = newRatingValue;
        NewAverageRating = newAverageRating;
        OccurredOn = DateTimeOffset.UtcNow;
    }
}
