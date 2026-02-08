using CommunityCar.Domain.Base.Interfaces;
using MediatR;

namespace CommunityCar.Domain.Events.Community;

/// <summary>
/// Domain event raised when a user removes their rating
/// </summary>
public class RatingRemovedEvent : IDomainEvent, INotification
{
    public Guid RatingId { get; }
    public Guid EntityId { get; }
    public string EntityType { get; }
    public Guid UserId { get; }
    public int RatingValue { get; }
    public decimal NewAverageRating { get; }
    public int NewRatingCount { get; }
    public DateTimeOffset OccurredOn { get; }

    public RatingRemovedEvent(
        Guid ratingId,
        Guid entityId,
        string entityType,
        Guid userId,
        int ratingValue,
        decimal newAverageRating,
        int newRatingCount)
    {
        RatingId = ratingId;
        EntityId = entityId;
        EntityType = entityType;
        UserId = userId;
        RatingValue = ratingValue;
        NewAverageRating = newAverageRating;
        NewRatingCount = newRatingCount;
        OccurredOn = DateTimeOffset.UtcNow;
    }
}
