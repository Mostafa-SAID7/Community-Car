# Post Like Feature - Toggle Implementation

## Overview
Implemented a secure, toggle-based like system for posts using the Command Handler pattern. Users can like/unlike posts, and the system prevents duplicate likes while maintaining accurate like counts.

## Architecture

### 1. Domain Layer
- **PostReaction Entity** (`src/CommunityCar.Domain/Entities/Community/post/PostReaction.cs`)
  - Tracks individual user reactions to posts
  - Enforces unique constraint: one reaction per user per post
  - Supports different reaction types (Like, Love, etc.)

- **LikePostCommand** (`src/CommunityCar.Domain/Commands/Community/LikePostCommand.cs`)
  - Command object containing PostId and UserId
  - Returns `LikePostResult` with `IsLiked` status and `TotalLikes` count

### 2. Infrastructure Layer
- **LikePostCommandHandler** (`src/CommunityCar.Infrastructure/Handlers/Community/LikePostCommandHandler.cs`)
  - Implements business logic for like/unlike toggle
  - Validates user authentication
  - Validates post existence
  - Checks for existing reaction:
    - If exists → Remove reaction (Unlike)
    - If not exists → Add reaction (Like)
  - Updates post like count atomically
  - Returns result with current state

- **PostReactionConfiguration** (`src/CommunityCar.Infrastructure/Data/Configurations/PostReactionConfiguration.cs`)
  - EF Core configuration
  - Unique index on (PostId, UserId) prevents duplicates
  - Cascade delete when post is deleted

### 3. Service Layer
- **PostService.ToggleLikeAsync** (`src/CommunityCar.Infrastructure/Services/Community/PostService.cs`)
  - Delegates to command handler
  - Returns `LikePostResult` to controller

### 4. Controller Layer
- **PostsController.ToggleLike** (`src/CommunityCar.Mvc/Controllers/Content/PostsController.cs`)
  - Requires authentication
  - Returns JSON with:
    - `success`: Operation status
    - `isLiked`: Current like state (true/false)
    - `totalLikes`: Updated like count
    - `message`: Localized message

## Security Features

1. **Authentication Required**: Only authenticated users can like posts
2. **Post Validation**: Ensures post exists before allowing like
3. **Duplicate Prevention**: Database unique constraint prevents multiple likes
4. **Atomic Operations**: Like count and reaction record updated in single transaction
5. **Soft Delete Support**: Inherits from BaseEntity with soft delete capability

## Database Schema

```sql
CREATE TABLE [PostReactions] (
    [Id] uniqueidentifier PRIMARY KEY,
    [PostId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [ReactionType] nvarchar(max) NOT NULL,
    -- BaseEntity fields
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    [IsDeleted] bit NOT NULL,
    -- Foreign Keys
    CONSTRAINT [FK_PostReactions_Posts] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PostReactions_Users] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
);

-- Unique constraint: one reaction per user per post
CREATE UNIQUE INDEX [IX_PostReactions_PostId_UserId] ON [PostReactions] ([PostId], [UserId]);
CREATE INDEX [IX_PostReactions_UserId] ON [PostReactions] ([UserId]);
```

## API Response

### Success Response
```json
{
  "success": true,
  "isLiked": true,
  "totalLikes": 42,
  "message": "Like toggled"
}
```

### Error Response
```json
{
  "success": false,
  "message": "Post not found"
}
```

## Usage Example

### Frontend (JavaScript)
```javascript
async function toggleLike(postId) {
    const url = CultureHelper.buildUrl(`/Posts/ToggleLike/${postId}`);
    
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    });
    
    const result = await response.json();
    
    if (result.success) {
        // Update UI
        const likeButton = document.querySelector(`#like-btn-${postId}`);
        likeButton.classList.toggle('liked', result.isLiked);
        
        const likeCount = document.querySelector(`#like-count-${postId}`);
        likeCount.textContent = result.totalLikes;
    }
}
```

## Testing Checklist

- [x] User can like a post they haven't liked before
- [x] User can unlike a post they previously liked
- [x] Like count increments correctly
- [x] Like count decrements correctly
- [x] Unauthenticated users cannot like posts
- [x] Cannot like non-existent posts
- [x] Database prevents duplicate likes
- [x] Atomic transaction ensures data consistency
- [x] Proper error handling and logging

## Future Enhancements

1. **Multiple Reaction Types**: Extend beyond "Like" to include Love, Laugh, Sad, etc.
2. **Real-time Updates**: Use SignalR to broadcast like updates to all viewers
3. **Like Notifications**: Notify post author when someone likes their post
4. **Analytics**: Track like patterns and popular content
5. **Rate Limiting**: Prevent spam by limiting likes per user per time period

## Migration

Migration: `20260208154840_AddPostReactions`

To apply:
```bash
dotnet ef database update --project src/CommunityCar.Infrastructure --startup-project src/CommunityCar.Mvc
```

## Dependencies

- Entity Framework Core 9.0
- ASP.NET Core Identity
- AutoMapper
- Microsoft.Extensions.Logging
