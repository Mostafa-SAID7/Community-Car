using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Community.friends;

namespace CommunityCar.Domain.Entities.Community.friends;

public class Friendship : BaseEntity
{
    public Guid UserId { get; private set; }
    public virtual CommunityCar.Domain.Entities.Identity.Users.ApplicationUser User { get; private set; } = null!;
    
    public Guid FriendId { get; private set; }
    public virtual CommunityCar.Domain.Entities.Identity.Users.ApplicationUser Friend { get; private set; } = null!;

    public FriendshipStatus Status { get; private set; }

    private Friendship() { }

    public Friendship(Guid userId, Guid friendId)
    {
        UserId = userId;
        FriendId = friendId;
        Status = FriendshipStatus.Pending;
    }

    public void Accept() => Status = FriendshipStatus.Accepted;
    public void Block() => Status = FriendshipStatus.Blocked;
}
