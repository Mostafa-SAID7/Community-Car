using CommunityCar.Domain.Enums.Community.post;

namespace CommunityCar.Domain.DTOs.Community;

public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public PostType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public PostCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public PostStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    
    public Guid? GroupId { get; set; }
    public string? GroupName { get; set; }
    
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkTitle { get; set; }
    public string? LinkDescription { get; set; }
    
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    
    public bool IsPinned { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsLocked { get; set; }
    public bool AllowComments { get; set; }
    
    public string? Tags { get; set; }
    
    public bool IsAuthor { get; set; }
    public bool IsLiked { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}
