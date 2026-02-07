using CommunityCar.Domain.Enums.Community.groups;

namespace CommunityCar.Domain.DTOs.Community;

public class GroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public bool IsPrivate { get; set; }
    public Guid CreatorId { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int MemberCount { get; set; }
    public DateTimeOffset? LastActivityAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsUserMember { get; set; }
    public GroupMemberRole? UserRole { get; set; }
}

public class GroupMemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? Slug { get; set; }
    public GroupMemberRole Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
}
