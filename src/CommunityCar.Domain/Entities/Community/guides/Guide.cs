using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Community.guides;

public class Guide : AggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
}
