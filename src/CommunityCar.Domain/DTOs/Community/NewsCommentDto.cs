namespace CommunityCar.Domain.DTOs.Community;

public class NewsCommentDto
{
    public Guid Id { get; set; }
    public Guid NewsArticleId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public bool IsAuthor { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
