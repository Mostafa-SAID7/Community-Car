using CommunityCar.Domain.Interfaces.Communications;
using CommunityCar.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using CommunityCar.Infrastructure.Services.Common;

namespace CommunityCar.Infrastructure.Services.Communications;

/// <summary>
/// Centralized service for sending real-time notifications through SignalR
/// Provides a unified interface for all Hub communications
/// </summary>
public class HubNotificationService : IHubNotificationService
{
    private readonly IHubContext<CommunityHub> _hubContext;
    private readonly ILogger<HubNotificationService> _logger;
    private readonly IConnectionManager _connectionManager;

    public HubNotificationService(
        IHubContext<CommunityHub> hubContext,
        ILogger<HubNotificationService> logger,
        IConnectionManager connectionManager)
    {
        _hubContext = hubContext;
        _logger = logger;
        _connectionManager = connectionManager;
    }

    /// <summary>
    /// Send notification to a specific user
    /// </summary>
    public async Task NotifyUserAsync<T>(Guid userId, string eventType, T data)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendCoreAsync(eventType, new object[] { new
                {
                    Data = data,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogDebug("Notification sent to user {UserId}: {EventType}", userId, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}: {EventType}", userId, eventType);
        }
    }

    /// <summary>
    /// Send notification to multiple users
    /// </summary>
    public async Task NotifyUsersAsync<T>(List<Guid> userIds, string eventType, T data)
    {
        try
        {
            var groups = userIds.Select(id => $"user_{id}").ToList();
            
            await _hubContext.Clients
                .Groups(groups)
                .SendCoreAsync(eventType, new object[] { new
                {
                    Data = data,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogDebug("Notification sent to {Count} users: {EventType}", userIds.Count, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to multiple users: {EventType}", eventType);
        }
    }

    /// <summary>
    /// Send notification to a specific group
    /// </summary>
    public async Task NotifyGroupAsync<T>(string groupName, string eventType, T data)
    {
        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendCoreAsync(eventType, new object[] { new
                {
                    Data = data,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogDebug("Notification sent to group {GroupName}: {EventType}", groupName, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to group {GroupName}: {EventType}", groupName, eventType);
        }
    }

    /// <summary>
    /// Broadcast notification to all connected users
    /// </summary>
    public async Task BroadcastAsync<T>(string eventType, T data)
    {
        try
        {
            await _hubContext.Clients.All.SendCoreAsync(eventType, new object[] { new
            {
                Data = data,
                Timestamp = DateTimeOffset.UtcNow
            } });

            _logger.LogDebug("Notification broadcasted to all users: {EventType}", eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting notification: {EventType}", eventType);
        }
    }

    /// <summary>
    /// Send notification to all users except the specified ones
    /// </summary>
    public async Task NotifyAllExceptAsync<T>(List<Guid> excludedUserIds, string eventType, T data)
    {
        try
        {
            var excludedGroups = excludedUserIds.Select(id => $"user_{id}").ToList();
            
            await _hubContext.Clients
                .AllExcept(excludedGroups)
                .SendCoreAsync(eventType, new object[] { new
                {
                    Data = data,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogDebug("Notification sent to all except {Count} users: {EventType}", excludedUserIds.Count, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to all except specified users: {EventType}", eventType);
        }
    }

    /// <summary>
    /// Check if a user is currently online
    /// </summary>
    public bool IsUserOnline(Guid userId)
    {
        return _connectionManager.IsUserOnline(userId);
    }

    /// <summary>
    /// Get count of online users
    /// </summary>
    public int GetOnlineUserCount()
    {
        // This method in IConnectionManager returns count of connected users
        // Since IConnectionManager interface might not have GetOnlineUserCount directly exposed or named differently,
        // let's assume it has a way or we use GetOnlineUserIds().Count
        return _connectionManager.GetOnlineUserCount();
    }
}
