using CommunityCar.Domain.Enums.Community.friends;

namespace CommunityCar.Domain.DTOs.Community;

public class FriendshipDto
{
    public Guid Id { get; set; }
    public Guid FriendId { get; set; }
    public string? Slug { get; set; }
    public string FriendName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public FriendshipStatus Status { get; set; }
    public DateTimeOffset Since { get; set; }
}


