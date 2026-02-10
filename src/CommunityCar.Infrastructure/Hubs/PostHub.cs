using CommunityCar.Infrastructure.Hubs.Base;
using CommunityCar.Infrastructure.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for real-time post-related notifications and updates
/// Handles: Post creation, updates, likes, comments, shares, and friend notifications
/// </summary>
[Authorize]
public class PostHub : BaseHub<PostHub>
{
    public PostHub(ILogger<PostHub> logger, IConnectionManager connectionManager) : base(logger, connectionManager)
    {
    }

    #region Group Management

    /// <summary>
    /// Join a specific post thread group
    /// </summary>
    public async Task JoinPostThread(Guid postId)
    {
        var groupName = $"post_{postId}";
        await JoinGroup(groupName);
        Logger.LogInformation("Connection {ConnectionId} joined post thread {PostId}", Context.ConnectionId, postId);
    }

    /// <summary>
    /// Leave a specific post thread group
    /// </summary>
    public async Task LeavePostThread(Guid postId)
    {
        var groupName = $"post_{postId}";
        await LeaveGroup(groupName);
        Logger.LogInformation("Connection {ConnectionId} left post thread {PostId}", Context.ConnectionId, postId);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Get count of users watching a specific post
    /// </summary>
    public int GetPostWatcherCount(Guid postId)
    {
        var groupName = $"post_{postId}";
        return ConnectionManager.GetGroupConnectionCount(groupName);
    }

    #endregion
}
