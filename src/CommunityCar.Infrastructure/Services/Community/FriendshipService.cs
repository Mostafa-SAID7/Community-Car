using CommunityCar.Domain.Entities.Community.friends;
using CommunityCar.Domain.Enums.Community.friends;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Infrastructure.Repos.Common;
using CommunityCar.Infrastructure.Uow.Common;

namespace CommunityCar.Infrastructure.Services.Community;

public class FriendshipService : IFriendshipService
{
    private readonly IRepository<Friendship> _friendshipRepository;
    private readonly IUnitOfWork _uow;

    public FriendshipService(
        IRepository<Friendship> friendshipRepository, 
        IUnitOfWork uow)
    {
        _friendshipRepository = friendshipRepository;
        _uow = uow;
    }

    public async Task SendRequestAsync(Guid userId, Guid friendId)
    {
        if (userId == friendId) throw new InvalidOperationException("Cannot friend yourself.");

        var existing = await _friendshipRepository.FirstOrDefaultAsync(f => 
            (f.UserId == userId && f.FriendId == friendId) || 
            (f.UserId == friendId && f.FriendId == userId));

        if (existing != null) return;

        var friendship = new Friendship(userId, friendId);
        await _friendshipRepository.AddAsync(friendship);
        await _uow.SaveChangesAsync();
    }

    public async Task AcceptRequestAsync(Guid userId, Guid friendId)
    {
        var friendship = await _friendshipRepository.FirstOrDefaultAsync(f => 
            f.UserId == friendId && f.FriendId == userId && f.Status == FriendshipStatus.Pending);

        if (friendship == null) return;

        friendship.Accept();
        await _uow.SaveChangesAsync();
    }

    public async Task RejectRequestAsync(Guid userId, Guid friendId)
    {
        var friendship = await _friendshipRepository.FirstOrDefaultAsync(f => 
            f.UserId == friendId && f.FriendId == userId && f.Status == FriendshipStatus.Pending);

        if (friendship == null) return;

        _friendshipRepository.Delete(friendship);
        await _uow.SaveChangesAsync();
    }

    public async Task BlockUserAsync(Guid userId, Guid friendId)
    {
        var friendship = await _friendshipRepository.FirstOrDefaultAsync(f => 
            (f.UserId == userId && f.FriendId == friendId) || 
            (f.UserId == friendId && f.FriendId == userId));

        if (friendship == null)
        {
            friendship = new Friendship(userId, friendId);
            await _friendshipRepository.AddAsync(friendship);
        }

        friendship.Block();
        await _uow.SaveChangesAsync();
    }

    public async Task RemoveFriendAsync(Guid userId, Guid friendId)
    {
        var friendship = await _friendshipRepository.FirstOrDefaultAsync(f => 
            (f.UserId == userId && f.FriendId == friendId) || 
            (f.UserId == friendId && f.FriendId == userId));

        if (friendship == null) return;

        _friendshipRepository.Delete(friendship);
        await _uow.SaveChangesAsync();
    }

    public async Task<FriendshipStatus> GetFriendshipStatusAsync(Guid userId, Guid friendId)
    {
        var friendship = await _friendshipRepository.FirstOrDefaultAsync(f => 
            (f.UserId == userId && f.FriendId == friendId) || 
            (f.UserId == friendId && f.FriendId == userId));

        if (friendship == null) return FriendshipStatus.None;
        
        return friendship.Status;
    }

    public async Task<IEnumerable<Friendship>> GetFriendsAsync(Guid userId)
    {
        return await _friendshipRepository.WhereAsync(f => 
            (f.UserId == userId || f.FriendId == userId) && f.Status == FriendshipStatus.Accepted);
    }

    public async Task<IEnumerable<Friendship>> GetPendingRequestsAsync(Guid userId)
    {
        return await _friendshipRepository.WhereAsync(f => 
            f.FriendId == userId && f.Status == FriendshipStatus.Pending);
    }
}
