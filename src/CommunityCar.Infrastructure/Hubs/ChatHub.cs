using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CommunityCar.Infrastructure.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private static readonly Dictionary<Guid, string> _userConnections = new();

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            _userConnections[userId.Value] = Context.ConnectionId;
            
            // Notify others that user is online
            await Clients.Others.SendAsync("UserOnline", userId.Value);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            _userConnections.Remove(userId.Value);
            
            // Notify others that user is offline
            await Clients.Others.SendAsync("UserOffline", userId.Value);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Guid receiverId, string message)
    {
        var senderId = GetUserId();
        if (!senderId.HasValue) return;

        // Send to receiver if online
        if (_userConnections.TryGetValue(receiverId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", senderId.Value, message, DateTimeOffset.UtcNow);
        }

        // Also send back to sender for confirmation
        await Clients.Caller.SendAsync("MessageSent", receiverId, message, DateTimeOffset.UtcNow);
    }

    public async Task MarkAsRead(Guid senderId, Guid messageId)
    {
        // Notify sender that message was read
        if (_userConnections.TryGetValue(senderId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("MessageRead", messageId);
        }
    }

    public async Task Typing(Guid receiverId)
    {
        var senderId = GetUserId();
        if (!senderId.HasValue) return;

        // Notify receiver that user is typing
        if (_userConnections.TryGetValue(receiverId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("UserTyping", senderId.Value);
        }
    }

    public async Task StopTyping(Guid receiverId)
    {
        var senderId = GetUserId();
        if (!senderId.HasValue) return;

        // Notify receiver that user stopped typing
        if (_userConnections.TryGetValue(receiverId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("UserStoppedTyping", senderId.Value);
        }
    }

    public static bool IsUserOnline(Guid userId)
    {
        return _userConnections.ContainsKey(userId);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }
}
