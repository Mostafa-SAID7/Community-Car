using CommunityCar.Domain.Enums.Community.news;

namespace CommunityCar.Domain.DTOs.Community;

public class NewsArticleDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public NewsStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public NewsCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    
    public string? Source { get; set; }
    public string? ExternalUrl { get; set; }
    public string? Tags { get; set; }
    public bool IsFeatured { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int ShareCount { get; set; }
    public int CommentCount { get; set; }
    
    public bool IsAuthor { get; set; }
    public bool IsLiked { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
