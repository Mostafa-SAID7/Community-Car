namespace CommunityCar.Domain.Interfaces.Communications;

/// <summary>
/// Centralized service for sending real-time notifications through SignalR
/// Provides a unified interface for all Hub communications
/// </summary>
public interface IHubNotificationService
{
    /// <summary>
    /// Send notification to a specific user
    /// </summary>
    Task NotifyUserAsync<T>(Guid userId, string eventType, T data);
    
    /// <summary>
    /// Send notification to multiple users
    /// </summary>
    Task NotifyUsersAsync<T>(List<Guid> userIds, string eventType, T data);
    
    /// <summary>
    /// Send notification to a specific group
    /// </summary>
    Task NotifyGroupAsync<T>(string groupName, string eventType, T data);
    
    /// <summary>
    /// Broadcast notification to all connected users
    /// </summary>
    Task BroadcastAsync<T>(string eventType, T data);
    
    /// <summary>
    /// Send notification to all users except the specified ones
    /// </summary>
    Task NotifyAllExceptAsync<T>(List<Guid> excludedUserIds, string eventType, T data);
    
    /// <summary>
    /// Check if a user is currently online
    /// </summary>
    bool IsUserOnline(Guid userId);
    
    /// <summary>
    /// Get count of online users
    /// </summary>
    int GetOnlineUserCount();
}
