using CommunityCar.Infrastructure.Hubs.Base;
using CommunityCar.Infrastructure.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Hubs;

/// <summary>
/// Centralized SignalR Hub for all real-time community communications
/// Handles: Friends, Posts, Chat, Notifications, Questions, Reviews, Groups, Events
/// Inherits from BaseHub for consistent connection management, authorization, and logging
/// </summary>
[Authorize]
public class CommunityHub : BaseHub<CommunityHub>
{
    public CommunityHub(ILogger<CommunityHub> logger, IConnectionManager connectionManager) : base(logger, connectionManager)
    {
    }

    #region Group Management Extensions

    /// <summary>
    /// Join a community group
    /// </summary>
    public async Task JoinCommunityGroup(string groupId)
    {
        await JoinGroup($"group_{groupId}");
        Logger.LogInformation("User {UserId} joined community group {GroupId}", GetUserId(), groupId);
    }

    /// <summary>
    /// Leave a community group
    /// </summary>
    public async Task LeaveCommunityGroup(string groupId)
    {
        await LeaveGroup($"group_{groupId}");
        Logger.LogInformation("User {UserId} left community group {GroupId}", GetUserId(), groupId);
    }

    /// <summary>
    /// Join an event group
    /// </summary>
    public async Task JoinEventGroup(string eventId)
    {
        await JoinGroup($"event_{eventId}");
        Logger.LogInformation("User {UserId} joined event group {EventId}", GetUserId(), eventId);
    }

    /// <summary>
    /// Leave an event group
    /// </summary>
    public async Task LeaveEventGroup(string eventId)
    {
        await LeaveGroup($"event_{eventId}");
        Logger.LogInformation("User {UserId} left event group {EventId}", GetUserId(), eventId);
    }

    #endregion

    #region Connection Lifecycle Overrides

    protected override async Task OnUserConnected(Guid userId, string connectionId)
    {
        Logger.LogInformation("CommunityHub: User {UserId} connected with connection {ConnectionId}", 
            userId, connectionId);
        await base.OnUserConnected(userId, connectionId);
    }

    protected override async Task OnUserDisconnected(Guid userId, string connectionId, bool isLastConnection)
    {
        Logger.LogInformation("CommunityHub: User {UserId} disconnected. Last connection: {IsLast}", 
            userId, isLastConnection);
        await base.OnUserDisconnected(userId, connectionId, isLastConnection);
    }

    protected override async Task OnUserOnline(Guid userId)
    {
        await Clients.Others.SendCoreAsync("CommunityUserOnline", new object[] { new
        {
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow
        } });
        
        Logger.LogInformation("CommunityHub: User {UserId} is now online", userId);
    }

    protected override async Task OnUserOffline(Guid userId)
    {
        await Clients.Others.SendCoreAsync("CommunityUserOffline", new object[] { new
        {
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow
        } });
        
        Logger.LogInformation("CommunityHub: User {UserId} is now offline", userId);
    }

    #endregion
}
