using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.news;

public class NewsComment : BaseEntity
{
    public Guid NewsArticleId { get; private set; }
    public virtual NewsArticle NewsArticle { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public string Content { get; private set; } = string.Empty;
    public int LikeCount { get; private set; }

    private NewsComment() { }

    public NewsComment(Guid newsArticleId, Guid userId, string content)
    {
        Guard.Against.Empty(newsArticleId, nameof(newsArticleId));
        Guard.Against.Empty(userId, nameof(userId));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));

        NewsArticleId = newsArticleId;
        UserId = userId;
        Content = content;
    }

    public void Update(string content)
    {
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Content = content;
    }

    public void IncrementLikes() => LikeCount++;
    public void DecrementLikes() => LikeCount = Math.Max(0, LikeCount - 1);
}
