using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

public class ChatHubService : IChatHubService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<ChatHubService> _logger;

    public ChatHubService(IHubContext<ChatHub> hubContext, ILogger<ChatHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendMessageAsync(Guid senderId, Guid receiverId, string message, Guid messageId)
    {
        try
        {
            var messageData = new
            {
                MessageId = messageId,
                SenderId = senderId,
                Message = message,
                Timestamp = DateTimeOffset.UtcNow
            };

            // Send to receiver
            await _hubContext.Clients.Group($"user_{receiverId}")
                .SendCoreAsync("ReceiveMessage", new object[] { messageData });

            // Send confirmation to sender
            await _hubContext.Clients.Group($"user_{senderId}")
                .SendCoreAsync("MessageSent", new object[] { new
                {
                    MessageId = messageId,
                    ReceiverId = receiverId,
                    Message = message,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Message {MessageId} sent from {SenderId} to {ReceiverId}", 
                messageId, senderId, receiverId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
        }
    }

    public async Task NotifyMessageReadAsync(Guid senderId, Guid messageId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{senderId}")
                .SendCoreAsync("MessageRead", new object[] { new
                {
                    MessageId = messageId,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Message {MessageId} marked as read", messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying message read");
        }
    }

    public async Task NotifyTypingAsync(Guid senderId, Guid receiverId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{receiverId}")
                .SendCoreAsync("UserTyping", new object[] { new
                {
                    UserId = senderId,
                    Timestamp = DateTimeOffset.UtcNow
                } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying typing");
        }
    }

    public async Task NotifyStopTypingAsync(Guid senderId, Guid receiverId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{receiverId}")
                .SendCoreAsync("UserStoppedTyping", new object[] { new
                {
                    UserId = senderId,
                    Timestamp = DateTimeOffset.UtcNow
                } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying stop typing");
        }
    }

    public async Task SendChatRoomMessageAsync(string chatRoomId, Guid senderId, string message, object messageData)
    {
        try
        {
            await _hubContext.Clients.Group($"chat_{chatRoomId}")
                .SendCoreAsync("ReceiveChatRoomMessage", new object[] { new
                {
                    ChatRoomId = chatRoomId,
                    SenderId = senderId,
                    Message = message,
                    Data = messageData,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Message sent to chat room {ChatRoomId} by {SenderId}", 
                chatRoomId, senderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending chat room message");
        }
    }

    public async Task NotifyChatRoomTypingAsync(string chatRoomId, Guid userId, string userName)
    {
        try
        {
            await _hubContext.Clients.Group($"chat_{chatRoomId}")
                .SendCoreAsync("ChatRoomUserTyping", new object[] { new
                {
                    ChatRoomId = chatRoomId,
                    UserId = userId,
                    UserName = userName,
                    Timestamp = DateTimeOffset.UtcNow
                } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying chat room typing");
        }
    }

    public async Task NotifyChatRoomStopTypingAsync(string chatRoomId, Guid userId)
    {
        try
        {
            await _hubContext.Clients.Group($"chat_{chatRoomId}")
                .SendCoreAsync("ChatRoomUserStoppedTyping", new object[] { new
                {
                    ChatRoomId = chatRoomId,
                    UserId = userId,
                    Timestamp = DateTimeOffset.UtcNow
                } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying chat room stop typing");
        }
    }

    public async Task NotifyMessageDeliveredAsync(Guid receiverId, Guid messageId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{receiverId}")
                .SendCoreAsync("MessageDelivered", new object[] { new
                {
                    MessageId = messageId,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Message {MessageId} delivered to {ReceiverId}", messageId, receiverId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying message delivered");
        }
    }

    public async Task NotifyMessageDeletedAsync(Guid receiverId, Guid messageId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{receiverId}")
                .SendCoreAsync("MessageDeleted", new object[] { new
                {
                    MessageId = messageId,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Message {MessageId} deleted notification sent to {ReceiverId}", 
                messageId, receiverId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying message deleted");
        }
    }

    public async Task NotifyMessageEditedAsync(Guid receiverId, Guid messageId, string newContent)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{receiverId}")
                .SendCoreAsync("MessageEdited", new object[] { new
                {
                    MessageId = messageId,
                    NewContent = newContent,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Message {MessageId} edited notification sent to {ReceiverId}", 
                messageId, receiverId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying message edited");
        }
    }
}
