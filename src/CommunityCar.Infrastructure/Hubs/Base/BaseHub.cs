using CommunityCar.Infrastructure.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CommunityCar.Infrastructure.Hubs.Base;

/// <summary>
/// Base Hub with common functionality for all SignalR hubs
/// Provides: Authorization, Connection Management (via ConnectionManager), Group Handling, Logging
/// </summary>
[Authorize]
public abstract class BaseHub<THub> : Hub where THub : Hub
{
    protected readonly ILogger<THub> Logger;
    protected readonly IConnectionManager ConnectionManager;

    protected BaseHub(ILogger<THub> logger, IConnectionManager connectionManager)
    {
        Logger = logger;
        ConnectionManager = connectionManager;
    }

    #region Connection Lifecycle

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            var isFirstConnection = !ConnectionManager.IsUserOnline(userId.Value);
            
            ConnectionManager.OnConnected(userId.Value, Context.ConnectionId);

            // Join user's personal group for targeted notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Value}");

            Logger.LogInformation(
                "{HubName}: User {UserId} connected with ConnectionId {ConnectionId}. Total connections: {ConnectionCount}",
                typeof(THub).Name, userId.Value, Context.ConnectionId, ConnectionManager.GetUserConnections(userId.Value).Count);

            // Notify if this is the first connection (user came online)
            if (isFirstConnection)
            {
                await OnUserOnline(userId.Value);
            }

            await OnUserConnected(userId.Value, Context.ConnectionId);
        }
        else
        {
            Logger.LogWarning("{HubName}: Connection {ConnectionId} attempted without valid user ID",
                typeof(THub).Name, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            ConnectionManager.OnDisconnected(userId.Value, Context.ConnectionId);
            
            var isLastConnection = !ConnectionManager.IsUserOnline(userId.Value);

            // If no more connections, notify offline
            if (isLastConnection)
            {
                await OnUserOffline(userId.Value);
            }

            Logger.LogInformation(
                "{HubName}: User {UserId} disconnected. Remaining connections: {ConnectionCount}. Exception: {Exception}",
                typeof(THub).Name, userId.Value, ConnectionManager.GetUserConnections(userId.Value).Count, exception?.Message ?? "None");

            await OnUserDisconnected(userId.Value, Context.ConnectionId, isLastConnection);
        }

        await base.OnDisconnectedAsync(exception);
    }

    #endregion

    #region Group Management

    /// <summary>
    /// Join a specific group (e.g., post comments, question thread, chat room)
    /// </summary>
    public virtual async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        ConnectionManager.AddToGroup(groupName, Context.ConnectionId);

        Logger.LogInformation("{HubName}: Connection {ConnectionId} joined group {GroupName}",
            typeof(THub).Name, Context.ConnectionId, groupName);

        await OnGroupJoined(groupName, Context.ConnectionId);
    }

    /// <summary>
    /// Leave a specific group
    /// </summary>
    public virtual async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        ConnectionManager.RemoveFromGroup(groupName, Context.ConnectionId);

        Logger.LogInformation("{HubName}: Connection {ConnectionId} left group {GroupName}",
            typeof(THub).Name, Context.ConnectionId, groupName);

        await OnGroupLeft(groupName, Context.ConnectionId);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get the current user's ID from claims
    /// </summary>
    protected Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }

    /// <summary>
    /// Get the current user's name from claims
    /// </summary>
    protected string? GetUserName()
    {
        return Context.User?.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Get the current user's email from claims
    /// </summary>
    protected string? GetUserEmail()
    {
        return Context.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    #endregion

    #region Virtual Methods for Derived Classes

    /// <summary>
    /// Called when a user connects (every connection)
    /// </summary>
    protected virtual Task OnUserConnected(Guid userId, string connectionId)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when a user disconnects (every disconnection)
    /// </summary>
    protected virtual Task OnUserDisconnected(Guid userId, string connectionId, bool isLastConnection)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when a user comes online (first connection)
    /// </summary>
    protected virtual Task OnUserOnline(Guid userId)
    {
        return Clients.Others.SendCoreAsync("UserOnline", new object[] { new
        {
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow
        } });
    }

    /// <summary>
    /// Called when a user goes offline (last connection closed)
    /// </summary>
    protected virtual Task OnUserOffline(Guid userId)
    {
        return Clients.Others.SendCoreAsync("UserOffline", new object[] { new
        {
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow
        } });
    }

    /// <summary>
    /// Called when a connection joins a group
    /// </summary>
    protected virtual Task OnGroupJoined(string groupName, string connectionId)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when a connection leaves a group
    /// </summary>
    protected virtual Task OnGroupLeft(string groupName, string connectionId)
    {
        return Task.CompletedTask;
    }

    #endregion
}
