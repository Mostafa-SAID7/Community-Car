# Q&A Vote Feature - Idempotent Voting System

## Overview
Implemented an idempotent voting system for Questions and Answers using the Command Handler pattern. The system supports upvotes, downvotes, vote removal, and vote switching with accurate score calculation.

## Voting Logic

### Score Calculation Table

| Current State | Action | Score Change | New State | Description |
|--------------|--------|--------------|-----------|-------------|
| No Vote | Upvote | +1 | Upvoted | User adds upvote |
| No Vote | Downvote | -1 | Downvoted | User adds downvote |
| Upvoted | Upvote | -1 (remove) | No Vote | User removes upvote |
| Downvoted | Downvote | +1 (remove) | No Vote | User removes downvote |
| Upvoted | Downvote | -2 | Downvoted | User switches from up to down |
| Downvoted | Upvote | +2 | Upvoted | User switches from down to up |

### Why Only One Vote Per User?

**Integrity**: Prevents vote manipulation and ensures fair scoring  
**Simplicity**: Clear user intent - you either like it, dislike it, or are neutral  
**Standard Practice**: Follows Stack Overflow, Reddit, and other Q&A platforms  
**Database Constraint**: Unique index on (QuestionId/AnswerId, UserId) enforces this

## Architecture

### 1. Domain Layer

**Commands** (`src/CommunityCar.Domain/Commands/Community/VoteQuestionCommand.cs`)
- `VoteQuestionCommand`: Vote on a question
- `VoteAnswerCommand`: Vote on an answer
- `VoteResult`: Contains current vote state, total score, score delta, and action type
- `VoteAction` enum: Added, Removed, Switched

**Entities** (Already exist)
- `QuestionVote`: Tracks user votes on questions
- `AnswerVote`: Tracks user votes on answers
- Both have `Toggle()` method for switching votes

### 2. Infrastructure Layer

**VoteQuestionCommandHandler** (`src/CommunityCar.Infrastructure/Handlers/Community/VoteQuestionCommandHandler.cs`)

Implements four scenarios with soft delete support:

1. **CreateVoteAsync** (No existing vote)
   ```csharp
   - Create new QuestionVote
   - Update score: +1 (upvote) or -1 (downvote)
   - Return: CurrentVote = true/false, Action = Added
   ```

2. **SoftDeleteVoteAsync** (Same vote clicked again)
   ```csharp
   - Soft delete existing QuestionVote (IsDeleted = true)
   - Update score: -1 (was upvote) or +1 (was downvote)
   - Return: CurrentVote = null, Action = Removed
   - Maintains audit trail for compliance
   ```

3. **ChangeVoteAsync** (Different vote clicked)
   ```csharp
   - Toggle existing QuestionVote
   - Update ModifiedAt timestamp
   - Update score: +2 (down→up) or -2 (up→down)
   - Return: CurrentVote = new state, Action = Switched
   ```

4. **ResurrectOrChangeVoteAsync** (Soft-deleted vote exists)
   ```csharp
   - Resurrect soft-deleted vote (IsDeleted = false)
   - If same vote: Just resurrect, Delta = +1/-1
   - If different vote: Resurrect and toggle, Delta = +1/-1
   - Update ModifiedAt timestamp
   - Return: CurrentVote = new state, Action = Added or Switched
   ```

**VoteAnswerCommandHandler** (`src/CommunityCar.Infrastructure/Handlers/Community/VoteAnswerCommandHandler.cs`)
- Same logic as VoteQuestionCommandHandler but for answers

### Soft Delete Benefits

1. **Audit Trail**: Complete history of all voting actions
2. **Compliance**: Meets data retention requirements
3. **Analytics**: Track voting patterns over time
4. **Recovery**: Can restore accidentally removed votes
5. **Debugging**: Easier to troubleshoot voting issues

### 3. Database Schema

```sql
-- QuestionVotes table (already exists)
CREATE TABLE [QuestionVotes] (
    [Id] uniqueidentifier PRIMARY KEY,
    [QuestionId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [IsUpvote] bit NOT NULL,
    -- BaseEntity fields
    [CreatedAt] datetimeoffset NOT NULL,
    [IsDeleted] bit NOT NULL,
    -- Foreign Keys
    CONSTRAINT [FK_QuestionVotes_Questions] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_QuestionVotes_Users] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
);

-- Unique constraint: one vote per user per question
CREATE UNIQUE INDEX [IX_QuestionVotes_QuestionId_UserId] ON [QuestionVotes] ([QuestionId], [UserId]);

-- AnswerVotes table (already exists)
CREATE TABLE [AnswerVotes] (
    [Id] uniqueidentifier PRIMARY KEY,
    [AnswerId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [IsUpvote] bit NOT NULL,
    -- BaseEntity fields
    [CreatedAt] datetimeoffset NOT NULL,
    [IsDeleted] bit NOT NULL,
    -- Foreign Keys
    CONSTRAINT [FK_AnswerVotes_Answers] FOREIGN KEY ([AnswerId]) REFERENCES [Answers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AnswerVotes_Users] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
);

-- Unique constraint: one vote per user per answer
CREATE UNIQUE INDEX [IX_AnswerVotes_AnswerId_UserId] ON [AnswerVotes] ([AnswerId], [UserId]);
```

## API Response Format

### Success Response
```json
{
  "success": true,
  "currentVote": true,      // true=upvoted, false=downvoted, null=no vote
  "totalScore": 42,         // Updated score
  "scoreDelta": 1,          // Change from this operation
  "action": "Added",        // Added, Removed, or Switched
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

## Idempotent Operations

The voting system is **idempotent**, meaning:

1. **Same Request = Same Result**: Clicking upvote twice removes the upvote (doesn't add it twice)
2. **No Duplicate Votes**: Database constraint prevents multiple votes from same user
3. **Atomic Transactions**: Vote record and score updated together
4. **Predictable Behavior**: Users always know what will happen when they click

### Example Scenarios

**Scenario 1: User upvotes a question**
```
Initial: No vote, Score = 10
Action: Click upvote
Result: Upvoted, Score = 11, Delta = +1
```

**Scenario 2: User clicks upvote again (toggle off)**
```
Initial: Upvoted, Score = 11
Action: Click upvote
Result: No vote, Score = 10, Delta = -1
```

**Scenario 3: User switches from upvote to downvote**
```
Initial: Upvoted, Score = 11
Action: Click downvote
Result: Downvoted, Score = 9, Delta = -2
```

**Scenario 4: User switches from downvote to upvote**
```
Initial: Downvoted, Score = 9
Action: Click upvote
Result: Upvoted, Score = 11, Delta = +2
```

## Security Features

1. **Authentication Required**: Only authenticated users can vote
2. **Entity Validation**: Ensures question/answer exists before voting
3. **Unique Constraint**: Database prevents duplicate votes
4. **Atomic Operations**: Vote and score updated in single transaction
5. **Soft Delete**: Uses soft delete instead of hard delete for audit trail
6. **Audit Trail**: CreatedAt, ModifiedAt, DeletedAt tracking
7. **Query Filters**: Uses IgnoreQueryFilters() to check for soft-deleted votes

## Usage Example

### Controller Integration
```csharp
[Authorize]
[HttpPost("VoteQuestion/{id:guid}")]
public async Task<IActionResult> VoteQuestion(Guid id, [FromBody] VoteRequest request)
{
    var command = new VoteQuestionCommand(id, GetCurrentUserId(), request.IsUpvote);
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
        scoreDelta = result.Value.ScoreDelta,
        action = result.Value.Action.ToString()
    });
}
```

### Frontend (JavaScript)
```javascript
async function voteQuestion(questionId, isUpvote) {
    const url = CultureHelper.buildUrl(`/Questions/Vote/${questionId}`);
    
    const response = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ isUpvote })
    });
    
    const result = await response.json();
    
    if (result.success) {
        // Update UI based on result.action
        updateVoteButtons(questionId, result.currentVote);
        updateScore(questionId, result.totalScore);
        
        // Show feedback
        if (result.action === 'Added') {
            showMessage(`${isUpvote ? 'Upvoted' : 'Downvoted'}!`);
        } else if (result.action === 'Removed') {
            showMessage('Vote removed');
        } else if (result.action === 'Switched') {
            showMessage(`Switched to ${isUpvote ? 'upvote' : 'downvote'}`);
        }
    }
}
```

## Testing Checklist

- [x] User can upvote a question/answer
- [x] User can downvote a question/answer
- [x] Clicking same vote removes it (toggle)
- [x] Clicking opposite vote switches it
- [x] Score calculation is correct for all scenarios
- [x] Unauthenticated users cannot vote
- [x] Cannot vote on non-existent questions/answers
- [x] Database prevents duplicate votes
- [x] Atomic transaction ensures consistency
- [x] Idempotent operations work correctly

## Benefits

1. **User-Friendly**: Clear, predictable behavior
2. **Secure**: Authentication and validation at every step
3. **Accurate**: Score always reflects actual votes
4. **Efficient**: Single database transaction per vote
5. **Maintainable**: Clean separation of concerns
6. **Testable**: Business logic isolated in handlers
7. **Scalable**: Optimized queries with proper indexes

## Future Enhancements

1. **Vote Notifications**: Notify authors when their content is voted on
2. **Vote Analytics**: Track voting patterns and trends
3. **Rate Limiting**: Prevent vote spam
4. **Vote History**: Show user's voting history
5. **Reputation System**: Award points based on votes received
6. **Vote Reasons**: Allow users to explain downvotes (optional)

## Dependencies

- Entity Framework Core 9.0
- ASP.NET Core Identity
- Microsoft.Extensions.Logging
- Existing QuestionVote and AnswerVote entities
