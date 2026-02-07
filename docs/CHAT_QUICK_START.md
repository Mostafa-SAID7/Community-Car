# Chat Feature - Quick Start Guide

## Prerequisites
- .NET 9.0 SDK installed
- SQL Server or compatible database
- Entity Framework Core tools installed

## Setup Steps

### 1. Database Migration

Run these commands from the solution root:

```bash
# Create migration
dotnet ef migrations add AddChatEntities --project src/CommunityCar.Infrastructure --startup-project src/CommunityCar.Mvc

# Apply migration
dotnet ef database update --project src/CommunityCar.Infrastructure --startup-project src/CommunityCar.Mvc
```

### 2. Configure SignalR (Optional but Recommended)

Open `src/CommunityCar.Mvc/Program.cs` and add:

```csharp
// After builder.Services.AddControllersWithViews()
builder.Services.AddSignalR();

// Before app.Run()
app.MapHub<CommunityCar.Infrastructure.Hubs.ChatHub>("/hubs/chat");
```

### 3. Install SignalR Client Library (If using SignalR)

Option A - Using LibMan:
```bash
cd src/CommunityCar.Mvc
libman install @microsoft/signalr@latest --destination wwwroot/lib/signalr
```

Option B - Manual download:
Download from: https://unpkg.com/@microsoft/signalr@latest/dist/browser/signalr.min.js
Place in: `src/CommunityCar.Mvc/wwwroot/lib/signalr/dist/browser/`

### 4. Add Navigation Link

Add to your main layout file (`src/CommunityCar.Mvc/Views/Shared/_Layout.cshtml`):

```html
<li class="nav-item">
    <a class="nav-link" asp-area="Communications" asp-controller="Chats" asp-action="Index">
        <i class="fas fa-comments"></i> Messages
        <span class="badge bg-danger" id="chatUnreadBadge"></span>
    </a>
</li>
```

### 5. Add Unread Count Badge Script (Optional)

Add to your layout file before `</body>`:

```html
<script>
    // Update unread message count
    function updateChatUnreadCount() {
        $.get('/Communications/Chats/GetUnreadCount', function(data) {
            if (data.success && data.count > 0) {
                $('#chatUnreadBadge').text(data.count).show();
            } else {
                $('#chatUnreadBadge').hide();
            }
        });
    }
    
    // Update every 30 seconds
    setInterval(updateChatUnreadCount, 30000);
    updateChatUnreadCount(); // Initial load
</script>
```

### 6. Verify Service Registration

Check `src/CommunityCar.Infrastructure/DependencyInjection.cs` contains:

```csharp
services.AddScoped<IChatService, ChatService>();
```

If not present, add it to the service registration section.

### 7. Build and Run

```bash
cd src/CommunityCar.Mvc
dotnet build
dotnet run
```

### 8. Test the Feature

1. Navigate to `/Communications/Chats`
2. Start a conversation with another user
3. Send messages back and forth
4. Verify unread counts update
5. Test read receipts

## Choosing Between Basic and SignalR Version

### Basic Version (Conversation.cshtml)
- **Pros**: Simpler, no additional dependencies
- **Cons**: 5-second polling, not truly real-time
- **Use when**: You want simplicity or can't use WebSockets

### SignalR Version (ConversationWithSignalR.cshtml)
- **Pros**: True real-time, typing indicators, online status
- **Cons**: Requires SignalR setup and WebSocket support
- **Use when**: You want the best user experience

To use SignalR version, rename:
- `ConversationWithSignalR.cshtml` → `Conversation.cshtml`

## Troubleshooting

### Migration Fails
```bash
# Check connection string in appsettings.json
# Ensure database server is running
# Try: dotnet ef database drop --force
# Then run migrations again
```

### SignalR Not Connecting
- Check browser console for errors
- Verify `/hubs/chat` endpoint is mapped in Program.cs
- Ensure SignalR client library is loaded
- Check firewall/proxy settings for WebSocket support

### Messages Not Appearing
- Check browser console for JavaScript errors
- Verify AJAX endpoints return success
- Check database for message records
- Verify user authentication

### Unread Count Not Updating
- Check `LastReadAt` field in ChatRoomMembers table
- Verify `MarkConversationAsReadAsync` is being called
- Check browser console for errors

## Quick Test Checklist

- [ ] Can access `/Communications/Chats`
- [ ] Can see conversation list
- [ ] Can open a conversation
- [ ] Can send a message
- [ ] Message appears in chat
- [ ] Unread count updates
- [ ] Can delete own message
- [ ] Read receipts work (double check)
- [ ] Typing indicator works (SignalR only)
- [ ] Online status works (SignalR only)

## Next Steps

After basic setup:
1. Customize the UI to match your design
2. Add group chat support
3. Implement file/image sharing
4. Add push notifications
5. Implement message search

## Support

For issues or questions:
- Check the full documentation: `docs/CHAT_FEATURE.md`
- Review implementation summary: `docs/CHAT_IMPLEMENTATION_SUMMARY.md`
- Check diagnostics with: `dotnet build`

## Performance Tips

1. **Database Indexes**: Already configured in entity configurations
2. **Pagination**: Adjust page size in controller (default: 50)
3. **Caching**: Consider Redis for active conversations
4. **SignalR**: Use Azure SignalR Service for scaling

## Security Checklist

- [x] Authentication required
- [x] Authorization checks
- [x] CSRF protection
- [x] Input validation
- [ ] Rate limiting (recommended)
- [ ] Content filtering (recommended)

## Production Considerations

Before deploying to production:
1. Enable rate limiting on message endpoints
2. Add comprehensive logging
3. Set up monitoring for SignalR connections
4. Configure Azure SignalR Service for scaling
5. Add message content filtering
6. Implement backup/archive strategy
7. Test under load

## Estimated Setup Time

- Basic setup: 15-30 minutes
- With SignalR: 30-45 minutes
- Full customization: 2-4 hours

## Resources

- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core MVC](https://docs.microsoft.com/en-us/aspnet/core/mvc/)
