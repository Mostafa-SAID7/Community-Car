using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.post;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.post;

public class Post : AggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public PostType Type { get; set; }
    public PostStatus Status { get; set; }
    
    public Guid AuthorId { get; set; }
    public virtual ApplicationUser Author { get; set; } = null!;
    
    public Guid? GroupId { get; set; }
    
    // Media
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkTitle { get; set; }
    public string? LinkDescription { get; set; }
    
    // Metadata
    public string? Tags { get; set; } // JSON array
    public bool IsPinned { get; set; }
    public bool IsLocked { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    
    // Engagement
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int ShareCount { get; set; }
    public int CommentCount { get; set; }
    
    // Navigation
    public virtual ICollection<PostComment> Comments { get; set; } = new List<PostComment>();

    private Post() { }

    public Post(
        string title,
        string content,
        PostType type,
        Guid authorId,
        Guid? groupId = null)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Guard.Against.Empty(authorId, nameof(authorId));

        Title = title;
        Content = content;
        Type = type;
        AuthorId = authorId;
        GroupId = groupId;
        Status = PostStatus.Draft;
        Slug = SlugHelper.GenerateSlug(title);
    }

    public void Update(string title, string content, PostType type)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));

        Title = title;
        Content = content;
        Type = type;
        Slug = SlugHelper.GenerateSlug(title);
    }

    public void Publish()
    {
        Status = PostStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
    }

    public void Archive() => Status = PostStatus.Archived;
    public void Pin() => IsPinned = true;
    public void Unpin() => IsPinned = false;
    public void Lock() => IsLocked = true;
    public void Unlock() => IsLocked = false;
    
    public void IncrementViews() => ViewCount++;
    public void IncrementLikes() => LikeCount++;
    public void DecrementLikes() => LikeCount = Math.Max(0, LikeCount - 1);
    public void IncrementShares() => ShareCount++;
    public void IncrementComments() => CommentCount++;
    public void DecrementComments() => CommentCount = Math.Max(0, CommentCount - 1);
    
    public void SetMedia(string? imageUrl, string? videoUrl)
    {
        ImageUrl = imageUrl;
        VideoUrl = videoUrl;
    }
    
    public void SetLink(string? linkUrl, string? linkTitle, string? linkDescription)
    {
        LinkUrl = linkUrl;
        LinkTitle = linkTitle;
        LinkDescription = linkDescription;
    }
}
