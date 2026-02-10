using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Infrastructure.Hubs;
using CommunityCar.Infrastructure.Services.Common;
using Microsoft.AspNetCore.SignalR;

namespace CommunityCar.Infrastructure.Services.Common;

/// <summary>
/// Service for sending real-time notifications via centralized SignalR Hub
/// </summary>
public class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<GenericHub> _hubContext;
    private readonly IConnectionManager _connectionManager;

    public NotificationHubService(IHubContext<GenericHub> hubContext, IConnectionManager connectionManager)
    {
        _hubContext = hubContext;
        _connectionManager = connectionManager;
    }

    public async Task SendToUserAsync(Guid userId, string eventName, object data)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendCoreAsync(eventName, new object[] { data });
    }

    public async Task SendToUsersAsync(List<Guid> userIds, string eventName, object data)
    {
        var groups = userIds.Select(id => $"user_{id}").ToList();
        if (groups.Any())
        {
            await _hubContext.Clients.Groups(groups).SendCoreAsync(eventName, new object[] { data });
        }
    }

    public async Task SendToGroupAsync(string groupName, string eventName, object data)
    {
        await _hubContext.Clients.Group(groupName).SendCoreAsync(eventName, new object[] { data });
    }

    public async Task BroadcastToAllAsync(string eventName, object data)
    {
        await _hubContext.Clients.All.SendCoreAsync(eventName, new object[] { data });
    }

    public async Task SendToAllExceptAsync(List<Guid> excludeUserIds, string eventName, object data)
    {
        var excludeConnectionIds = new List<string>();
        foreach (var userId in excludeUserIds)
        {
            excludeConnectionIds.AddRange(_connectionManager.GetUserConnections(userId));
        }

        await _hubContext.Clients.AllExcept(excludeConnectionIds).SendCoreAsync(eventName, new object[] { data });
    }

    // Friend-specific events
    public Task NotifyFriendRequestAsync(Guid receiverId, object data) =>
        SendToUserAsync(receiverId, "ReceiveFriendRequest", data);

    public Task NotifyFriendRequestAcceptedAsync(Guid requesterId, object data) =>
        SendToUserAsync(requesterId, "FriendRequestAccepted", data);

    public Task NotifyFriendRequestRejectedAsync(Guid requesterId, object data) =>
        SendToUserAsync(requesterId, "FriendRequestRejected", data);

    // Post-specific events
    public Task NotifyNewPostAsync(List<Guid> followerIds, object data) =>
        SendToUsersAsync(followerIds, "NewPost", data);

    public Task NotifyPostCommentAsync(Guid postAuthorId, object data) =>
        SendToUserAsync(postAuthorId, "NewPostComment", data);

    public Task NotifyPostLikeAsync(Guid postAuthorId, object data) =>
        SendToUserAsync(postAuthorId, "PostLiked", data);

    // Review-specific events
    public Task NotifyNewReviewAsync(Guid entityOwnerId, object data) =>
        SendToUserAsync(entityOwnerId, "NewReview", data);

    public Task NotifyReviewReactionAsync(Guid reviewerId, object data) =>
        SendToUserAsync(reviewerId, "ReviewReaction", data);

    // Question-specific events
    public Task NotifyNewAnswerAsync(Guid questionAuthorId, object data) =>
        SendToUserAsync(questionAuthorId, "NewAnswer", data);

    public Task NotifyQuestionVoteAsync(Guid questionAuthorId, object data) =>
        SendToUserAsync(questionAuthorId, "QuestionVoted", data);

    // Chat-specific events
    public Task SendChatMessageAsync(string chatRoomId, object data) =>
        SendToGroupAsync($"chat_{chatRoomId}", "ReceiveMessage", data);

    public Task NotifyTypingAsync(string chatRoomId, object data) =>
        SendToGroupAsync($"chat_{chatRoomId}", "UserTyping", data);

    // General notification
    public Task SendNotificationAsync(Guid userId, object data) =>
        SendToUserAsync(userId, "NewNotification", data);

    // Event-specific notifications
    public Task NotifyEventUpdateAsync(List<Guid> attendeeIds, object data) =>
        SendToUsersAsync(attendeeIds, "EventUpdated", data);

    public Task NotifyEventReminderAsync(List<Guid> attendeeIds, object data) =>
        SendToUsersAsync(attendeeIds, "EventReminder", data);

    // Utility methods
    public bool IsUserOnline(Guid userId) => _connectionManager.IsUserOnline(userId);

    public List<string> GetUserConnectionIds(Guid userId) => _connectionManager.GetUserConnections(userId);

    public int GetOnlineUserCount() => _connectionManager.GetOnlineUserCount();

    public List<Guid> GetOnlineUserIds() => _connectionManager.GetOnlineUserIds();
}
