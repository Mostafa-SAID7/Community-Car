namespace CommunityCar.Domain.DTOs.Community;

public class FriendRequestDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string Slug { get; set; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; set; }
}
