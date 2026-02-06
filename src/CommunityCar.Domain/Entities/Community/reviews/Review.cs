using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Community.reviews;

public class Review : AggregateRoot
{
    public Guid EntityId { get; set; } // The ID of what is being reviewed (e.g., Guide, News, Event)
    public string EntityType { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public Guid ReviewerId { get; set; }
}
