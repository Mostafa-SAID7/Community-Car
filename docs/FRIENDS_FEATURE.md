# Friends Feature with SignalR Real-Time Notifications

## Overview

The Friends feature provides comprehensive friend management with real-time notifications using SignalR. Users can send/receive friend requests, accept/reject requests, block/unblock users, view friend suggestions, and receive instant notifications for all friend-related activities.

## Architecture

### Backend Components

#### 1. FriendHub (SignalR Hub)
**Location**: `src/CommunityCar.Infrastructure/Hubs/FriendHub.cs`

The FriendHub manages real-time WebSocket connections for friend-related notifications.

**Features**:
- Connection tracking (online/offline status)
- Real-time friend request notifications
- Block/unblock notifications
- Friendship removal notifications
- Profile update notifications
- Friend suggestions
- Status updates (online/offline/busy)
- System announcements

**Key Methods**:
- `SendFriendRequest()` - Notify user of incoming friend request
- `NotifyFriendRequestAccepted()` - Notify requester of acceptance
- `NotifyFriendRequestRejected()` - Notify requester of rejection
- `NotifyUserBlocked()` - Notify user they've been blocked
- `NotifyUserUnblocked()` - Notify user they've been unblocked
- `NotifyFriendshipRemoved()` - Notify user friendship was removed
- `UpdateUserStatus()` - Broadcast status changes to friends
- `NotifyFriendProfileUpdated()` - Notify friends of profile updates
- `NotifyNewSuggestions()` - Notify user of new friend suggestions

**Static Methods**:
- `IsUserOnline(Guid userId)` - Check if user is currently online
- `GetConnectionId(Guid userId)` - Get connection ID for a specific user

#### 2. FriendsController
**Location**: `src/CommunityCar.Mvc/Controllers/Community/FriendsController.cs`

Handles all friend-related HTTP requests and integrates with FriendHub for real-time notifications.

**Key Actions**:
- `Index()` - View friends list
- `Requests()` - View pending friend requests
- `SentRequests()` - View sent friend requests
- `SendRequest(Guid friendId)` - Send friend request
- `AcceptRequest(Guid friendId)` - Accept friend request
- `RejectRequest(Guid friendId)` - Reject friend request
- `RemoveFriend(Guid friendId)` - Remove friend
- `BlockUser(Guid friendId)` - Block user
- `UnblockUser(Guid friendId)` - Unblock user
- `Search(string query)` - Search for users
- `Suggestions()` - View friend suggestions
- `Blocked()` - View blocked users
- `GetStatus(Guid friendId)` - Get friendship status

**JSON API Actions** (for AJAX calls):
- `SendRequestJson()`
- `AcceptRequestJson()`
- `RejectRequestJson()`
- `RemoveFriendJson()`
- `BlockUserJson()`
- `UnblockUserJson()`
- `SearchApi()`
- `GetPendingRequestCount()`

#### 3. FriendshipService
**Location**: `src/CommunityCar.Infrastructure/Services/Community/FriendshipService.cs`

Business logic for friend management.

### Frontend Components

#### 1. FriendHub Client
**Location**: `src/CommunityCar.Mvc/wwwroot/js/components/friend-hub.js`

JavaScript client for connecting to FriendHub and handling real-time events.

**Features**:
- Automatic connection management
- Reconnection with exponential backoff
- Event handling for all friend-related notifications
- Browser notifications support
- In-app notification display
- Custom event dispatching for integration with other components

**Events Handled**:
- `ReceiveFriendRequest` - Incoming friend request
- `FriendRequestAccepted` - Friend request accepted
- `FriendRequestRejected` - Friend request rejected
- `UserBlocked` - User blocked
- `UserUnblocked` - User unblocked
- `FriendshipRemoved` - Friendship removed
- `UserOnline` - Friend came online
- `UserOffline` - Friend went offline
- `FriendStatusChanged` - Friend status changed
- `FriendProfileUpdated` - Friend updated profile
- `NewFriendSuggestions` - New friend suggestions available
- `SystemAnnouncement` - System-wide announcement

**Custom DOM Events** (for integration):
- `friendRequestReceived`
- `friendRequestAccepted`
- `friendRequestRejected`
- `userBlocked`
- `userUnblocked`
- `friendshipRemoved`
- `userOnline`
- `userOffline`
- `friendStatusChanged`
- `friendProfileUpdated`
- `newFriendSuggestions`
- `systemAnnouncement`

## Configuration

### 1. Register FriendHub in Program.cs

```csharp
app.MapHub<CommunityCar.Infrastructure.Hubs.FriendHub>("/friendHub");
```

### 2. Add FriendHub to CultureRedirectMiddleware

The middleware skips the `/friendHub` path to prevent culture prefix issues:

```csharp
if (path.StartsWith("/friendHub"))
{
    await _next(context);
    return;
}
```

### 3. Include FriendHub Client in Views

Add to friend-related views (Index, Requests, Suggestions, etc.):

```html
@section Scripts {
    <script src="~/lib/signalr/dist/browser/signalr.min.js"></script>
    <script src="~/js/components/friend-hub.js"></script>
}
```

## Usage Examples

### Backend: Sending Real-Time Notifications

```csharp
// In FriendsController
private readonly IHubContext<FriendHub> _friendHubContext;

// Send friend request notification
var connectionId = FriendHub.GetConnectionId(friendId);
if (connectionId != null)
{
    await _friendHubContext.Clients.Client(connectionId).SendAsync("ReceiveFriendRequest", new
    {
        SenderId = userId,
        SenderName = currentUserName,
        SenderProfilePicture = currentUser?.ProfilePictureUrl,
        Timestamp = DateTimeOffset.UtcNow
    });
}
```

### Frontend: Handling Real-Time Events

```javascript
// Listen for custom events
document.addEventListener('friendRequestReceived', (event) => {
    const data = event.detail;
    console.log('New friend request from:', data.senderName);
    
    // Update UI
    updateFriendRequestList();
});

// Check connection status
if (window.friendHub && window.friendHub.isConnected) {
    console.log('FriendHub is connected');
}
```

### Frontend: Making AJAX Calls

```javascript
// Send friend request
async function sendFriendRequest(friendId) {
    const url = CultureHelper.addCultureToUrl(`/Friends/SendRequestJson`);
    
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ friendId })
        });
        
        const result = await response.json();
        if (result.success) {
            console.log('Friend request sent');
        }
    } catch (error) {
        console.error('Error sending friend request:', error);
    }
}
```

## Real-Time Notification Flow

### 1. Send Friend Request
```
User A → FriendsController.SendRequestJson()
       → FriendshipService.SendRequestAsync()
       → NotificationService.NotifyUserOfFriendRequestAsync()
       → FriendHub sends "ReceiveFriendRequest" to User B
       → User B's browser shows notification
```

### 2. Accept Friend Request
```
User B → FriendsController.AcceptRequestJson()
       → FriendshipService.AcceptRequestAsync()
       → NotificationService.NotifyUserOfFriendRequestAcceptedAsync()
       → FriendHub sends "FriendRequestAccepted" to User A
       → User A's browser shows notification
```

### 3. Block User
```
User A → FriendsController.BlockUserJson()
       → FriendshipService.BlockUserAsync()
       → FriendHub sends "UserBlocked" to User B
       → User B's browser shows notification
```

## UI Integration

### Friend Request Count Badge

```html
<span class="friend-request-count badge bg-danger" style="display: none;">0</span>
```

The FriendHub client automatically updates this count when new requests arrive.

### User Online Status Indicator

```html
<div data-user-id="@friend.FriendId">
    <span class="status-indicator"></span>
    <span class="user-name">@friend.FriendName</span>
</div>
```

The FriendHub client automatically updates the status indicator based on online/offline events.

### Notification Container

```html
<div class="notification-container"></div>
```

The FriendHub client appends notifications to this container.

## Browser Notifications

To enable browser notifications, request permission:

```javascript
// Request notification permission
if ('Notification' in window && Notification.permission === 'default') {
    Notification.requestPermission();
}
```

The FriendHub client will automatically show browser notifications when permission is granted.

## Security

### Authorization
- FriendHub requires `[Authorize]` attribute - only authenticated users can connect
- All controller actions require authentication
- User can only perform actions on their own friendships

### Connection Tracking
- User connections are tracked in a static dictionary
- Connection ID is mapped to User ID from claims
- Only the target user receives notifications (no broadcast to all users)

### Anti-Forgery Tokens
- All POST actions require `[ValidateAntiForgeryToken]`
- AJAX calls must include the anti-forgery token in headers

## Error Handling

### Connection Failures
- Automatic reconnection with exponential backoff
- Maximum 5 reconnection attempts
- Graceful degradation if SignalR is unavailable

### AJAX Failures
- All AJAX calls include `.catch()` handlers
- Errors logged to console with `console.debug()` (not `console.error()`)
- User-friendly error messages displayed

## Testing

### Manual Testing Checklist

1. **Friend Requests**
   - [ ] Send friend request
   - [ ] Receive real-time notification
   - [ ] Accept friend request
   - [ ] Reject friend request
   - [ ] Cancel sent request

2. **Friend Management**
   - [ ] View friends list
   - [ ] Remove friend
   - [ ] Receive removal notification

3. **Blocking**
   - [ ] Block user
   - [ ] Receive block notification
   - [ ] Unblock user
   - [ ] Receive unblock notification

4. **Online Status**
   - [ ] User comes online
   - [ ] User goes offline
   - [ ] Status indicator updates

5. **Suggestions**
   - [ ] View friend suggestions
   - [ ] Receive new suggestions notification

6. **Search**
   - [ ] Search for users
   - [ ] View friendship status in results

### Testing with Multiple Browsers

1. Open application in two different browsers
2. Login as different users
3. Send friend request from Browser A
4. Verify notification appears in Browser B immediately
5. Accept request in Browser B
6. Verify notification appears in Browser A immediately

## Troubleshooting

### SignalR Connection Issues

**Problem**: ERR_CONNECTION_REFUSED for /friendHub

**Solution**: 
- Ensure server is running
- Check that FriendHub is registered in Program.cs
- Verify `/friendHub` is in CultureRedirectMiddleware skip list
- Check browser console for connection errors

### Notifications Not Appearing

**Problem**: Real-time notifications not showing

**Solution**:
- Check browser console for JavaScript errors
- Verify SignalR connection is established
- Check that `data-friend-hub` attribute exists on page
- Verify user is authenticated

### AJAX Calls Failing

**Problem**: AJAX calls return 404 or 302

**Solution**:
- Ensure culture prefix is added using `CultureHelper.addCultureToUrl()`
- Check anti-forgery token is included in request
- Verify controller action exists and is accessible

## Performance Considerations

### Connection Pooling
- SignalR maintains persistent WebSocket connections
- Each user has one connection to FriendHub
- Connections are automatically cleaned up on disconnect

### Scalability
- Current implementation uses in-memory dictionary for connection tracking
- For production with multiple servers, consider:
  - Redis backplane for SignalR
  - Distributed cache for connection tracking
  - Azure SignalR Service for cloud deployments

### Bandwidth
- Only targeted notifications are sent (no broadcast to all users)
- Notifications are small JSON payloads
- Automatic reconnection prevents connection spam

## Future Enhancements

1. **Typing Indicators**: Show when friend is typing a message
2. **Read Receipts**: Show when friend has read a message
3. **Friend Groups**: Organize friends into groups
4. **Mutual Friends**: Show mutual friends in suggestions
5. **Friend Activity Feed**: Show recent friend activities
6. **Friend Recommendations**: AI-based friend suggestions
7. **Friend Requests Expiry**: Auto-expire old friend requests
8. **Friend Request Limits**: Prevent spam by limiting requests per day

## Related Documentation

- [SignalR Connection Issues](FIXING_ERR_CONNECTION_REFUSED.md)
- [MVC + AJAX Pattern](MVC_AJAX_PATTERN.md)
- [Notification Feature](NOTIFICATIONS_FEATURE.md)
- [Chat Feature](CHAT_FEATURE.md)
