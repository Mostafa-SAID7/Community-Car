using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.news;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.news;

public class NewsArticle : AggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public NewsStatus Status { get; set; }
    public NewsCategory Category { get; set; }
    
    public Guid AuthorId { get; set; }
    public virtual ApplicationUser Author { get; set; } = null!;
    
    // Metadata
    public string? Source { get; set; }
    public string? ExternalUrl { get; set; }
    public string? Tags { get; set; } // JSON array
    public bool IsFeatured { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    
    // Engagement
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int ShareCount { get; set; }
    public int CommentCount { get; set; }
    
    // SEO
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    
    // Navigation
    public virtual ICollection<NewsComment> Comments { get; set; } = new List<NewsComment>();

    private NewsArticle() { }

    public NewsArticle(
        string title,
        string content,
        string summary,
        NewsCategory category,
        Guid authorId)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Guard.Against.NullOrWhiteSpace(summary, nameof(summary));
        Guard.Against.Empty(authorId, nameof(authorId));

        Title = title;
        Content = content;
        Summary = summary;
        Category = category;
        AuthorId = authorId;
        Status = NewsStatus.Draft;
        Slug = SlugHelper.GenerateSlug(title);
    }

    public void Update(string title, string content, string summary, NewsCategory category)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Guard.Against.NullOrWhiteSpace(summary, nameof(summary));

        Title = title;
        Content = content;
        Summary = summary;
        Category = category;
        Slug = SlugHelper.GenerateSlug(title);
    }

    public void Publish()
    {
        Status = NewsStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
    }

    public void Archive() => Status = NewsStatus.Archived;
    public void Feature() => IsFeatured = true;
    public void Unfeature() => IsFeatured = false;
    
    public void IncrementViews() => ViewCount++;
    public void IncrementLikes() => LikeCount++;
    public void DecrementLikes() => LikeCount = Math.Max(0, LikeCount - 1);
    public void IncrementShares() => ShareCount++;
    public void IncrementComments() => CommentCount++;
    public void DecrementComments() => CommentCount = Math.Max(0, CommentCount - 1);
    
    public void SetImage(string? imageUrl) => ImageUrl = imageUrl;
    public void SetSource(string? source, string? externalUrl)
    {
        Source = source;
        ExternalUrl = externalUrl;
    }
}
