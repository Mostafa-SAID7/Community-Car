# SignalR Hub Architecture - Clean Architecture Implementation

## Overview

All SignalR hubs in the CommunityCar application follow a standardized Clean Architecture pattern with proper separation of concerns, authorization, groups handling, logging, and connection lifecycle management.

## Architecture Pattern

### 1. Hub Layer (Infrastructure)
**Location**: `src/CommunityCar.Infrastructure/Hubs/`

All hubs follow this standardized structure:

```csharp
[Authorize]
public class XxxHub : Hub
{
    // Thread-safe connection tracking
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();
    private static readonly ConcurrentDictionary<string, HashSet<string>> _groupConnections = new();
    
    private readonly ILogger<XxxHub> _logger;

    public XxxHub(ILogger<XxxHub> logger)
    {
        _logger = logger;
    }

    #region Connection Management
    public override async Task OnConnectedAsync() { }
    public override async Task OnDisconnectedAsync(Exception? exception) { }
    #endregion

    #region Group Management
    public async Task JoinGroup(string groupName) { }
    public async Task LeaveGroup(string groupName) { }
    #endregion

    #region Utility Methods
    public static bool IsUserOnline(Guid userId) { }
    public static List<string> GetUserConnectionIds(Guid userId) { }
    public static int GetOnlineUserCount() { }
    public static List<Guid> GetOnlineUserIds() { }
    #endregion
}
```

### 2. Service Interface Layer (Domain)
**Location**: `src/CommunityCar.Domain/Interfaces/Community/`

```csharp
public interface IXxxHubService
{
    // Broadcast methods
    Task BroadcastXxxAsync(...);
    
    // User-specific notifications
    Task NotifyUserXxxAsync(Guid userId, ...);
    
    // Group notifications
    Task NotifyGroupXxxAsync(string groupName, ...);
}
```

### 3. Service Implementation Layer (Infrastructure)
**Location**: `src/CommunityCar.Infrastructure/Services/Community/`

```csharp
public class XxxHubService : IXxxHubService
{
    private readonly IHubContext<XxxHub> _hubContext;
    private readonly ILogger<XxxHubService> _logger;

    public XxxHubService(IHubContext<XxxHub> hubContext, ILogger<XxxHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastXxxAsync(...)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("EventName", data);
            _logger.LogInformation("...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "...");
            throw;
        }
    }
}
```

### 4. Dependency Injection Registration
**Location**: `src/CommunityCar.Infrastructure/DependencyInjection.cs`

```csharp
// SignalR Hub Services (Clean Architecture with IHubContext)
services.AddScoped<INotificationHubService, NotificationHubService>();
services.AddScoped<IQuestionHubService, QuestionHubService>();
services.AddScoped<IPostHubService, PostHubService>();
services.AddScoped<IFriendHubService, FriendHubService>();
```

## Implemented Hubs

### 1. QuestionHub
**Purpose**: Real-time Q&A notifications and updates

**Features**:
- Question/Answer broadcasting
- Vote notifications
- Comment management
- Score updates
- Resolution status tracking

**Service**: `IQuestionHubService` / `QuestionHubService`

**Groups**:
- `user_{userId}` - Personal notifications
- `question_{questionId}` - Question thread watchers

### 2. PostHub
**Purpose**: Real-time post-related notifications

**Features**:
- Post creation/update/deletion
- Like/Comment/Share notifications
- Engagement metrics updates
- Comment thread management
- Milestone notifications

**Service**: `IPostHubService` / `PostHubService`

**Groups**:
- `user_{userId}` - Personal notifications
- `post_{postId}` - Post thread watchers

### 3. FriendHub
**Purpose**: Real-time friend-related notifications

**Features**:
- Friend request notifications
- Accept/Reject notifications
- Block/Unblock notifications
- Friend suggestions
- Profile updates
- Status changes (online/offline/busy)

**Service**: `IFriendHubService` / `FriendHubService`

**Groups**:
- `user_{userId}` - Personal notifications
- Custom friend circles

### 4. NotificationHub
**Purpose**: General system notifications

**Features**:
- System-wide notifications
- Alert broadcasting
- General user notifications

**Service**: Uses `INotificationHubService` / `NotificationHubService`

**Groups**:
- `user_{userId}` - Personal notifications
- Custom notification groups

### 5. GenericHub
**Purpose**: Centralized hub for backward compatibility

**Features**:
- Generic event broadcasting
- Multi-purpose notifications
- Backward compatibility layer

**Service**: `INotificationHubService` / `NotificationHubService`

## Key Features

### 1. Authorization
All hubs use `[Authorize]` attribute to ensure only authenticated users can connect.

### 2. Connection Lifecycle Management
- **OnConnectedAsync**: 
  - Tracks user connections using `ConcurrentDictionary`
  - Supports multiple connections per user (multiple tabs/devices)
  - Automatically joins user to personal group `user_{userId}`
  - Broadcasts "UserOnline" event on first connection
  - Comprehensive logging

- **OnDisconnectedAsync**:
  - Removes connection from tracking
  - Broadcasts "UserOffline" event when last connection closes
  - Cleans up group memberships
  - Handles exceptions gracefully

### 3. Groups Handling
- **Personal Groups**: `user_{userId}` for targeted notifications
- **Thread Groups**: `question_{questionId}`, `post_{postId}` for thread-specific updates
- **Custom Groups**: Support for chat rooms, friend circles, etc.
- **Thread-safe**: Uses `ConcurrentDictionary` for group tracking

### 4. Logging
- Connection/Disconnection events
- Group join/leave operations
- Notification sending
- Error handling with detailed context

### 5. Thread Safety
- Uses `ConcurrentDictionary<Guid, HashSet<string>>` for connection tracking
- Supports multiple simultaneous connections per user
- Thread-safe group management

### 6. Error Handling
- Try-catch blocks in all service methods
- Detailed error logging
- Exception propagation for proper error handling

## Usage Examples

### From Application Services

```csharp
public class QuestionService
{
    private readonly IQuestionHubService _questionHubService;

    public async Task CreateQuestionAsync(CreateQuestionDto dto)
    {
        // ... create question logic ...
        
        // Broadcast to all users
        await _questionHubService.BroadcastNewQuestionAsync(questionDto);
    }

    public async Task VoteQuestionAsync(Guid questionId, Guid userId, int voteType)
    {
        // ... vote logic ...
        
        // Notify question author
        await _questionHubService.NotifyQuestionVotedAsync(
            questionAuthorId, userId, userName, questionId, questionTitle, voteType);
        
        // Update score for all viewers
        await _questionHubService.BroadcastQuestionScoreUpdateAsync(questionId, newScore);
    }
}
```

### From Client (JavaScript)

```javascript
// Connect to hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/question")
    .build();

// Join question thread
await connection.invoke("JoinQuestionThread", questionId);

// Listen for events
connection.on("ReceiveAnswer", (data) => {
    console.log("New answer:", data);
});

connection.on("QuestionScoreUpdated", (data) => {
    updateScoreUI(data.QuestionId, data.NewScore);
});

// Leave question thread
await connection.invoke("LeaveQuestionThread", questionId);
```

## Benefits of This Architecture

1. **Clean Architecture**: Clear separation between hubs, services, and interfaces
2. **Testability**: Services can be easily mocked and tested
3. **Maintainability**: Consistent pattern across all hubs
4. **Scalability**: Thread-safe connection tracking supports high concurrency
5. **Flexibility**: Easy to add new hubs following the same pattern
6. **Observability**: Comprehensive logging for debugging and monitoring
7. **Type Safety**: Strong typing through interfaces
8. **Error Handling**: Consistent error handling and logging

## Adding a New Hub

To add a new hub following this architecture:

1. **Create Hub** in `Infrastructure/Hubs/`:
   - Copy structure from `QuestionHub.cs`
   - Implement connection lifecycle
   - Add group management
   - Add utility methods

2. **Create Interface** in `Domain/Interfaces/Community/`:
   - Define service methods
   - Use async Task return types
   - Document with XML comments

3. **Create Service** in `Infrastructure/Services/Community/`:
   - Inject `IHubContext<YourHub>`
   - Inject `ILogger<YourHubService>`
   - Implement interface methods with try-catch
   - Use hub's static methods for connection tracking

4. **Register in DI** in `Infrastructure/DependencyInjection.cs`:
   ```csharp
   services.AddScoped<IYourHubService, YourHubService>();
   ```

5. **Map Hub Endpoint** in `Program.cs`:
   ```csharp
   app.MapHub<YourHub>("/hubs/your-hub-name");
   ```

## Best Practices

1. **Always use IHubContext in services** - Never call hub methods directly
2. **Use groups for targeted notifications** - More efficient than individual sends
3. **Log all significant events** - Helps with debugging and monitoring
4. **Handle exceptions gracefully** - Always wrap in try-catch
5. **Use ConcurrentDictionary** - For thread-safe connection tracking
6. **Support multiple connections** - Users may have multiple tabs/devices
7. **Clean up resources** - Remove connections and groups on disconnect
8. **Use meaningful event names** - Clear, descriptive event names for clients
9. **Include timestamps** - Always include timestamp in event data
10. **Document everything** - XML comments for all public methods

## Connection Tracking

All hubs maintain two static dictionaries:

```csharp
// Maps userId to set of connectionIds (supports multiple connections)
private static readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();

// Maps groupName to set of connectionIds
private static readonly ConcurrentDictionary<string, HashSet<string>> _groupConnections = new();
```

This allows:
- Checking if a user is online
- Getting all connections for a user
- Counting online users
- Tracking group memberships

## Security Considerations

1. **Authorization**: All hubs require authentication via `[Authorize]` attribute
2. **User Validation**: Always validate user identity from `Context.User`
3. **Input Validation**: Validate all parameters in hub methods
4. **Rate Limiting**: Consider implementing rate limiting for high-frequency events
5. **Data Sanitization**: Sanitize all data before broadcasting

## Performance Considerations

1. **Use Groups**: More efficient than sending to individual connections
2. **Batch Operations**: Combine multiple notifications when possible
3. **Async All The Way**: All methods are async for better scalability
4. **Connection Pooling**: SignalR handles connection pooling automatically
5. **Message Size**: Keep messages small and focused

## Monitoring and Debugging

All hubs log:
- Connection/Disconnection events with userId and connectionId
- Group join/leave operations
- Notification sending with recipient information
- Errors with full exception details

Use these logs to:
- Track user activity
- Debug connection issues
- Monitor notification delivery
- Identify performance bottlenecks

## Future Enhancements

Potential improvements to consider:
1. Redis backplane for scaling across multiple servers
2. Message persistence for offline users
3. Delivery confirmation and retry logic
4. Rate limiting per user
5. Analytics and metrics collection
6. A/B testing for notification strategies
