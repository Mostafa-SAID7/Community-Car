# Q&A Voting System - Complete Logic Flow

## Overview
The voting system implements idempotent, soft-delete-aware voting logic for Questions and Answers. Each user can have one vote per Q&A item, and votes can be toggled or switched.

## Vote States
A vote can be in one of three states:
1. **No Vote** - User has never voted, or all previous votes are soft-deleted
2. **Active Vote** - User has an active upvote or downvote
3. **Soft-Deleted Vote** - User previously voted but removed it (audit trail preserved)

## Voting Scenarios

### 1. No Vote → Upvote (+1)
```
User State: No vote exists
Action: Click upvote
Result: Create new vote (IsUpvote=true)
Score Change: +1
Database: INSERT new QuestionVote/AnswerVote
```

### 2. No Vote → Downvote (-1)
```
User State: No vote exists
Action: Click downvote
Result: Create new vote (IsUpvote=false)
Score Change: -1
Database: INSERT new QuestionVote/AnswerVote
```

### 3. Upvote → Upvote (Remove, 0)
```
User State: Active upvote
Action: Click upvote again
Result: Soft delete vote
Score Change: -1 (removes the +1)
Database: UPDATE IsDeleted=true, DeletedAt=now
```

### 4. Downvote → Downvote (Remove, 0)
```
User State: Active downvote
Action: Click downvote again
Result: Soft delete vote
Score Change: +1 (removes the -1)
Database: UPDATE IsDeleted=true, DeletedAt=now
```

### 5. Upvote → Downvote (-2)
```
User State: Active upvote
Action: Click downvote
Result: Switch vote direction
Score Change: -2 (remove +1, add -1)
Database: UPDATE IsUpvote=false, ModifiedAt=now
```

### 6. Downvote → Upvote (+2)
```
User State: Active downvote
Action: Click upvote
Result: Switch vote direction
Score Change: +2 (remove -1, add +1)
Database: UPDATE IsUpvote=true, ModifiedAt=now
```

### 7. Soft-Deleted Upvote → Upvote (+1)
```
User State: Soft-deleted upvote exists
Action: Click upvote
Result: Resurrect vote (same direction)
Score Change: +1
Database: UPDATE IsDeleted=false, DeletedAt=null, ModifiedAt=now
```

### 8. Soft-Deleted Downvote → Downvote (-1)
```
User State: Soft-deleted downvote exists
Action: Click downvote
Result: Resurrect vote (same direction)
Score Change: -1
Database: UPDATE IsDeleted=false, DeletedAt=null, ModifiedAt=now
```

### 9. Soft-Deleted Upvote → Downvote (-1)
```
User State: Soft-deleted upvote exists
Action: Click downvote
Result: Resurrect and switch vote
Score Change: -1
Database: UPDATE IsDeleted=false, DeletedAt=null, IsUpvote=false, ModifiedAt=now
```

### 10. Soft-Deleted Downvote → Upvote (+1)
```
User State: Soft-deleted downvote exists
Action: Click upvote
Result: Resurrect and switch vote
Score Change: +1
Database: UPDATE IsDeleted=false, DeletedAt=null, IsUpvote=true, ModifiedAt=now
```

## Handler Logic Flow

```
VoteQuestionCommandHandler.HandleAsync()
│
├─ Validate: User authenticated?
├─ Validate: Question exists?
│
├─ Query: Check existing vote (including soft-deleted)
│   └─ .IgnoreQueryFilters() to see soft-deleted votes
│
├─ Decision Tree:
│  │
│  ├─ No vote exists?
│  │  └─ CreateVoteAsync() → INSERT new vote
│  │
│  ├─ Soft-deleted vote exists?
│  │  └─ ResurrectOrChangeVoteAsync()
│  │     ├─ Same direction? → Resurrect only
│  │     └─ Different direction? → Resurrect + Toggle
│  │
│  ├─ Active vote, same direction?
│  │  └─ SoftDeleteVoteAsync() → UPDATE IsDeleted=true
│  │
│  └─ Active vote, different direction?
│     └─ ChangeVoteAsync() → UPDATE IsUpvote=!IsUpvote
│
├─ Update Question.VoteCount (atomic)
├─ SaveChangesAsync() (transaction)
└─ Return VoteResult
```

## Database Constraints

### Unique Constraint
```sql
-- Ensures one vote per user per question/answer
UNIQUE INDEX IX_QuestionVotes_QuestionId_UserId (QuestionId, UserId)
UNIQUE INDEX IX_AnswerVotes_AnswerId_UserId (AnswerId, UserId)
```

This constraint includes soft-deleted votes, preventing duplicate vote records.

### Soft Delete Query Filter
```csharp
// Applied globally in ApplicationDbContext
modelBuilder.Entity<QuestionVote>()
    .HasQueryFilter(v => !v.IsDeleted);
```

Normal queries automatically exclude soft-deleted votes. Use `.IgnoreQueryFilters()` to include them.

## Idempotency Guarantee

The system is **idempotent** - repeating the same vote action produces the same result:

- Click upvote twice → First adds vote, second removes it
- Click upvote three times → Adds, removes, adds again
- Switch from up to down → Always results in downvote, regardless of how many times clicked

## Audit Trail

All vote changes are tracked:
- **CreatedAt**: When vote was first created
- **ModifiedAt**: When vote direction was changed
- **DeletedAt**: When vote was soft-deleted
- **IsDeleted**: Current soft-delete status

This provides complete audit history of user voting behavior.

## Score Calculation

Question/Answer VoteCount is updated atomically in the same transaction:

```csharp
question.UpdateVoteCount(delta);
await _context.SaveChangesAsync();
```

Score deltas:
- Add vote: +1 or -1
- Remove vote: -1 or +1 (opposite of original)
- Switch vote: +2 or -2 (remove old + add new)
- Resurrect same: +1 or -1
- Resurrect different: +1 or -1 (net effect)

## Error Handling

Validation failures return `Result.Failure<VoteResult>`:
- User not authenticated
- Question/Answer not found
- Database errors

All operations are wrapped in try-catch with logging.

## Performance Considerations

1. **Single Query**: Check for existing vote (active or soft-deleted) in one query
2. **Atomic Updates**: Score changes happen in same transaction as vote changes
3. **Indexed Lookups**: Unique constraint provides fast vote lookups
4. **No Cascading**: Vote changes don't trigger other operations

## Testing Scenarios

See `docs/QA_VOTE_TEST_SCENARIOS.md` for comprehensive test cases covering all 10 scenarios.
