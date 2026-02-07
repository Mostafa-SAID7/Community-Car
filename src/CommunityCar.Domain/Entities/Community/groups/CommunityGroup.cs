using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.groups;

public class CommunityGroup : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsPrivate { get; private set; }
    public Guid CreatorId { get; private set; }
    public virtual ApplicationUser Creator { get; private set; } = null!;
    
    public string? ImageUrl { get; private set; }
    public int MemberCount { get; private set; }
    public DateTimeOffset? LastActivityAt { get; private set; }
    
    public virtual ICollection<GroupMember> Members { get; private set; } = new List<GroupMember>();

    private CommunityGroup() { }

    public CommunityGroup(string name, string description, Guid creatorId, bool isPrivate = false)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));
        Guard.Against.Empty(creatorId, nameof(creatorId));

        Name = name;
        Description = description;
        CreatorId = creatorId;
        IsPrivate = isPrivate;
        Slug = SlugHelper.GenerateSlug(name);
        MemberCount = 1; // Creator is the first member
        LastActivityAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name, string description, bool isPrivate)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        Name = name;
        Description = description;
        IsPrivate = isPrivate;
        Slug = SlugHelper.GenerateSlug(name);
    }

    public void SetImage(string? imageUrl) => ImageUrl = imageUrl;
    
    public void IncrementMemberCount() => MemberCount++;
    public void DecrementMemberCount() => MemberCount = Math.Max(0, MemberCount - 1);
    
    public void UpdateActivity() => LastActivityAt = DateTimeOffset.UtcNow;
}
