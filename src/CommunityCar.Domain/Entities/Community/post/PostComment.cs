using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.post;

public class PostComment : BaseEntity
{
    public Guid PostId { get; set; }
    public virtual Post Post { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
    
    public string Content { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    
    public Guid? ParentCommentId { get; set; }
    public virtual PostComment? ParentComment { get; set; }
    public virtual ICollection<PostComment> Replies { get; set; } = new List<PostComment>();

    private PostComment() { }

    public PostComment(Guid postId, Guid userId, string content, Guid? parentCommentId = null)
    {
        Guard.Against.Empty(postId, nameof(postId));
        Guard.Against.Empty(userId, nameof(userId));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));

        PostId = postId;
        UserId = userId;
        Content = content;
        ParentCommentId = parentCommentId;
    }

    public void Update(string content)
    {
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Content = content;
    }

    public void IncrementLikes() => LikeCount++;
    public void DecrementLikes() => LikeCount = Math.Max(0, LikeCount - 1);
}
