# Chat Feature Documentation

## Overview
The Chat feature provides real-time messaging capabilities between users in the CommunityCar application. It supports one-on-one conversations with read receipts, message history, and unread message tracking.

## Architecture

### Domain Layer
- **Entities**:
  - `ChatRoom`: Represents a chat room (supports both direct and group chats)
  - `ChatMessage`: Individual messages within a chat room
  - `ChatRoomMember`: Tracks user membership in chat rooms
  
- **DTOs**:
  - `ChatMessageDto`: Message data transfer object
  - `ChatConversationDto`: Conversation summary with unread count
  - `SendMessageDto`: DTO for sending new messages

- **Enums**:
  - `MessageType`: Text, Image, File, Audio, Video, Link, System

### Infrastructure Layer
- **Service**: `ChatService` - Implements `IChatService` interface
- **Configurations**: EF Core entity configurations for chat entities
- **Mappings**: AutoMapper profile for chat DTOs

### Presentation Layer
- **Controller**: `ChatsController` in Communications area
- **Views**:
  - `Index.cshtml`: List of conversations
  - `Conversation.cshtml`: Chat interface with message history
- **ViewModels**: Chat-related view models

## Features

### 1. Conversation List
- View all active conversations
- Display last message and timestamp
- Show unread message count per conversation
- User profile pictures and online status indicators

### 2. Chat Interface
- Real-time message sending and receiving
- Message history with pagination
- Read receipts (double check marks)
- Auto-scroll to latest messages
- Sidebar with conversation list

### 3. Message Management
- Send text messages (up to 2000 characters)
- Delete own messages
- Mark messages/conversations as read
- Track unread message count

### 4. User Experience
- Responsive design for mobile and desktop
- Auto-refresh for new messages (5-second interval)
- Visual distinction between sent and received messages
- Timestamp display for all messages

## API Endpoints

### GET /Communications/Chats
Lists all conversations for the current user.

### GET /Communications/Chats/Conversation/{userId}
Opens a conversation with a specific user.

### POST /Communications/Chats/SendMessage
Sends a new message.
- **Body**: `{ ReceiverId: Guid, Content: string }`

### GET /Communications/Chats/GetMessages/{userId}
Retrieves messages with pagination.
- **Query**: `page`, `pageSize`

### POST /Communications/Chats/MarkAsRead/{messageId}
Marks a specific message as read.

### POST /Communications/Chats/MarkConversationAsRead/{userId}
Marks all messages in a conversation as read.

### POST /Communications/Chats/DeleteMessage/{messageId}
Deletes a message (only sender can delete).

### GET /Communications/Chats/GetUnreadCount
Gets total unread message count for current user.

## Database Schema

### ChatRooms Table
- `Id` (PK)
- `Name`
- `IsGroup`
- `CreatedBy` (FK to Users)
- `CreatedAt`

### ChatMessages Table
- `Id` (PK)
- `ChatRoomId` (FK to ChatRooms)
- `SenderId` (FK to Users)
- `Content`
- `Type` (enum)
- `AttachmentUrl`
- `IsEdited`
- `EditedAt`
- `ReplyToMessageId` (FK to ChatMessages, nullable)
- `CreatedAt`

### ChatRoomMembers Table
- `Id` (PK)
- `ChatRoomId` (FK to ChatRooms)
- `UserId` (FK to Users)
- `JoinedAt`
- `LastReadAt`
- `IsMuted`

## Setup Instructions

### 1. Run Migration
```bash
dotnet ef migrations add AddChatEntities --project src/CommunityCar.Infrastructure --startup-project src/CommunityCar.Mvc
dotnet ef database update --project src/CommunityCar.Infrastructure --startup-project src/CommunityCar.Mvc
```

### 2. Verify Service Registration
Ensure `ChatService` is registered in `DependencyInjection.cs`:
```csharp
services.AddScoped<IChatService, ChatService>();
```

### 3. Add Navigation Link
Add a link to the chat feature in your layout:
```html
<a href="@Url.Action("Index", "Chats", new { area = "Communications" })">
    <i class="fas fa-comments"></i> Messages
    <span class="badge bg-danger" id="unreadCount"></span>
</a>
```

## Future Enhancements

### Planned Features
1. **Real-time Updates**: Integrate SignalR for instant message delivery
2. **Group Chats**: Support for multi-user conversations
3. **Rich Media**: Image, file, and video sharing
4. **Message Reactions**: Emoji reactions to messages
5. **Message Editing**: Edit sent messages
6. **Typing Indicators**: Show when other user is typing
7. **Online Status**: Real-time online/offline status
8. **Message Search**: Search within conversations
9. **Push Notifications**: Browser/mobile notifications for new messages
10. **Voice/Video Calls**: WebRTC integration

### Technical Improvements
- Implement message caching with Redis
- Add message encryption for privacy
- Optimize database queries with proper indexing
- Implement message pagination with cursor-based approach
- Add rate limiting to prevent spam

## Security Considerations

1. **Authorization**: Users can only access their own conversations
2. **Validation**: Message content is validated and sanitized
3. **Anti-Forgery**: CSRF protection on all POST endpoints
4. **Rate Limiting**: Consider implementing rate limiting for message sending
5. **Content Filtering**: Add profanity filter if needed

## Testing

### Unit Tests
- Test `ChatService` methods
- Test message validation
- Test unread count calculations

### Integration Tests
- Test conversation creation
- Test message sending and receiving
- Test read receipt functionality

### UI Tests
- Test chat interface responsiveness
- Test message sending flow
- Test conversation switching

## Troubleshooting

### Messages Not Appearing
- Check browser console for JavaScript errors
- Verify AJAX endpoints are accessible
- Check database for message records

### Unread Count Not Updating
- Verify `LastReadAt` is being updated correctly
- Check auto-refresh interval in JavaScript
- Verify `GetUnreadCount` endpoint is working

### Performance Issues
- Add database indexes on frequently queried columns
- Implement message pagination
- Consider caching conversation lists

## Support
For issues or questions, please contact the development team or create an issue in the project repository.
