using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Community.post;

public class Post : AggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
    public Guid? GroupId { get; set; }
}
