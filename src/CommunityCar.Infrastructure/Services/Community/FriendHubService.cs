using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

public class FriendHubService : IFriendHubService
{
    private readonly IHubContext<FriendHub> _hubContext;
    private readonly ILogger<FriendHubService> _logger;

    public FriendHubService(IHubContext<FriendHub> hubContext, ILogger<FriendHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyFriendRequestAsync(Guid receiverId, Guid senderId, string senderName, string senderProfilePicture)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{receiverId}")
                .SendCoreAsync("ReceiveFriendRequest", new object[] { new
                {
                    SenderId = senderId,
                    SenderName = senderName,
                    SenderProfilePicture = senderProfilePicture,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Friend request notification sent from {SenderId} to {ReceiverId}", 
                senderId, receiverId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friend request notification");
        }
    }

    public async Task NotifyFriendRequestAcceptedAsync(Guid requesterId, Guid accepterId, 
        string accepterName, string accepterProfilePicture)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{requesterId}")
                .SendCoreAsync("FriendRequestAccepted", new object[] { new
                {
                    FriendId = accepterId,
                    FriendName = accepterName,
                    FriendProfilePicture = accepterProfilePicture,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Friend request accepted notification sent to {RequesterId}", requesterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friend request accepted notification");
        }
    }

    public async Task NotifyFriendRequestRejectedAsync(Guid requesterId, Guid rejecterId, string rejecterName)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{requesterId}")
                .SendCoreAsync("FriendRequestRejected", new object[] { new
                {
                    UserId = rejecterId,
                    UserName = rejecterName,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Friend request rejected notification sent to {RequesterId}", requesterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friend request rejected notification");
        }
    }

    public async Task NotifyUserBlockedAsync(Guid blockedUserId, Guid blockerId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{blockedUserId}")
                .SendCoreAsync("UserBlocked", new object[] { new
                {
                    BlockedBy = blockerId,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("User blocked notification sent to {BlockedUserId}", blockedUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending user blocked notification");
        }
    }

    public async Task NotifyUserUnblockedAsync(Guid unblockedUserId, Guid unblockerId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{unblockedUserId}")
                .SendCoreAsync("UserUnblocked", new object[] { new
                {
                    UnblockedBy = unblockerId,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("User unblocked notification sent to {UnblockedUserId}", unblockedUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending user unblocked notification");
        }
    }

    public async Task NotifyFriendshipRemovedAsync(Guid userId, Guid removerId, string removerName)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{userId}")
                .SendCoreAsync("FriendshipRemoved", new object[] { new
                {
                    RemovedBy = removerId,
                    RemovedByName = removerName,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Friendship removed notification sent to {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friendship removed notification");
        }
    }

    public async Task NotifyNewSuggestionsAsync(Guid userId, int suggestionCount)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{userId}")
                .SendCoreAsync("NewFriendSuggestions", new object[] { new
                {
                    Count = suggestionCount,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("New friend suggestions notification sent to {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending new suggestions notification");
        }
    }

    public async Task NotifyFriendProfileUpdatedAsync(Guid friendId, Guid userId, 
        string userName, string userProfilePicture)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{friendId}")
                .SendCoreAsync("FriendProfileUpdated", new object[] { new
                {
                    FriendId = userId,
                    FriendName = userName,
                    FriendProfilePicture = userProfilePicture,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Friend profile updated notification sent to {FriendId}", friendId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friend profile updated notification");
        }
    }

    public async Task UpdateUserStatusAsync(Guid userId, string status, List<Guid> friendIds)
    {
        try
        {
            var statusData = new
            {
                FriendId = userId,
                Status = status,
                Timestamp = DateTimeOffset.UtcNow
            };

            foreach (var friendId in friendIds)
            {
                await _hubContext.Clients.Group($"user_{friendId}")
                    .SendCoreAsync("FriendStatusChanged", new object[] { statusData });
            }

            _logger.LogInformation("User {UserId} status updated to {Status} for {Count} friends", 
                userId, status, friendIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status");
        }
    }

    public async Task BroadcastAnnouncementAsync(string message, string type)
    {
        try
        {
            await _hubContext.Clients.All.SendCoreAsync("SystemAnnouncement", new object[] { new
            {
                Message = message,
                Type = type,
                Timestamp = DateTimeOffset.UtcNow
            } });

            _logger.LogInformation("System announcement broadcasted: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting system announcement");
        }
    }
}
