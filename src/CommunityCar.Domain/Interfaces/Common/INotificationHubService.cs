namespace CommunityCar.Domain.Interfaces.Common;

/// <summary>
/// Service interface for sending real-time notifications via SignalR
/// </summary>
public interface INotificationHubService
{
    // User-specific notifications
    Task SendToUserAsync(Guid userId, string eventName, object data);
    Task SendToUsersAsync(List<Guid> userIds, string eventName, object data);
    
    // Group notifications
    Task SendToGroupAsync(string groupName, string eventName, object data);
    
    // Broadcast notifications
    Task BroadcastToAllAsync(string eventName, object data);
    Task SendToAllExceptAsync(List<Guid> excludeUserIds, string eventName, object data);
    
    // Friend-specific events
    Task NotifyFriendRequestAsync(Guid receiverId, object data);
    Task NotifyFriendRequestAcceptedAsync(Guid requesterId, object data);
    Task NotifyFriendRequestRejectedAsync(Guid requesterId, object data);
    
    // Post-specific events
    Task NotifyNewPostAsync(List<Guid> followerIds, object data);
    Task NotifyPostCommentAsync(Guid postAuthorId, object data);
    Task NotifyPostLikeAsync(Guid postAuthorId, object data);
    
    // Review-specific events
    Task NotifyNewReviewAsync(Guid entityOwnerId, object data);
    Task NotifyReviewReactionAsync(Guid reviewerId, object data);
    
    // Question-specific events
    Task NotifyNewAnswerAsync(Guid questionAuthorId, object data);
    Task NotifyQuestionVoteAsync(Guid questionAuthorId, object data);
    
    // Chat-specific events
    Task SendChatMessageAsync(string chatRoomId, object data);
    Task NotifyTypingAsync(string chatRoomId, object data);
    
    // General notification
    Task SendNotificationAsync(Guid userId, object data);
    
    // Event-specific notifications
    Task NotifyEventUpdateAsync(List<Guid> attendeeIds, object data);
    Task NotifyEventReminderAsync(List<Guid> attendeeIds, object data);
    
    // Utility methods
    bool IsUserOnline(Guid userId);
    List<string> GetUserConnectionIds(Guid userId);
    int GetOnlineUserCount();
    List<Guid> GetOnlineUserIds();
}
