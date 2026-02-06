using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Community.groups;

public class CommunityGroup : AggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public Guid CreatorId { get; set; }
}
