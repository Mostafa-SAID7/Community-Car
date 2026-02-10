namespace CommunityCar.Domain.Interfaces.Community;

/// <summary>
/// Service interface for sending real-time chat and messaging notifications via ChatHub
/// </summary>
public interface IChatHubService
{
    // Direct messaging
    Task SendMessageAsync(Guid senderId, Guid receiverId, string message, Guid messageId);
    Task NotifyMessageReadAsync(Guid senderId, Guid messageId);
    
    // Typing indicators
    Task NotifyTypingAsync(Guid senderId, Guid receiverId);
    Task NotifyStopTypingAsync(Guid senderId, Guid receiverId);
    
    // Chat room messaging
    Task SendChatRoomMessageAsync(string chatRoomId, Guid senderId, string message, object messageData);
    Task NotifyChatRoomTypingAsync(string chatRoomId, Guid userId, string userName);
    Task NotifyChatRoomStopTypingAsync(string chatRoomId, Guid userId);
    
    // Message status
    Task NotifyMessageDeliveredAsync(Guid receiverId, Guid messageId);
    Task NotifyMessageDeletedAsync(Guid receiverId, Guid messageId);
    Task NotifyMessageEditedAsync(Guid receiverId, Guid messageId, string newContent);
}
