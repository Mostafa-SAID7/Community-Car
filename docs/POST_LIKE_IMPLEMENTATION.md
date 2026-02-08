# Post Like/Unlike Implementation

## Overview
Implemented proper toggle like/unlike functionality for posts with secure tracking of user reactions.

## Requirements Met
✅ **Authentication Required**: User must be authenticated to like/unlike posts  
✅ **Post Validation**: System validates that the post exists before allowing likes  
✅ **Toggle Behavior**: 
- If user already liked → Unlike (remove reaction)
- If user hasn't liked → Like (add reaction)  
✅ **Secure Like Count**: Like count is updated atomically in the database  
✅ **Duplicate Prevention**: Unique index prevents duplicate likes from same user

## Implementation Details

### 1. PostReaction Entity
**File**: `src/CommunityCar.Domain/Entities/Community/post/PostReaction.cs`

- Tracks individual user reactions to posts
- Properties: PostId, UserId, ReactionType
- Immutable design with private setters
- Uses Guard clauses for validation

### 2. Database Configuration
**File**: `src/CommunityCar.Infrastructure/Data/Configurations/PostReactionConfiguration.cs`

- Table: `PostReactions`
- Unique index on (PostId, UserId) - prevents duplicate likes
- Cascade delete on Post deletion
- Restrict delete on User deletion
- Soft delete query filter applied

### 3. PostService Update
**File**: `src/CommunityCar.Infrastructure/Services/Community/PostService.cs`

Updated `ToggleLikeAsync` method:
```csharp
public async Task ToggleLikeAsync(Guid postId, Guid userId)
{
    // 1. Validate post exists
    var post = await _context.Set<Post>()
        .FirstOrDefaultAsync(p => p.Id == postId);
    
    if (post == null)
        throw new NotFoundException("Post not found");
    
    // 2. Check for existing reaction
    var existingReaction = await _context.Set<PostReaction>()
        .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);
    
    if (existingReaction != null)
    {
        // Unlike - remove reaction and decrement count
        _context.Set<PostReaction>().Remove(existingReaction);
        post.DecrementLikes();
    }
    else
    {
        // Like - add reaction and increment count
        var reaction = new PostReaction(postId, userId, ReactionType.Like);
        await _context.Set<PostReaction>().AddAsync(reaction);
        post.IncrementLikes();
    }
    
    // 3. Save changes atomically
    await _context.SaveChangesAsync();
}
```

### 4. LikePostCommandHandler
**File**: `src/CommunityCar.Infrastructure/Handlers/Community/LikePostCommandHandler.cs`

CQRS command handler that:
- Validates user authentication (UserId != Guid.Empty)
- Validates post existence
- Implements toggle logic
- Returns Result<LikePostResult> with IsLiked status and TotalLikes count
- Handles errors gracefully with proper logging

## Database Schema

```sql
CREATE TABLE [PostReactions] (
    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
    [PostId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [ReactionType] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [IsDeleted] bit NOT NULL,
    -- ... other audit fields
    
    CONSTRAINT [FK_PostReactions_Posts_PostId] 
        FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PostReactions_AspNetUsers_UserId] 
        FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_PostReactions_PostId_UserId] 
    ON [PostReactions] ([PostId], [UserId]);
```

## Security Features

1. **Authentication Check**: UserId validation prevents anonymous likes
2. **Unique Constraint**: Database-level prevention of duplicate likes
3. **Atomic Operations**: All changes saved in single transaction
4. **Soft Delete Support**: Reactions respect soft delete pattern
5. **Cascade Delete**: Reactions automatically removed when post is deleted
6. **Audit Trail**: CreatedAt, ModifiedAt, and user tracking on all reactions

## Testing Checklist

- [ ] Authenticated user can like a post
- [ ] Authenticated user can unlike a previously liked post
- [ ] Like count increments correctly
- [ ] Like count decrements correctly
- [ ] Same user cannot like the same post twice
- [ ] Unauthenticated users cannot like posts
- [ ] Liking non-existent post returns error
- [ ] Like count remains accurate after multiple toggles
- [ ] Deleting a post removes all associated reactions
- [ ] UI updates to reflect like status

## API Endpoints

### Toggle Like
**POST** `/{culture}/Posts/ToggleLike/{id}`

**Authorization**: Required  
**Parameters**: 
- `id` (Guid): Post ID

**Response**:
```json
{
  "success": true,
  "message": "Like toggled"
}
```

## Migration

Migration: `20260208154840_AddPostReactions`  
Status: ✅ Applied to database

## Related Files

- Domain Entity: `src/CommunityCar.Domain/Entities/Community/post/PostReaction.cs`
- Configuration: `src/CommunityCar.Infrastructure/Data/Configurations/PostReactionConfiguration.cs`
- Service: `src/CommunityCar.Infrastructure/Services/Community/PostService.cs`
- Handler: `src/CommunityCar.Infrastructure/Handlers/Community/LikePostCommandHandler.cs`
- Controller: `src/CommunityCar.Mvc/Controllers/Content/PostsController.cs`
- Interface: `src/CommunityCar.Domain/Interfaces/Community/IPostService.cs`

## Notes

- Implementation follows the same pattern as GuideReaction
- Uses existing ReactionType enum from `CommunityCar.Domain.Enums.Community.qa`
- Post entity already had `DecrementLikes()` method - no changes needed
- Configuration automatically discovered via `ApplyConfigurationsFromAssembly`
