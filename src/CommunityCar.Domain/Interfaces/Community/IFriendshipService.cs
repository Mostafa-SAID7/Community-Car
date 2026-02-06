using CommunityCar.Domain.Entities.Community.friends;

namespace CommunityCar.Domain.Interfaces.Community;

public interface IFriendshipService
{
    Task SendRequestAsync(Guid userId, Guid friendId);
    Task AcceptRequestAsync(Guid userId, Guid friendId);
    Task RejectRequestAsync(Guid userId, Guid friendId);
    Task BlockUserAsync(Guid userId, Guid friendId);
    Task<IEnumerable<Friendship>> GetFriendsAsync(Guid userId);
    Task<IEnumerable<Friendship>> GetPendingRequestsAsync(Guid userId);
}
