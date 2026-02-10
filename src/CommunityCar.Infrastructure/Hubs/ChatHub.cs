using CommunityCar.Infrastructure.Hubs.Base;
using CommunityCar.Infrastructure.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for real-time chat and messaging
/// Handles: Direct messages, typing indicators, read receipts, and online status
/// Inherits from BaseHub for consistent connection management, authorization, and logging
/// </summary>
[Authorize]
public class ChatHub : BaseHub<ChatHub>
{
    public ChatHub(ILogger<ChatHub> logger, IConnectionManager connectionManager) : base(logger, connectionManager)
    {
    }

    #region Chat Room Management

    /// <summary>
    /// Join a specific chat room group
    /// </summary>
    public async Task JoinChatRoom(string chatRoomId)
    {
        var groupName = $"chat_{chatRoomId}";
        await JoinGroup(groupName);
        
        Logger.LogInformation("User {UserId} joined chat room {ChatRoomId}", 
            GetUserId(), chatRoomId);
    }

    /// <summary>
    /// Leave a specific chat room group
    /// </summary>
    public async Task LeaveChatRoom(string chatRoomId)
    {
        var groupName = $"chat_{chatRoomId}";
        await LeaveGroup(groupName);
        
        Logger.LogInformation("User {UserId} left chat room {ChatRoomId}", 
            GetUserId(), chatRoomId);
    }

    /// <summary>
    /// Get count of users in a specific chat room
    /// </summary>
    public int GetChatRoomUserCount(string chatRoomId)
    {
        var groupName = $"chat_{chatRoomId}";
        return ConnectionManager.GetGroupConnectionCount(groupName);
    }

    #endregion

    #region Messaging Methods

    /// <summary>
    /// Send a direct message to a specific user
    /// </summary>
    public async Task SendMessage(Guid receiverId, string message)
    {
        var senderId = GetUserId();
        if (!senderId.HasValue) return;

        var messageData = new
        {
            SenderId = senderId.Value,
            Message = message,
            Timestamp = DateTimeOffset.UtcNow
        };

        // Send to all receiver's connections
        await Clients.Group($"user_{receiverId}").SendCoreAsync("ReceiveMessage", new object[] { messageData });

        // Send confirmation to all sender's connections
        await Clients.Group($"user_{senderId.Value}").SendCoreAsync("MessageSent", new object[] { new
        {
            ReceiverId = receiverId,
            Message = message,
            Timestamp = DateTimeOffset.UtcNow
        } });

        Logger.LogInformation("Message sent from {SenderId} to {ReceiverId}", senderId.Value, receiverId);
    }

    /// <summary>
    /// Mark a message as read
    /// </summary>
    public async Task MarkAsRead(Guid senderId, Guid messageId)
    {
        await Clients.Group($"user_{senderId}").SendCoreAsync("MessageRead", new object[] { new
        {
            MessageId = messageId,
            Timestamp = DateTimeOffset.UtcNow
        } });

        Logger.LogInformation("Message {MessageId} marked as read", messageId);
    }

    /// <summary>
    /// Notify that user is typing
    /// </summary>
    public async Task Typing(Guid receiverId)
    {
        var senderId = GetUserId();
        if (!senderId.HasValue) return;

        await Clients.Group($"user_{receiverId}").SendCoreAsync("UserTyping", new object[] { new
        {
            UserId = senderId.Value,
            Timestamp = DateTimeOffset.UtcNow
        } });
    }

    /// <summary>
    /// Notify that user stopped typing
    /// </summary>
    public async Task StopTyping(Guid receiverId)
    {
        var senderId = GetUserId();
        if (!senderId.HasValue) return;

        await Clients.Group($"user_{receiverId}").SendCoreAsync("UserStoppedTyping", new object[] { new
        {
            UserId = senderId.Value,
            Timestamp = DateTimeOffset.UtcNow
        } });
    }

    #endregion

    #region Connection Lifecycle Overrides

    protected override async Task OnUserConnected(Guid userId, string connectionId)
    {
        Logger.LogInformation("ChatHub: User {UserId} connected with connection {ConnectionId}", 
            userId, connectionId);
        await base.OnUserConnected(userId, connectionId);
    }

    protected override async Task OnUserDisconnected(Guid userId, string connectionId, bool isLastConnection)
    {
        Logger.LogInformation("ChatHub: User {UserId} disconnected. Last connection: {IsLast}", 
            userId, isLastConnection);
        await base.OnUserDisconnected(userId, connectionId, isLastConnection);
    }

    protected override async Task OnUserOnline(Guid userId)
    {
        await Clients.Others.SendCoreAsync("ChatUserOnline", new object[] { new
        {
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow
        } });
        
        Logger.LogInformation("ChatHub: User {UserId} is now online", userId);
    }

    protected override async Task OnUserOffline(Guid userId)
    {
        await Clients.Others.SendCoreAsync("ChatUserOffline", new object[] { new
        {
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow
        } });
        
        Logger.LogInformation("ChatHub: User {UserId} is now offline", userId);
    }

    #endregion
}
