# SignalR Hubs Standardization - Complete Summary

## Overview
All SignalR hubs have been standardized to follow Clean Architecture principles with consistent patterns for real-time communication, authorization, groups handling, logging, and connection lifecycle management.

## Architecture Pattern

### 1. Base Hub (`BaseHub<THub>`)
**Location:** `src/CommunityCar.Infrastructure/Hubs/Base/BaseHub.cs`

**Features:**
- Generic base class for all SignalR hubs
- Thread-safe connection tracking using `ConcurrentDictionary`
- Automatic user-to-connection mapping
- Built-in group management
- Comprehensive logging
- Virtual methods for lifecycle hooks
- Authorization via `[Authorize]` attribute

**Key Components:**
```csharp
// Connection tracking
protected static readonly ConcurrentDictionary<Guid, HashSet<string>> UserConnections
protected static readonly ConcurrentDictionary<string, Guid> ConnectionUsers
protected static readonly ConcurrentDictionary<string, HashSet<string>> GroupConnections

// Lifecycle methods
public override async Task OnConnectedAsync()
public override async Task OnDisconnectedAsync(Exception? exception)

// Virtual hooks for derived classes
protected virtual Task OnUserConnected(Guid userId, string connectionId)
protected virtual Task OnUserDisconnected(Guid userId, string connectionId, bool isLastConnection)
protected virtual Task OnUserOnline(Guid userId)
protected virtual Task OnUserOffline(Guid userId)
protected virtual Task OnGroupJoined(string groupName, string connectionId)
protected virtual Task OnGroupLeft(string groupName, string connectionId)

// Helper methods
protected Guid? GetUserId()
protected string? GetUserName()
protected string? GetUserEmail()
public static bool IsUserOnline(Guid userId)
public static List<string> GetUserConnectionIds(Guid userId)
public static int GetOnlineUserCount()
public static List<Guid> GetOnlineUserIds()
public static int GetGroupConnectionCount(string groupName)
```

### 2. Hub Services Pattern

All hub services follow Clean Architecture with:
- **Interface in Domain Layer:** `src/CommunityCar.Domain/Interfaces/Community/I{Name}HubService.cs`
- **Implementation in Infrastructure Layer:** `src/CommunityCar.Infrastructure/Services/Community/{Name}HubService.cs`
- **Dependency Injection:** `private readonly IHubContext<{Name}Hub> _hubContext`
- **Logging:** `private readonly ILogger<{Name}HubService> _logger`
- **Error Handling:** Try-catch blocks with logging
- **Timestamps:** All notifications include `DateTimeOffset.UtcNow`

**Service Pattern:**
```csharp
public class QuestionHubService : IQuestionHubService
{
    private readonly IHubContext<QuestionHub> _hubContext;
    private readonly ILogger<QuestionHubService> _logger;

    public QuestionHubService(IHubContext<QuestionHub> hubContext, ILogger<QuestionHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastNewQuestionAsync(QuestionDto question)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveQuestion", new
            {
                Question = question,
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("New question {QuestionId} broadcasted", question.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting new question {QuestionId}", question.Id);
        }
    }
}
```

## Standardized Hubs

### 1. QuestionHub
**Purpose:** Real-time Q&A notifications and updates
**Location:** `src/CommunityCar.Infrastructure/Hubs/QuestionHub.cs`
**Service:** `QuestionHubService` with `IQuestionHubService`

**Features:**
- Question thread management (`JoinQuestionThread`, `LeaveQuestionThread`)
- Watcher count tracking
- Inherits from `BaseHub<QuestionHub>`

**Service Methods:**
- `BroadcastNewQuestionAsync` - Broadcast new questions to all users
- `BroadcastNewAnswerAsync` - Broadcast answers to question thread
- `BroadcastQuestionScoreUpdateAsync` - Real-time score updates
- `BroadcastAnswerScoreUpdateAsync` - Real-time answer score updates
- `BroadcastQuestionResolvedAsync` - Resolution status updates
- `NotifyNewAnswerAsync` - Notify question author
- `NotifyQuestionVotedAsync` - Notify about votes
- `NotifyAnswerVotedAsync` - Notify answer author about votes
- `NotifyAnswerAcceptedAsync` - Notify when answer is accepted
- `NotifyNewCommentAsync` - Notify content owner
- `BroadcastCommentToThreadAsync` - Broadcast comments to thread
- `BroadcastCommentUpdatedAsync` - Broadcast comment updates
- `BroadcastCommentDeletedAsync` - Broadcast comment deletions

### 2. NotificationHub
**Purpose:** System notifications and alerts
**Location:** `src/CommunityCar.Infrastructure/Hubs/NotificationHub.cs`
**Service:** Uses `NotificationHubService` with `INotificationHubService`

**Features:**
- General notification delivery
- Inherits from `BaseHub<NotificationHub>`
- Minimal hub-specific logic (uses service layer)

### 3. FriendHub
**Purpose:** Friend-related real-time notifications
**Location:** `src/CommunityCar.Infrastructure/Hubs/FriendHub.cs`
**Service:** `FriendHubService` with `IFriendHubService`

**Features:**
- Friend online/offline status
- Inherits from `BaseHub<FriendHub>`

**Service Methods:**
- `NotifyFriendRequestAsync` - Send friend request notifications
- `NotifyFriendRequestAcceptedAsync` - Notify request acceptance
- `NotifyFriendRequestRejectedAsync` - Notify request rejection
- `NotifyUserBlockedAsync` - Notify user blocking
- `NotifyUserUnblockedAsync` - Notify user unblocking
- `NotifyFriendshipRemovedAsync` - Notify friendship removal
- `NotifyNewSuggestionsAsync` - Notify about friend suggestions
- `NotifyFriendProfileUpdatedAsync` - Notify profile updates
- `UpdateUserStatusAsync` - Update user status to friends
- `BroadcastAnnouncementAsync` - System-wide announcements

### 4. PostHub
**Purpose:** Post-related real-time notifications
**Location:** `src/CommunityCar.Infrastructure/Hubs/PostHub.cs`
**Service:** `PostHubService` with `IPostHubService`

**Features:**
- Post thread management (`JoinPostThread`, `LeavePostThread`)
- Post watcher count tracking
- Inherits from `BaseHub<PostHub>`

**Service Methods:**
- `NotifyFriendsNewPostAsync` - Notify friends about new posts
- `NotifyPostCreatedAsync` - Confirm post creation
- `NotifyPostUpdatedAsync` - Confirm post update
- `NotifyPostDeletedAsync` - Confirm post deletion
- `NotifyPostLikedAsync` - Notify post author about likes
- `NotifyPostCommentedAsync` - Notify about comments
- `NotifyPostSharedAsync` - Notify about shares
- `UpdatePostEngagementAsync` - Real-time engagement metrics
- `BroadcastNewCommentAsync` - Broadcast new comments
- `BroadcastCommentUpdatedAsync` - Broadcast comment updates
- `BroadcastCommentDeletedAsync` - Broadcast comment deletions
- `NotifyPostStatusChangedAsync` - Notify status changes
- `NotifyPostPinnedAsync` - Notify pin status changes
- `NotifyCommentReplyAsync` - Notify about comment replies
- `NotifyPostMilestoneAsync` - Notify milestone achievements

### 5. ChatHub
**Purpose:** Real-time chat and messaging
**Location:** `src/CommunityCar.Infrastructure/Hubs/ChatHub.cs`
**Service:** `ChatHubService` with `IChatHubService`

**Features:**
- Chat room management (`JoinChatRoom`, `LeaveChatRoom`)
- Direct messaging support
- Typing indicators
- Read receipts
- Inherits from `BaseHub<ChatHub>`

**Service Methods:**
- `SendMessageAsync` - Send direct messages
- `NotifyMessageReadAsync` - Notify message read status
- `NotifyTypingAsync` - Notify typing indicator
- `NotifyStopTypingAsync` - Notify stop typing
- `SendChatRoomMessageAsync` - Send chat room messages
- `NotifyChatRoomTypingAsync` - Chat room typing indicator
- `NotifyChatRoomStopTypingAsync` - Chat room stop typing
- `NotifyMessageDeliveredAsync` - Notify message delivery
- `NotifyMessageDeletedAsync` - Notify message deletion
- `NotifyMessageEditedAsync` - Notify message edits

### 6. CommunityHub
**Purpose:** Centralized hub for all community communications
**Location:** `src/CommunityCar.Infrastructure/Hubs/CommunityHub.cs`

**Features:**
- Community group management
- Event group management
- Inherits from `BaseHub<CommunityHub>`
- Aggregates multiple communication types

### 7. GenericHub
**Purpose:** Generic hub with backward compatibility
**Location:** `src/CommunityCar.Infrastructure/Hubs/GenericHub.cs`
**Service:** `NotificationHubService` with `INotificationHubService`

**Features:**
- Generic notification methods
- Backward compatibility with existing code
- Inherits from `BaseHub<GenericHub>`
- Provides helper methods for all notification types

## Key Benefits

### 1. Consistency
- All hubs follow the same architectural pattern
- Predictable behavior across all real-time features
- Standardized error handling and logging

### 2. Clean Architecture
- Clear separation of concerns
- Domain interfaces in Domain layer
- Implementation in Infrastructure layer
- Easy to test and mock

### 3. Authorization
- All hubs require authentication via `[Authorize]` attribute
- User identity extracted from claims
- Secure connection management

### 4. Connection Management
- Thread-safe connection tracking
- Support for multiple tabs/devices per user
- Automatic cleanup on disconnect
- Online/offline status tracking

### 5. Group Management
- Flexible group joining/leaving
- Thread-specific groups (questions, posts, chat rooms)
- User-specific groups for targeted notifications
- Group member count tracking

### 6. Logging
- Comprehensive logging at all levels
- Connection lifecycle logging
- Error logging with context
- Performance monitoring capability

### 7. Real-time Features
- Instant notifications
- Live updates
- Typing indicators
- Read receipts
- Online status
- Engagement metrics

## Usage Examples

### From Controllers/Services

```csharp
// Inject the hub service
private readonly IQuestionHubService _questionHubService;

public QuestionsController(IQuestionHubService questionHubService)
{
    _questionHubService = questionHubService;
}

// Use in action methods
public async Task<IActionResult> Create(CreateQuestionDto dto)
{
    var question = await _questionService.CreateAsync(dto);
    
    // Broadcast to all users
    await _questionHubService.BroadcastNewQuestionAsync(question);
    
    return Ok(question);
}

// Notify specific user
public async Task<IActionResult> CreateAnswer(CreateAnswerDto dto)
{
    var answer = await _answerService.CreateAsync(dto);
    
    // Notify question author
    await _questionHubService.NotifyNewAnswerAsync(
        questionAuthorId: answer.Question.AuthorId,
        answerId: answer.Id,
        answerAuthorId: answer.AuthorId,
        answerAuthorName: answer.Author.UserName,
        questionTitle: answer.Question.Title
    );
    
    // Broadcast to thread watchers
    await _questionHubService.BroadcastNewAnswerAsync(answer);
    
    return Ok(answer);
}
```

### From Client (JavaScript)

```javascript
// Connect to hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/question")
    .withAutomaticReconnect()
    .build();

// Join question thread
await connection.invoke("JoinQuestionThread", questionId);

// Listen for events
connection.on("ReceiveAnswer", (data) => {
    console.log("New answer:", data);
    updateUI(data);
});

connection.on("QuestionScoreUpdated", (data) => {
    updateScore(data.QuestionId, data.NewScore);
});

// Start connection
await connection.start();
```

## Registration in DependencyInjection.cs

```csharp
// Hub services
services.AddScoped<IQuestionHubService, QuestionHubService>();
services.AddScoped<IFriendHubService, FriendHubService>();
services.AddScoped<IPostHubService, PostHubService>();
services.AddScoped<IChatHubService, ChatHubService>();
services.AddScoped<INotificationHubService, NotificationHubService>();
```

## Hub Endpoints in Program.cs

```csharp
// Map SignalR hubs
app.MapHub<QuestionHub>("/hubs/question");
app.MapHub<NotificationHub>("/hubs/notification");
app.MapHub<FriendHub>("/hubs/friend");
app.MapHub<PostHub>("/hubs/post");
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<CommunityHub>("/hubs/community");
app.MapHub<GenericHub>("/hubs/generic");
```

## Migration Notes

### For Existing Code
1. Replace direct hub method calls with service layer calls
2. Update client-side event names if needed
3. Ensure proper dependency injection registration
4. Test connection lifecycle and group management

### Breaking Changes
- Hub methods are now primarily in service layer
- Some event names may have changed for consistency
- Connection tracking is now centralized in BaseHub

## Future Enhancements

1. **Rate Limiting:** Add rate limiting to prevent abuse
2. **Message Queuing:** Implement message queuing for offline users
3. **Presence System:** Enhanced presence tracking with status messages
4. **Analytics:** Add analytics for hub usage and performance
5. **Compression:** Enable message compression for large payloads
6. **Encryption:** Add end-to-end encryption for sensitive messages

## Conclusion

All SignalR hubs now follow a consistent, clean architecture pattern with:
- ✅ Authorization via `[Authorize]` attribute
- ✅ Proper connection lifecycle management
- ✅ Thread-safe group handling
- ✅ Comprehensive logging
- ✅ Clean Architecture with service layer
- ✅ Error handling and resilience
- ✅ Real-time capabilities
- ✅ Scalability and maintainability

The standardization ensures all hubs work the same way, making the codebase easier to understand, maintain, and extend.
