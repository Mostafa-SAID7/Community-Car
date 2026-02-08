# Q&A Voting Feature - Command Handler Implementation

## Overview
Implemented a robust, idempotent voting system for Questions and Answers using the Command Handler pattern. The system ensures users can only have ONE vote per Q&A item and handles all vote scenarios correctly.

## Vote Logic

### Scenarios

| Current State | User Action | Result | Score Change |
|--------------|-------------|---------|--------------|
| **No Vote** | Vote Up | Add Upvote | **+1** |
| **No Vote** | Vote Down | Add Downvote | **-1** |
| **Upvoted** | Vote Up | Remove Upvote | **-1** (back to 0) |
| **Downvoted** | Vote Down | Remove Downvote | **+1** (back to 0) |
| **Upvoted** | Vote Down | Switch to Downvote | **-2** |
| **Downvoted** | Vote Up | Switch to Upvote | **+2** |

### Key Principles

1. **One Vote Per User**: Each user can only have ONE vote (up or down) per question/answer
2. **Idempotent**: Clicking the same vote button twice removes the vote
3. **Toggle Behavior**: Clicking opposite vote switches the vote
4. **Atomic Operations**: Vote record and score updated in single transaction

## Architecture

### 1. Domain Layer

**Commands** (`src/CommunityCar.Domain/Commands/Community/VoteQuestionCommand.cs`)
```csharp
public class VoteQuestionCommand : ICommand<VoteResult>
{
    public Guid QuestionId { get; }
    public Guid UserId { get; }
    public bool IsUpvote { get; }
}

public class VoteAnswerCommand : ICommand<VoteResult>
{
    public Guid AnswerId { get; }
    public Guid UserId { get; }
    public bool IsUpvote { get; }
}

public class VoteResult
{
    public bool? CurrentVote { get; set; }  // true=up, false=down, null=none
    public int TotalScore { get; set; }      // Current score
    public VoteAction Action { get; set; }   // Added/Removed/Switched
}

public enum VoteAction
{
    Added,      // No vote → Vote added
    Removed,    // Same vote → Vote removed
    Switched    // Different vote → Vote switched
}
```

### 2. Infrastructure Layer

**VoteQuestionCommandHandler** (`src/CommunityCar.Infrastructure/Handlers/Community/VoteQuestionCommandHandler.cs`)
- Validates user authentication
- Validates question existence
- Implements three-scenario logic:
  1. **No existing vote** → Create new vote (+1 or -1)
  2. **Same vote exists** → Remove vote (0)
  3. **Different vote exists** → Toggle vote (-2 or +2)
- Updates question score atomically
- Returns current state

**VoteAnswerCommandHandler** (`src/CommunityCar.Infrastructure/Handlers/Community/VoteAnswerCommandHandler.cs`)
- Same logic as VoteQuestionCommandHandler but for answers
- Validates answer existence
- Updates answer score atomically

### 3. Existing Entities

**QuestionVote** (`src/CommunityCar.Domain/Entities/Community/qa/QuestionVote.cs`)
```csharp
public class QuestionVote : BaseEntity
{
    public Guid QuestionId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsUpvote { get; private set; }
    
    public void Toggle() => IsUpvote = !IsUpvote;
}
```

**AnswerVote** (`src/CommunityCar.Domain/Entities/Community/qa/AnswerVote.cs`)
```csharp
public class AnswerVote : BaseEntity
{
    public Guid AnswerId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsUpvote { get; private set; }
    
    public void Toggle() => IsUpvote = !IsUpvote;
}
```

## Database Schema

Tables already exist with proper constraints:

```sql
CREATE TABLE [QuestionVotes] (
    [Id] uniqueidentifier PRIMARY KEY,
    [QuestionId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [IsUpvote] bit NOT NULL,
    -- BaseEntity fields
    [CreatedAt] datetimeoffset NOT NULL,
    [IsDeleted] bit NOT NULL,
    -- Foreign Keys
    CONSTRAINT [FK_QuestionVotes_Questions] FOREIGN KEY ([QuestionId]) 
        REFERENCES [Questions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_QuestionVotes_Users] FOREIGN KEY ([UserId]) 
        REFERENCES [AspNetUsers] ([Id])
);

-- Unique constraint: one vote per user per question
CREATE UNIQUE INDEX [IX_QuestionVotes_QuestionId_UserId] 
    ON [QuestionVotes] ([QuestionId], [UserId]);

-- Same structure for AnswerVotes
```

## API Response Format

### Success Response
```json
{
  "success": true,
  "currentVote": true,      // true=upvoted, false=downvoted, null=no vote
  "totalScore": 42,         // Current score (upvotes - downvotes)
  "action": "Switched",     // "Added", "Removed", or "Switched"
  "message": "Vote recorded"
}
```

### Error Response
```json
{
  "success": false,
  "message": "Question not found"
}
```

## Usage Example

### Controller Integration
```csharp
[Authorize]
[HttpPost("VoteQuestion/{id:guid}")]
public async Task<IActionResult> VoteQuestion(Guid id, [FromBody] VoteRequest request)
{
    var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
    var command = new VoteQuestionCommand(id, userId, request.IsUpvote);
    var result = await _voteQuestionHandler.HandleAsync(command);

    if (!result.IsSuccess)
    {
        return Json(new { success = false, message = result.Error });
    }

    return Json(new
    {
        success = true,
        currentVote = result.Value.CurrentVote,
        totalScore = result.Value.TotalScore,
        action = result.Value.Action.ToString(),
        message = "Vote recorded"
    });
}
```

### Frontend (JavaScript)
```javascript
async function voteQuestion(questionId, isUpvote) {
    const url = CultureHelper.buildUrl(`/Questions/Vote/${questionId}`);
    
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ isUpvote })
    });
    
    const result = await response.json();
    
    if (result.success) {
        // Update UI based on result.currentVote
        const upBtn = document.querySelector(`#upvote-${questionId}`);
        const downBtn = document.querySelector(`#downvote-${questionId}`);
        const scoreEl = document.querySelector(`#score-${questionId}`);
        
        // Clear both buttons
        upBtn.classList.remove('active');
        downBtn.classList.remove('active');
        
        // Set active state based on current vote
        if (result.currentVote === true) {
            upBtn.classList.add('active');
        } else if (result.currentVote === false) {
            downBtn.classList.add('active');
        }
        
        // Update score
        scoreEl.textContent = result.totalScore;
        
        // Show action feedback
        console.log(`Vote ${result.action}: Score is now ${result.totalScore}`);
    }
}
```

## Security Features

1. **Authentication Required**: Only authenticated users can vote
2. **Entity Validation**: Ensures question/answer exists before voting
3. **One Vote Per User**: Database unique constraint enforces this
4. **Atomic Operations**: Vote and score updated in single transaction
5. **Idempotent**: Same request produces same result (safe to retry)
6. **Soft Delete Support**: Inherits from BaseEntity

## Testing Scenarios

### Scenario 1: No Vote → Upvote
- **Initial**: No vote, Score = 0
- **Action**: User clicks upvote
- **Result**: Vote added, Score = 1
- **Response**: `currentVote=true, action=Added`

### Scenario 2: Upvote → Upvote (Remove)
- **Initial**: Upvoted, Score = 1
- **Action**: User clicks upvote again
- **Result**: Vote removed, Score = 0
- **Response**: `currentVote=null, action=Removed`

### Scenario 3: Upvote → Downvote (Switch)
- **Initial**: Upvoted, Score = 1
- **Action**: User clicks downvote
- **Result**: Vote switched, Score = -1
- **Response**: `currentVote=false, action=Switched`

### Scenario 4: Downvote → Upvote (Switch)
- **Initial**: Downvoted, Score = -1
- **Action**: User clicks upvote
- **Result**: Vote switched, Score = 1
- **Response**: `currentVote=true, action=Switched`

### Scenario 5: Downvote → Downvote (Remove)
- **Initial**: Downvoted, Score = -1
- **Action**: User clicks downvote again
- **Result**: Vote removed, Score = 0
- **Response**: `currentVote=null, action=Removed`

### Scenario 6: No Vote → Downvote
- **Initial**: No vote, Score = 0
- **Action**: User clicks downvote
- **Result**: Vote added, Score = -1
- **Response**: `currentVote=false, action=Added`

## Testing Checklist

- [x] User can upvote a question/answer
- [x] User can downvote a question/answer
- [x] Clicking same vote button removes the vote
- [x] Clicking opposite vote button switches the vote
- [x] Score calculation is correct for all scenarios
- [x] Only one vote per user per Q&A item
- [x] Unauthenticated users cannot vote
- [x] Cannot vote on non-existent questions/answers
- [x] Operations are idempotent
- [x] Atomic transactions ensure data consistency

## Benefits of This Implementation

1. **Clean Separation**: Business logic isolated in command handlers
2. **Testable**: Easy to unit test without dependencies
3. **Maintainable**: Clear, single-responsibility classes
4. **Consistent**: Follows same pattern as LikePostCommandHandler
5. **Idempotent**: Safe to retry, no duplicate votes
6. **Atomic**: Score and vote always in sync
7. **Extensible**: Easy to add notifications, analytics, etc.

## Future Enhancements

1. **Vote Notifications**: Notify author when their Q&A receives votes
2. **Vote Analytics**: Track voting patterns and trends
3. **Rate Limiting**: Prevent vote spam
4. **Vote History**: Track vote changes over time
5. **Reputation System**: Award points based on votes received
6. **Vote Reversal Detection**: Detect and handle suspicious voting patterns

## Dependencies

- Entity Framework Core 9.0
- ASP.NET Core Identity
- Microsoft.Extensions.Logging
- Existing QuestionVote and AnswerVote entities
