using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Community.news;

public class NewsArticle : AggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Guid AuthorId { get; set; }
}
