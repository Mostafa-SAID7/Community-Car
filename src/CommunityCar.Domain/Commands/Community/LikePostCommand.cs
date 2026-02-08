using CommunityCar.Domain.Base.Interfaces;

namespace CommunityCar.Domain.Commands.Community;

/// <summary>
/// Command to toggle like on a post (like if not liked, unlike if already liked)
/// </summary>
public class LikePostCommand : ICommand<LikePostResult>
{
    public Guid PostId { get; }
    public Guid UserId { get; }

    public LikePostCommand(Guid postId, Guid userId)
    {
        PostId = postId;
        UserId = userId;
    }
}

/// <summary>
/// Result of the like/unlike operation
/// </summary>
public class LikePostResult
{
    public bool IsLiked { get; set; }
    public int TotalLikes { get; set; }
}
