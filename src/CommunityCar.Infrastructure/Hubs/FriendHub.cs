using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CommunityCar.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for real-time friend-related notifications
/// Handles: Friend requests, acceptances, rejections, blocks, suggestions, and status updates
/// </summary>
[Authorize]
public class FriendHub : Hub
{
    private static readonly Dictionary<Guid, string> _userConnections = new();
    private readonly ILogger<FriendHub> _logger;

    public FriendHub(ILogger<FriendHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            _userConnections[userId.Value] = Context.ConnectionId;
            _logger.LogInformation("User {UserId} connected to FriendHub", userId.Value);
            
            // Notify user's friends that they are online
            await Clients.Others.SendAsync("UserOnline", userId.Value);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            _userConnections.Remove(userId.Value);
            _logger.LogInformation("User {UserId} disconnected from FriendHub", userId.Value);
            
            // Notify user's friends that they are offline
            await Clients.Others.SendAsync("UserOffline", userId.Value);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Send friend request notification to target user
    /// </summary>
    public async Task SendFriendRequest(Guid receiverId, string senderName, string senderProfilePicture)
    {
        var senderId = GetUserId();
        if (!senderId.HasValue) return;

        if (_userConnections.TryGetValue(receiverId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveFriendRequest", new
            {
                SenderId = senderId.Value,
                SenderName = senderName,
                SenderProfilePicture = senderProfilePicture,
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Friend request sent from {SenderId} to {ReceiverId}", senderId.Value, receiverId);
        }
    }

    /// <summary>
    /// Notify user that their friend request was accepted
    /// </summary>
    public async Task NotifyFriendRequestAccepted(Guid requesterId, string accepterName, string accepterProfilePicture)
    {
        if (_userConnections.TryGetValue(requesterId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("FriendRequestAccepted", new
            {
                FriendId = GetUserId(),
                FriendName = accepterName,
                FriendProfilePicture = accepterProfilePicture,
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Friend request accepted notification sent to {RequesterId}", requesterId);
        }
    }

    /// <summary>
    /// Notify user that their friend request was rejected
    /// </summary>
    public async Task NotifyFriendRequestRejected(Guid requesterId, string rejecterName)
    {
        if (_userConnections.TryGetValue(requesterId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("FriendRequestRejected", new
            {
                UserId = GetUserId(),
                UserName = rejecterName,
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Friend request rejected notification sent to {RequesterId}", requesterId);
        }
    }

    /// <summary>
    /// Notify user that they have been blocked
    /// </summary>
    public async Task NotifyUserBlocked(Guid blockedUserId)
    {
        if (_userConnections.TryGetValue(blockedUserId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("UserBlocked", new
            {
                BlockedBy = GetUserId(),
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("User blocked notification sent to {BlockedUserId}", blockedUserId);
        }
    }

    /// <summary>
    /// Notify user that they have been unblocked
    /// </summary>
    public async Task NotifyUserUnblocked(Guid unblockedUserId)
    {
        if (_userConnections.TryGetValue(unblockedUserId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("UserUnblocked", new
            {
                UnblockedBy = GetUserId(),
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("User unblocked notification sent to {UnblockedUserId}", unblockedUserId);
        }
    }

    /// <summary>
    /// Notify user about new friend suggestions
    /// </summary>
    public async Task NotifyNewSuggestions(Guid userId, int suggestionCount)
    {
        if (_userConnections.TryGetValue(userId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("NewFriendSuggestions", new
            {
                Count = suggestionCount,
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("New friend suggestions notification sent to {UserId}", userId);
        }
    }

    /// <summary>
    /// Notify user that a friend has updated their profile
    /// </summary>
    public async Task NotifyFriendProfileUpdated(Guid friendId, string friendName, string friendProfilePicture)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        if (_userConnections.TryGetValue(friendId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("FriendProfileUpdated", new
            {
                FriendId = userId.Value,
                FriendName = friendName,
                FriendProfilePicture = friendProfilePicture,
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Friend profile updated notification sent to {FriendId}", friendId);
        }
    }

    /// <summary>
    /// Notify all friends about user status change (online/offline/busy)
    /// </summary>
    public async Task UpdateUserStatus(string status, List<Guid> friendIds)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        foreach (var friendId in friendIds)
        {
            if (_userConnections.TryGetValue(friendId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("FriendStatusChanged", new
                {
                    FriendId = userId.Value,
                    Status = status,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        _logger.LogInformation("User {UserId} status updated to {Status}", userId.Value, status);
    }

    /// <summary>
    /// Notify user that a friend has removed them
    /// </summary>
    public async Task NotifyFriendshipRemoved(Guid userId, string removerName)
    {
        if (_userConnections.TryGetValue(userId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("FriendshipRemoved", new
            {
                RemovedBy = GetUserId(),
                RemovedByName = removerName,
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Friendship removed notification sent to {UserId}", userId);
        }
    }

    /// <summary>
    /// Broadcast to all connected users (for system-wide announcements)
    /// </summary>
    public async Task BroadcastToAll(string message, string type)
    {
        await Clients.All.SendAsync("SystemAnnouncement", new
        {
            Message = message,
            Type = type,
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("System announcement broadcasted: {Message}", message);
    }

    /// <summary>
    /// Check if a user is currently online
    /// </summary>
    public static bool IsUserOnline(Guid userId)
    {
        return _userConnections.ContainsKey(userId);
    }

    /// <summary>
    /// Get connection ID for a specific user
    /// </summary>
    public static string? GetConnectionId(Guid userId)
    {
        return _userConnections.TryGetValue(userId, out var connectionId) ? connectionId : null;
    }

    private Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }
}
