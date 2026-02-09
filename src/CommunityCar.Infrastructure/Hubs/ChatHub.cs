using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace CommunityCar.Infrastructure.Hubs;

[Authorize]
public class ChatHub : Hub
{
    // Track how many connections each user has
    private static readonly ConcurrentDictionary<Guid, int> _userConnections = new();

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            // Join a group named after the user's ID to receive messages across all tabs
            await Groups.AddToGroupAsync(Context.ConnectionId, userId.Value.ToString());

            _userConnections.AddOrUpdate(userId.Value, 1, (_, count) => count + 1);
            
            // Notify others that user is online if this is their first connection
            if (_userConnections[userId.Value] == 1)
            {
                await Clients.Others.SendAsync("UserOnline", userId.Value);
            }
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.Value.ToString());

            if (_userConnections.TryGetValue(userId.Value, out var count))
            {
                if (count <= 1)
                {
                    _userConnections.TryRemove(userId.Value, out _);
                    // Notify others that user is offline if this was their last connection
                    await Clients.Others.SendAsync("UserOffline", userId.Value);
                }
                else
                {
                    _userConnections.TryUpdate(userId.Value, count - 1, count);
                }
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Guid receiverId, string message)
    {
        var senderId = GetUserId();
        if (!senderId.HasValue) return;

        // Send to ALL connections in the receiver's group
        await Clients.Group(receiverId.ToString()).SendAsync("ReceiveMessage", senderId.Value, message, DateTimeOffset.UtcNow);

        // Also send back to sender's group (not just caller) so all sender's tabs stay synced
        await Clients.Group(senderId.Value.ToString()).SendAsync("MessageSent", receiverId, message, DateTimeOffset.UtcNow);
    }

    public async Task MarkAsRead(Guid senderId, Guid messageId)
    {
        // Notify sender that message was read
        await Clients.Group(senderId.ToString()).SendAsync("MessageRead", messageId);
    }

    public async Task Typing(Guid receiverId)
    {
        var senderId = GetUserId();
        if (!senderId.HasValue) return;

        // Notify receiver's group that user is typing
        await Clients.Group(receiverId.ToString()).SendAsync("UserTyping", senderId.Value);
    }

    public async Task StopTyping(Guid receiverId)
    {
        var senderId = GetUserId();
        if (!senderId.HasValue) return;

        // Notify receiver's group that user stopped typing
        await Clients.Group(receiverId.ToString()).SendAsync("UserStoppedTyping", senderId.Value);
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
