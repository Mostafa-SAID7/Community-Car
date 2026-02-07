using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.groups;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.groups;

public class GroupMember : BaseEntity
{
    public Guid GroupId { get; private set; }
    public virtual CommunityGroup Group { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public GroupMemberRole Role { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }

    private GroupMember() { }

    public GroupMember(Guid groupId, Guid userId, GroupMemberRole role = GroupMemberRole.Member)
    {
        Guard.Against.Empty(groupId, nameof(groupId));
        Guard.Against.Empty(userId, nameof(userId));

        GroupId = groupId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTimeOffset.UtcNow;
    }

    public void PromoteToAdmin() => Role = GroupMemberRole.Admin;
    public void PromoteToModerator() => Role = GroupMemberRole.Moderator;
    public void DemoteToMember() => Role = GroupMemberRole.Member;
}
