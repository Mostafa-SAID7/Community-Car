# Q&A Vote System - Test Scenarios

## Test Scenario Matrix

### Question Voting Scenarios

| Test # | Initial State | User Action | Expected Score Change | Expected New State | Expected Action | Notes |
|--------|--------------|-------------|----------------------|-------------------|----------------|-------|
| Q1 | No vote (Score: 10) | Click Upvote | +1 | Upvoted (Score: 11) | Added | First upvote |
| Q2 | No vote (Score: 10) | Click Downvote | -1 | Downvoted (Score: 9) | Added | First downvote |
| Q3 | Upvoted (Score: 11) | Click Upvote | -1 | No vote (Score: 10) | Removed | Toggle off upvote |
| Q4 | Downvoted (Score: 9) | Click Downvote | +1 | No vote (Score: 10) | Removed | Toggle off downvote |
| Q5 | Upvoted (Score: 11) | Click Downvote | -2 | Downvoted (Score: 9) | Switched | Switch from up to down |
| Q6 | Downvoted (Score: 9) | Click Upvote | +2 | Upvoted (Score: 11) | Switched | Switch from down to up |

### Answer Voting Scenarios

| Test # | Initial State | User Action | Expected Score Change | Expected New State | Expected Action | Notes |
|--------|--------------|-------------|----------------------|-------------------|----------------|-------|
| A1 | No vote (Score: 5) | Click Upvote | +1 | Upvoted (Score: 6) | Added | First upvote |
| A2 | No vote (Score: 5) | Click Downvote | -1 | Downvoted (Score: 4) | Added | First downvote |
| A3 | Upvoted (Score: 6) | Click Upvote | -1 | No vote (Score: 5) | Removed | Toggle off upvote |
| A4 | Downvoted (Score: 4) | Click Downvote | +1 | No vote (Score: 5) | Removed | Toggle off downvote |
| A5 | Upvoted (Score: 6) | Click Downvote | -2 | Downvoted (Score: 4) | Switched | Switch from up to down |
| A6 | Downvoted (Score: 4) | Click Upvote | +2 | Upvoted (Score: 6) | Switched | Switch from down to up |

## Detailed Test Cases

### Test Case Q1: First Upvote
**Setup:**
- Question exists with VoteCount = 10
- User has no existing vote

**Action:**
```csharp
var command = new VoteQuestionCommand(questionId, userId, isUpvote: true);
var result = await handler.HandleAsync(command);
```

**Expected Result:**
```json
{
  "currentVote": true,
  "totalScore": 11,
  "scoreDelta": 1,
  "action": "Added"
}
```

**Database State:**
- New QuestionVote record created with IsUpvote = true
- Question.VoteCount = 11

---

### Test Case Q3: Toggle Off Upvote
**Setup:**
- Question exists with VoteCount = 11
- User has existing upvote (QuestionVote with IsUpvote = true)

**Action:**
```csharp
var command = new VoteQuestionCommand(questionId, userId, isUpvote: true);
var result = await handler.HandleAsync(command);
```

**Expected Result:**
```json
{
  "currentVote": null,
  "totalScore": 10,
  "scoreDelta": -1,
  "action": "Removed"
}
```

**Database State:**
- QuestionVote record deleted
- Question.VoteCount = 10

---

### Test Case Q5: Switch from Upvote to Downvote
**Setup:**
- Question exists with VoteCount = 11
- User has existing upvote (QuestionVote with IsUpvote = true)

**Action:**
```csharp
var command = new VoteQuestionCommand(questionId, userId, isUpvote: false);
var result = await handler.HandleAsync(command);
```

**Expected Result:**
```json
{
  "currentVote": false,
  "totalScore": 9,
  "scoreDelta": -2,
  "action": "Switched"
}
```

**Database State:**
- QuestionVote record updated with IsUpvote = false
- Question.VoteCount = 9

---

## Edge Cases

### EC1: Unauthenticated User
**Setup:**
- UserId = Guid.Empty

**Expected:**
- Result.IsSuccess = false
- Error message: "User must be authenticated"

---

### EC2: Non-existent Question
**Setup:**
- QuestionId does not exist in database

**Expected:**
- Result.IsSuccess = false
- Error message: "Question not found"

---

### EC3: Concurrent Votes
**Setup:**
- Two requests from same user at same time

**Expected:**
- Database unique constraint prevents duplicate
- One request succeeds, other may fail or be idempotent

---

### EC4: Rapid Toggle
**Setup:**
- User clicks upvote 5 times rapidly

**Expected:**
- Final state alternates: up, none, up, none, up
- Score changes: +1, -1, +1, -1, +1
- All operations are idempotent

---

## Integration Test Example

```csharp
[Fact]
public async Task VoteQuestion_NoVoteToUpvote_AddsVoteAndIncrementsScore()
{
    // Arrange
    var questionId = Guid.NewGuid();
    var userId = Guid.NewGuid();
    var question = new Question("Test", "Content", userId);
    question.UpdateVoteCount(10); // Initial score
    
    await _context.Questions.AddAsync(question);
    await _context.SaveChangesAsync();
    
    var command = new VoteQuestionCommand(questionId, userId, isUpvote: true);
    
    // Act
    var result = await _handler.HandleAsync(command);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Value.CurrentVote);
    Assert.Equal(11, result.Value.TotalScore);
    Assert.Equal(1, result.Value.ScoreDelta);
    Assert.Equal(VoteAction.Added, result.Value.Action);
    
    // Verify database
    var vote = await _context.QuestionVotes
        .FirstOrDefaultAsync(v => v.QuestionId == questionId && v.UserId == userId);
    Assert.NotNull(vote);
    Assert.True(vote.IsUpvote);
    
    var updatedQuestion = await _context.Questions.FindAsync(questionId);
    Assert.Equal(11, updatedQuestion.VoteCount);
}
```

## Manual Testing Checklist

### Question Voting
- [ ] Can upvote a question (no existing vote)
- [ ] Can downvote a question (no existing vote)
- [ ] Clicking upvote twice removes the upvote
- [ ] Clicking downvote twice removes the downvote
- [ ] Can switch from upvote to downvote
- [ ] Can switch from downvote to upvote
- [ ] Score updates correctly for all scenarios
- [ ] UI reflects current vote state
- [ ] Unauthenticated users cannot vote
- [ ] Cannot vote on deleted questions

### Answer Voting
- [ ] Can upvote an answer (no existing vote)
- [ ] Can downvote an answer (no existing vote)
- [ ] Clicking upvote twice removes the upvote
- [ ] Clicking downvote twice removes the downvote
- [ ] Can switch from upvote to downvote
- [ ] Can switch from downvote to upvote
- [ ] Score updates correctly for all scenarios
- [ ] UI reflects current vote state
- [ ] Unauthenticated users cannot vote
- [ ] Cannot vote on deleted answers

### Database Integrity
- [ ] Unique constraint prevents duplicate votes
- [ ] Vote and score updated atomically
- [ ] Soft delete works correctly
- [ ] Foreign key constraints enforced
- [ ] Indexes improve query performance

### UI/UX
- [ ] Vote buttons show current state
- [ ] Score updates immediately
- [ ] Feedback message shows action taken
- [ ] Loading state during vote
- [ ] Error messages are clear
- [ ] Works on mobile devices

## Performance Tests

### P1: Vote Response Time
**Target:** < 200ms for 95th percentile

**Test:**
- Measure time from request to response
- Include database query and update time

---

### P2: Concurrent Votes
**Target:** Handle 100 concurrent votes without errors

**Test:**
- Simulate 100 users voting simultaneously
- Verify all votes processed correctly
- Check for race conditions

---

### P3: Database Load
**Target:** < 5 queries per vote operation

**Test:**
- Monitor SQL queries during vote
- Optimize if necessary

---

## Security Tests

### S1: Authentication
- [ ] Unauthenticated requests rejected
- [ ] Invalid user ID rejected
- [ ] Cannot vote as another user

### S2: Authorization
- [ ] Can only vote once per question/answer
- [ ] Cannot manipulate vote count directly
- [ ] Cannot vote on own questions/answers (optional)

### S3: Input Validation
- [ ] Invalid question/answer ID handled
- [ ] Malformed requests rejected
- [ ] SQL injection prevented

## Regression Tests

After any changes, verify:
- [ ] All 6 question voting scenarios still work
- [ ] All 6 answer voting scenarios still work
- [ ] Score calculation remains accurate
- [ ] No duplicate votes possible
- [ ] Idempotent behavior maintained
