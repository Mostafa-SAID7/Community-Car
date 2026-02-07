namespace CommunityCar.Domain.DTOs.Community;

public class MapPointReviewDto
{
    public Guid Id { get; set; }
    public Guid MapPointId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? ImageUrl { get; set; }
    public int HelpfulCount { get; set; }
    public bool IsAuthor { get; set; }
    public bool IsHelpful { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
