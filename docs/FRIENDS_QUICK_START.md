# Friends Feature Quick Start Guide

## What's New

The Friends feature now has real-time notifications using SignalR! Users receive instant notifications for all friend-related activities without needing to refresh the page.

## Quick Test

### 1. Start the Application

```bash
dotnet run --project src/CommunityCar.Mvc
```

### 2. Open Two Browsers

- **Browser A**: Chrome (login as User 1)
- **Browser B**: Edge (login as User 2)

### 3. Test Real-Time Notifications

#### Send Friend Request
1. In Browser A, go to Friends → Find Friends
2. Search for User 2
3. Click "Send Request"
4. **Browser B should show notification immediately** ✨

#### Accept Friend Request
1. In Browser B, go to Friends → Requests
2. Click "Accept" on User 1's request
3. **Browser A should show notification immediately** ✨

#### Block User
1. In Browser A, go to Friends → My Friends
2. Click "Block" on User 2
3. **Browser B should show notification immediately** ✨

## What to Expect

### Instant Notifications
- Friend request received
- Friend request accepted/rejected
- User blocked/unblocked
- Friendship removed
- Friend came online/offline

### Visual Indicators
- Badge count updates automatically
- Online/offline status indicators
- In-app notification banners
- Browser notifications (if permission granted)

## Browser Console

Open browser console (F12) to see connection status:

```
FriendHub: Connected successfully
FriendHub: Received friend request {senderName: "John Doe", ...}
```

## Troubleshooting

### No Notifications?

1. **Check browser console** for errors
2. **Hard refresh** (Ctrl+Shift+R) to clear cache
3. **Verify server is running** on http://localhost:5010
4. **Check SignalR connection**:
   ```javascript
   window.friendHub.isConnected // should be true
   ```

### Connection Refused?

- Ensure server is running
- Check that `/friendHub` is registered in Program.cs
- Verify `/friendHub` is in CultureRedirectMiddleware skip list

## Enable Browser Notifications

To receive notifications even when tab is not active:

1. Click the lock icon in address bar
2. Allow notifications for localhost:5010
3. Refresh the page

## Next Steps

- Test all friend operations (send, accept, reject, block, unblock, remove)
- Check online/offline status indicators
- Try with multiple users
- Monitor browser console for events

## Need Help?

See full documentation:
- [Friends Feature Documentation](FRIENDS_FEATURE.md)
- [Implementation Summary](FRIENDS_SIGNALR_IMPLEMENTATION.md)
- [Fixing Connection Issues](FIXING_ERR_CONNECTION_REFUSED.md)

## Key Files

### Backend
- `src/CommunityCar.Infrastructure/Hubs/FriendHub.cs`
- `src/CommunityCar.Mvc/Controllers/Community/FriendsController.cs`

### Frontend
- `src/CommunityCar.Mvc/wwwroot/js/components/friend-hub.js`
- `src/CommunityCar.Mvc/Views/Friends/*.cshtml`

## Architecture

```
User A                    Server                    User B
  |                         |                         |
  |-- Send Request -------->|                         |
  |                         |-- Notify -------------->|
  |                         |                         |
  |                         |<-- Accept Request ------|
  |<-- Notify --------------|                         |
  |                         |                         |
```

## Real-Time Events

### Sent by Server
- `ReceiveFriendRequest` - New friend request
- `FriendRequestAccepted` - Request accepted
- `FriendRequestRejected` - Request rejected
- `UserBlocked` - User blocked you
- `UserUnblocked` - User unblocked you
- `FriendshipRemoved` - Friend removed you
- `UserOnline` - Friend came online
- `UserOffline` - Friend went offline
- `FriendStatusChanged` - Friend status changed
- `FriendProfileUpdated` - Friend updated profile
- `NewFriendSuggestions` - New suggestions available
- `SystemAnnouncement` - System message

### Custom DOM Events (for integration)
All SignalR events trigger custom DOM events that you can listen to:

```javascript
document.addEventListener('friendRequestReceived', (event) => {
    console.log('New friend request:', event.detail);
});
```

## Performance

- **Connection**: One WebSocket per user
- **Bandwidth**: < 1KB per notification
- **Latency**: < 1 second for notifications
- **Reconnection**: Automatic with exponential backoff

## Security

- ✅ Requires authentication (`[Authorize]`)
- ✅ Anti-forgery token validation
- ✅ Targeted notifications (no broadcast)
- ✅ Connection tracking by User ID
- ✅ Secure WebSocket (wss:// in production)

## Enjoy Real-Time Friends! 🎉

Your friends feature is now fully real-time. Users will love the instant feedback and seamless experience!
