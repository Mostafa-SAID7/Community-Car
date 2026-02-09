# Friends Feature with SignalR Real-Time Updates

## Overview

The Friends feature provides real-time friend management with SignalR for instant notifications and status updates. Users can send/accept/reject friend requests, block/unblock users, and see online status in real-time.

## Architecture

### Components

1. **FriendHub** (`src/CommunityCar.Infrastructure/Hubs/FriendHub.cs`)
   - SignalR hub for real-time friend interactions
   - Handles connection lifecycle and user tracking
   - Broadcasts friend events to connected users

2. **FriendsController** (`src/CommunityCar.Mvc/Controllers/Community/FriendsController.cs`)
   - MVC controller for friend management
   - Integrates with FriendHub for real-time notifications
   - Provides both page views and JSON API endpoints

3. **FriendshipService** (`src/CommunityCar.Infrastructure/Services/Community/FriendshipService.cs`)
   - Business logic for friend operations
   - Database operations via repository pattern

4. **FriendHub Client** (`src/CommunityCar.Mvc/wwwroot/js/friends-hub.js`)
   - JavaScript client for SignalR connection
   - Event handlers for real-time updates
   - UI updates and notifications

## Features

### 1. Friend Requests

**Send Request:**
- User sends friend request to another user
- Real-time notification sent via SignalR
- Database notification created
- Request appears in recipient's pending requests

**Accept Request:**
- User accepts pending friend request
- Both users become friends
- Real-time notification sent to requester
- Friend lists updated automatically

**Reject Request:**
- User rejects pending friend request
- Request removed from database
- Optional real-time notification to requester

### 2. Friend Management

**View Friends:**
- List of all accepted friendships
- Shows friend name, profile picture, and since date
- Real-time online status indicators

**Remove Friend:**
- User can remove a friend
- Real-time notification sent to removed friend
- Friendship deleted from database

### 3. Block/Unblock

**Block User:**
- User can block another user
- Removes existing friendship if present
- Real-time notification sent to blocked user
- Blocked user cannot send friend requests

**Unblock User:**
- User can unblock previously blocked user
- Real-time notification sent to unblocked user
- Users can interact again

### 4. Real-Time Status

**Online/Offline Status:**
- Automatic tracking when users connect/disconnect
- Real-time status updates for friends
- Visual indicators in UI

**Status Changes:**
- Users can set status (online/busy/away)
- Real-time broadcast to all friends
- Status displayed in friend list

### 5. Friend Suggestions

**Suggestions:**
- System suggests potential friends
- Based on mutual friends, interests, etc.
- Real-time notifications for new suggestions

### 6. Search

**User Search:**
- Search users by name or username
- Shows friendship status for each result
- Quick actions (send request, view profile)

## SignalR Events

### Server-to-Client Events

| Event | Data | Description |
|-------|------|-------------|
| `ReceiveFriendRequest` | `{ SenderId, SenderName, SenderProfilePicture, Timestamp }` | New friend request received |
| `FriendRequestAccepted` | `{ FriendId, FriendName, FriendProfilePicture, Timestamp }` | Friend request was accepted |
| `FriendRequestRejected` | `{ UserId, UserName, Timestamp }` | Friend request was rejected |
| `UserBlocked` | `{ BlockedBy, Timestamp }` | User was blocked |
| `UserUnblocked` | `{ UnblockedBy, Timestamp }` | User was unblocked |
| `FriendshipRemoved` | `{ RemovedBy, RemovedByName, Timestamp }` | Friendship was removed |
| `UserOnline` | `userId` | Friend came online |
| `UserOffline` | `userId` | Friend went offline |
| `FriendStatusChanged` | `{ FriendId, Status, Timestamp }` | Friend status changed |
| `FriendProfileUpdated` | `{ FriendId, FriendName, FriendProfilePicture, Timestamp }` | Friend updated profile |
| `NewFriendSuggestions` | `{ Count, Timestamp }` | New friend suggestions available |
| `SystemAnnouncement` | `{ Message, Type, Timestamp }` | System-wide announcement |

### Client-Side Event Handlers

The `FriendHubClient` class provides handlers for all server events:

```javascript
// Example: Listen for friend request
document.addEventListener('friendRequestReceived', (event) => {
    const data = event.detail;
    console.log('Friend request from:', data.SenderName);
    // Update UI, show notification, etc.
});
```

## API Endpoints

### Friend Requests

- `POST /{culture}/Friends/SendRequest` - Send friend request
- `POST /{culture}/Friends/SendRequestJson` - Send friend request (JSON)
- `POST /{culture}/Friends/AcceptRequest` - Accept friend request
- `POST /{culture}/Friends/AcceptRequestJson` - Accept friend request (JSON)
- `POST /{culture}/Friends/RejectRequest` - Reject friend request
- `POST /{culture}/Friends/RejectRequestJson` - Reject friend request (JSON)

### Friend Management

- `GET /{culture}/Friends` - View friends list
- `GET /{culture}/Friends/Requests` - View pending requests
- `GET /{culture}/Friends/SentRequests` - View sent requests
- `POST /{culture}/Friends/RemoveFriend` - Remove friend
- `POST /{culture}/Friends/RemoveFriendJson` - Remove friend (JSON)
- `GET /{culture}/Friends/GetPendingRequestCount` - Get pending request count

### Block/Unblock

- `POST /{culture}/Friends/BlockUser` - Block user
- `POST /{culture}/Friends/BlockUserJson` - Block user (JSON)
- `POST /{culture}/Friends/UnblockUser` - Unblock user
- `POST /{culture}/Friends/UnblockUserJson` - Unblock user (JSON)
- `GET /{culture}/Friends/Blocked` - View blocked users

### Search & Suggestions

- `GET /{culture}/Friends/Search?query={query}` - Search users
- `GET /{culture}/Friends/SearchApi?query={query}` - Search users (JSON)
- `GET /{culture}/Friends/Suggestions` - View friend suggestions
- `GET /{culture}/Friends/Status/{friendId}` - Get friendship status

## Usage Examples

### Client-Side: Send Friend Request

```javascript
// Using AJAX with culture prefix
const friendId = '12345678-1234-1234-1234-123456789012';
const url = CultureHelper.addCultureToUrl('/Friends/SendRequestJson');

fetch(url, {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]').value
    },
    body: JSON.stringify({ friendId: friendId })
})
.then(response => response.json())
.then(data => {
    if (data.success) {
        console.log('Friend request sent!');
    }
});
```

### Client-Side: Listen for Real-Time Events

```javascript
// Listen for friend request accepted
document.addEventListener('friendRequestAccepted', (event) => {
    const { FriendName, FriendProfilePicture } = event.detail;
    
    // Show notification
    alert(`${FriendName} accepted your friend request!`);
    
    // Refresh friend list
    window.location.reload();
});

// Listen for user online status
document.addEventListener('userOnline', (event) => {
    const { userId } = event.detail;
    
    // Update UI to show user is online
    const statusIndicator = document.querySelector(`[data-user-id="${userId}"] .status`);
    if (statusIndicator) {
        statusIndicator.classList.add('online');
    }
});
```

### Server-Side: Send Real-Time Notification

```csharp
// In FriendsController
var connectionId = CommunityCar.Infrastructure.Hubs.FriendHub.GetConnectionId(friendId);
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

## Configuration

### 1. Hub Registration

The FriendHub is registered in `Program.cs`:

```csharp
app.MapHub<CommunityCar.Infrastructure.Hubs.FriendHub>("/friendHub");
```

### 2. Culture Middleware

The FriendHub path is excluded from culture redirect in `CultureRedirectMiddleware.cs`:

```csharp
if (path.StartsWith("/friendHub") || ...)
{
    await _next(context);
    return;
}
```

### 3. Client Script

Include the FriendHub client script in your layout or specific pages:

```html
<script src="~/lib/signalr/dist/browser/signalr.min.js"></script>
<script src="~/js/friends-hub.js"></script>
```

### 4. Authentication

Set the authentication flag in your layout:

```html
<body data-authenticated="@User.Identity.IsAuthenticated.ToString().ToLower()">
```

## UI Integration

### Friend Request Badge

```html
<span id="friend-request-count" class="badge">0</span>
```

### Online Status Indicator

```html
<div data-user-id="@friend.FriendId">
    <span class="online-status"></span>
    <span class="friend-name">@friend.FriendName</span>
</div>
```

### Connection Status

```html
<div id="friend-hub-status" class="connection-status"></div>
```

## Notifications

### Browser Notifications

The client automatically requests browser notification permission and shows notifications for:
- New friend requests
- Friend request accepted
- Friendship removed
- New friend suggestions

### In-App Notifications

Toast-style notifications appear in the app for all friend events.

### Notification Sound

A notification sound plays for important events (configurable).

## Security

### Authorization

- FriendHub requires `[Authorize]` attribute
- Only authenticated users can connect
- User ID extracted from claims

### Connection Tracking

- Static dictionary tracks user connections
- Connection ID mapped to user ID
- Automatic cleanup on disconnect

### Validation

- All friend operations validate user authentication
- Cannot send request to self
- Cannot block self
- Friendship status checked before operations

## Performance Considerations

### Connection Management

- Automatic reconnection with exponential backoff
- Maximum 5 reconnection attempts
- 3-second delay between attempts

### Scalability

- Static dictionary for connection tracking (consider Redis for multi-server)
- Connection ID lookup is O(1)
- Targeted notifications (not broadcast to all)

### Optimization

- Only send notifications to online users
- Batch status updates when possible
- Lazy-load friend lists

## Troubleshooting

### Connection Issues

1. **ERR_CONNECTION_REFUSED**
   - Ensure server is running
   - Check FriendHub is registered in Program.cs
   - Verify `/friendHub` is excluded from culture redirect

2. **401 Unauthorized**
   - User must be authenticated
   - Check `[Authorize]` attribute on hub
   - Verify authentication cookie is sent

3. **Reconnection Failures**
   - Check network connectivity
   - Verify server is running
   - Check browser console for errors

### Notification Issues

1. **No Browser Notifications**
   - Check notification permission
   - Call `requestNotificationPermission()`
   - Verify HTTPS (required for notifications)

2. **No In-App Notifications**
   - Check notification container exists
   - Verify CSS styles are loaded
   - Check browser console for errors

### Real-Time Update Issues

1. **Status Not Updating**
   - Verify SignalR connection is active
   - Check event handlers are registered
   - Verify data-attributes in HTML

2. **Friend List Not Refreshing**
   - Check `refreshFriendList()` is called
   - Verify page reload or AJAX update
   - Check for JavaScript errors

## Testing

### Manual Testing

1. Open two browser windows (different users)
2. Send friend request from User A to User B
3. Verify User B receives real-time notification
4. Accept request from User B
5. Verify User A receives acceptance notification
6. Check both users see each other in friend list

### Automated Testing

```csharp
// Example unit test for FriendHub
[Fact]
public async Task SendFriendRequest_ShouldNotifyReceiver()
{
    // Arrange
    var hub = new FriendHub(_logger);
    var mockClients = new Mock<IHubCallerClients>();
    
    // Act
    await hub.SendFriendRequest(receiverId, senderName, senderPicture);
    
    // Assert
    mockClients.Verify(x => x.Client(It.IsAny<string>())
        .SendAsync("ReceiveFriendRequest", It.IsAny<object>(), default));
}
```

## Future Enhancements

1. **Mutual Friends**
   - Show mutual friends in suggestions
   - Display mutual friend count

2. **Friend Groups**
   - Organize friends into groups
   - Group-based notifications

3. **Activity Feed**
   - Show friend activities
   - Real-time activity updates

4. **Video/Voice Chat**
   - Integrate WebRTC for calls
   - Use SignalR for signaling

5. **Presence Status**
   - Rich presence (playing game, listening to music)
   - Custom status messages

6. **Friend Recommendations**
   - ML-based friend suggestions
   - Based on interests, location, etc.

## Related Documentation

- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- [FRIENDS_FEATURE.md](./FRIENDS_FEATURE.md) - Basic friends feature
- [NOTIFICATIONS_FEATURE.md](./NOTIFICATIONS_FEATURE.md) - Notification system
- [CHAT_FEATURE.md](./CHAT_FEATURE.md) - Chat with SignalR

## Support

For issues or questions:
1. Check browser console for errors
2. Check server logs for exceptions
3. Verify SignalR connection status
4. Review this documentation
5. Contact development team
