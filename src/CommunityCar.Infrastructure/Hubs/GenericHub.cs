using CommunityCar.Infrastructure.Hubs.Base;
using CommunityCar.Infrastructure.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Hubs;

/// <summary>
/// Generic centralized SignalR Hub for all real-time communications
/// Handles: Friends, Posts, Reviews, Questions, Notifications, Chat, Events, etc.
/// Inherits from BaseHub for consistent connection management, authorization, and logging
/// Provides backward compatibility with existing notification patterns
/// </summary>
[Authorize]
public class GenericHub : BaseHub<GenericHub>
{
    public GenericHub(ILogger<GenericHub> logger, IConnectionManager connectionManager) : base(logger, connectionManager)
    {
    }

    #region Generic Notification Methods

    /// <summary>
    /// Send notification to specific user
    /// </summary>
    public async Task SendToUser(Guid userId, string eventName, object data)
    {
        await Clients.Group($"user_{userId}").SendCoreAsync(eventName, new object[] { data });
        Logger.LogInformation("Event {EventName} sent to user {UserId} via group", eventName, userId);
    }

    /// <summary>
    /// Send notification to multiple users
    /// </summary>
    public async Task SendToUsers(List<Guid> userIds, string eventName, object data)
    {
        var groups = userIds.Select(id => $"user_{id}").ToList();
        if (groups.Any())
        {
            await Clients.Groups(groups).SendCoreAsync(eventName, new object[] { data });
            Logger.LogInformation("Event {EventName} sent to {Count} users via groups", eventName, userIds.Count);
        }
    }

    /// <summary>
    /// Send notification to a group
    /// </summary>
    public async Task SendToGroup(string groupName, string eventName, object data)
    {
        await Clients.Group(groupName).SendCoreAsync(eventName, new object[] { data });
        Logger.LogInformation("Event {EventName} sent to group {GroupName}", eventName, groupName);
    }

    /// <summary>
    /// Broadcast to all connected users
    /// </summary>
    public async Task BroadcastToAll(string eventName, object data)
    {
        await Clients.All.SendCoreAsync(eventName, new object[] { data });
        Logger.LogInformation("Event {EventName} broadcasted to all users", eventName);
    }

    /// <summary>
    /// Send to all except specific users
    /// </summary>
    public async Task SendToAllExcept(List<Guid> excludeUserIds, string eventName, object data)
    {
        var excludeConnectionIds = new List<string>();
        
        foreach (var userId in excludeUserIds)
        {
            excludeConnectionIds.AddRange(ConnectionManager.GetUserConnections(userId));
        }

        await Clients.AllExcept(excludeConnectionIds).SendCoreAsync(eventName, new object[] { data });
        Logger.LogInformation("Event {EventName} sent to all except {Count} users", eventName, excludeUserIds.Count);
    }

    #endregion

    #region Specific Event Handlers (Backward Compatibility)

    // Friend Events
    public Task NotifyFriendRequest(Guid receiverId, object data) => 
        SendToUser(receiverId, "ReceiveFriendRequest", data);

    public Task NotifyFriendRequestAccepted(Guid requesterId, object data) => 
        SendToUser(requesterId, "FriendRequestAccepted", data);

    public Task NotifyFriendRequestRejected(Guid requesterId, object data) => 
        SendToUser(requesterId, "FriendRequestRejected", data);

    // Post Events
    public Task NotifyNewPost(List<Guid> followerIds, object data) => 
        SendToUsers(followerIds, "NewPost", data);

    public Task NotifyPostComment(Guid postAuthorId, object data) => 
        SendToUser(postAuthorId, "NewPostComment", data);

    public Task NotifyPostLike(Guid postAuthorId, object data) => 
        SendToUser(postAuthorId, "PostLiked", data);

    // Review Events
    public Task NotifyNewReview(Guid entityOwnerId, object data) => 
        SendToUser(entityOwnerId, "NewReview", data);

    public Task NotifyReviewReaction(Guid reviewerId, object data) => 
        SendToUser(reviewerId, "ReviewReaction", data);

    // Question Events
    public Task NotifyNewAnswer(Guid questionAuthorId, object data) => 
        SendToUser(questionAuthorId, "NewAnswer", data);

    public Task NotifyQuestionVote(Guid questionAuthorId, object data) => 
        SendToUser(questionAuthorId, "QuestionVoted", data);

    // Chat Events
    public Task SendChatMessage(string chatRoomId, object data) => 
        SendToGroup($"chat_{chatRoomId}", "ReceiveMessage", data);

    public Task NotifyTyping(string chatRoomId, object data) => 
        SendToGroup($"chat_{chatRoomId}", "UserTyping", data);

    // Notification Events
    public Task SendNotification(Guid userId, object data) => 
        SendToUser(userId, "NewNotification", data);

    // Event (Community Event) Notifications
    public Task NotifyEventUpdate(List<Guid> attendeeIds, object data) => 
        SendToUsers(attendeeIds, "EventUpdated", data);

    public Task NotifyEventReminder(List<Guid> attendeeIds, object data) => 
        SendToUsers(attendeeIds, "EventReminder", data);

    #endregion

    #region Connection Lifecycle Overrides

    protected override async Task OnUserConnected(Guid userId, string connectionId)
    {
        Logger.LogInformation("GenericHub: User {UserId} connected with connection {ConnectionId}", 
            userId, connectionId);
        await base.OnUserConnected(userId, connectionId);
    }

    protected override async Task OnUserDisconnected(Guid userId, string connectionId, bool isLastConnection)
    {
        Logger.LogInformation("GenericHub: User {UserId} disconnected. Last connection: {IsLast}", 
            userId, isLastConnection);
        await base.OnUserDisconnected(userId, connectionId, isLastConnection);
    }

    protected override async Task OnUserOnline(Guid userId)
    {
        await Clients.Others.SendCoreAsync("UserOnline", new object[] { new
        {
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow
        } });
        
        Logger.LogInformation("GenericHub: User {UserId} is now online", userId);
    }

    protected override async Task OnUserOffline(Guid userId)
    {
        await Clients.Others.SendCoreAsync("UserOffline", new object[] { new
        {
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow
        } });
        
        Logger.LogInformation("GenericHub: User {UserId} is now offline", userId);
    }

    #endregion
}
