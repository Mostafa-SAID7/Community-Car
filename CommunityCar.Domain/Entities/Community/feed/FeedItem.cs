using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Community.feed;

public class FeedItem : BaseEntity
{
    public string Type { get; set; } = string.Empty; // e.g., "Post", "Event", "News"
    public Guid SourceId { get; set; }
    public string Summary { get; set; } = string.Empty;
}
