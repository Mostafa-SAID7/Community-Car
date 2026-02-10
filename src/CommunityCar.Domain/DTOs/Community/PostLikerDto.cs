namespace CommunityCar.Domain.DTOs.Community;

public class PostLikerDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public DateTimeOffset LikedAt { get; set; }
}
