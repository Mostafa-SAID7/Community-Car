using CommunityCar.Domain.Base;
using CommunityCar.Domain.Utilities;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.qa;

namespace CommunityCar.Domain.Entities.Community.post;

public class PostReaction : BaseEntity
{
    public Guid PostId { get; private set; }
    public virtual Post Post { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public virtual ApplicationUser User { get; private set; } = null!;
    
    public ReactionType ReactionType { get; private set; }

    private PostReaction() { }

    public PostReaction(Guid postId, Guid userId, ReactionType reactionType)
    {
        Guard.Against.Empty(postId, nameof(postId));
        Guard.Against.Empty(userId, nameof(userId));

        PostId = postId;
        UserId = userId;
        ReactionType = reactionType;
    }

    public void ChangeReaction(ReactionType reactionType) => ReactionType = reactionType;
}
