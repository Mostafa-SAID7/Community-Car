namespace CommunityCar.Domain.Interfaces.Community;

/// <summary>
/// Service interface for sending real-time friend-related notifications via FriendHub
/// </summary>
public interface IFriendHubService
{
    // Friend request notifications
    Task NotifyFriendRequestAsync(Guid receiverId, Guid senderId, string senderName, string senderProfilePicture);
    Task NotifyFriendRequestAcceptedAsync(Guid requesterId, Guid accepterId, string accepterName, string accepterProfilePicture);
    Task NotifyFriendRequestRejectedAsync(Guid requesterId, Guid rejecterId, string rejecterName);
    
    // Block/Unblock notifications
    Task NotifyUserBlockedAsync(Guid blockedUserId, Guid blockerId);
    Task NotifyUserUnblockedAsync(Guid unblockedUserId, Guid unblockerId);
    
    // Friendship management
    Task NotifyFriendshipRemovedAsync(Guid userId, Guid removerId, string removerName);
    Task NotifyNewSuggestionsAsync(Guid userId, int suggestionCount);
    
    // Profile updates
    Task NotifyFriendProfileUpdatedAsync(Guid friendId, Guid userId, string userName, string userProfilePicture);
    
    // Status updates
    Task UpdateUserStatusAsync(Guid userId, string status, List<Guid> friendIds);
    
    // System announcements
    Task BroadcastAnnouncementAsync(string message, string type);
}
