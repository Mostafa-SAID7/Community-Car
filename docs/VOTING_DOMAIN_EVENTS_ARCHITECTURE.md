# Voting System - Domain Events & Audit Architecture

## Overview
Enhanced voting system with Domain-Driven Design (DDD) patterns, Domain Events, and comprehensive audit trail for analytics and compliance.

## Architecture Components

### 1. Domain Events (Observable)
Domain events capture every voting action for downstream processing:

- **VoteCreatedEvent** - New vote added
- **VoteChangedEvent** - Vote direction switched (up↔down)
- **VoteRemovedEvent** - Vote soft-deleted
- **VoteResurrectedEvent** - Soft-deleted vote restored

### 2. Aggregate Root Pattern
**VoteAggregate** manages voting operations and raises domain events:

```csharp
public class VoteAggregate : AggregateRoot
{
    // State
    public Guid EntityId { get; }
    public string EntityType { get; } // "Question" or "Answer"
    public Guid UserId { get; }
    public bool IsUpvote { get; }
    public int CurrentScore { get; }
    
    // Operations that raise events
    public static VoteAggregate CreateVote(...) → VoteCreatedEvent
    public void ChangeVote(...) → VoteChangedEvent
    public void RemoveVote(...) → VoteRemovedEvent
    public void ResurrectVote(...) → VoteResurrectedEvent
}
```

### 3. IVotable Interface
Generic interface for votable entities:

```csharp
public interface IVotable
{
    Guid Id { get; }
    int VoteCount { get; }
    void UpdateVoteCount(int delta);
    string GetEntityType();
}
```

Implemented by:
- `Question : AggregateRoot, IVotable`
- `Answer : AggregateRoot, IVotable`

### 4. Generic Vote Command Handler
**VoteCommandHandler<TEntity, TVote>** - Reusable for any votable entity:

```csharp
public class VoteCommandHandler<TEntity, TVote>
    where TEntity : class, IVotable
    where TVote : BaseEntity
{
    // Handles all voting scenarios
    // Raises appropriate domain events
    // Dispatches events to handlers
}
```

### 5. Domain Event Handlers

#### VoteAuditEventHandler
Handles all vote events and creates audit log entries:

```csharp
public class VoteAuditEventHandler :
    IDomainEventHandler<VoteCreatedEvent>,
    IDomainEventHandler<VoteChangedEvent>,
    IDomainEventHandler<VoteRemovedEvent>,
    IDomainEventHandler<VoteResurrectedEvent>
{
    // Creates AuditLog entries with:
    // - EntityName (QuestionVote/AnswerVote)
    // - EntityId (Vote ID)
    // - Action (Created/Updated/Deleted)
    // - UserId
    // - OldValues (JSON)
    // - NewValues (JSON)
    // - AffectedColumns
    // - Timestamp
}
```

## Audit Trail Structure

### AuditLog Entity
```csharp
public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string EntityName { get; set; } // "QuestionVote" or "AnswerVote"
    public string EntityId { get; set; } // Vote ID
    public string Action { get; set; } // "Created", "Updated", "Deleted"
    public string? Description { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? AffectedColumns { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
```

### Audit Log Examples

#### Vote Created
```json
{
  "EntityName": "QuestionVote",
  "EntityId": "vote-guid",
  "Action": "Created",
  "Description": "User voted up on question",
  "OldValues": null,
  "NewValues": {
    "VoteId": "vote-guid",
    "EntityId": "question-guid",
    "EntityType": "Question",
    "IsUpvote": true,
    "ScoreDelta": 1,
    "NewScore": 15
  },
  "AffectedColumns": "IsUpvote,VoteCount"
}
```

#### Vote Changed
```json
{
  "EntityName": "QuestionVote",
  "EntityId": "vote-guid",
  "Action": "Updated",
  "Description": "User changed vote from up to down on question",
  "OldValues": {
    "IsUpvote": true
  },
  "NewValues": {
    "VoteId": "vote-guid",
    "EntityId": "question-guid",
    "EntityType": "Question",
    "IsUpvote": false,
    "ScoreDelta": -2,
    "NewScore": 13
  },
  "AffectedColumns": "IsUpvote,VoteCount,ModifiedAt"
}
```

#### Vote Removed (Soft Delete)
```json
{
  "EntityName": "QuestionVote",
  "EntityId": "vote-guid",
  "Action": "Deleted",
  "Description": "User removed upvote from question",
  "OldValues": {
    "IsUpvote": true,
    "IsDeleted": false
  },
  "NewValues": {
    "VoteId": "vote-guid",
    "EntityId": "question-guid",
    "EntityType": "Question",
    "IsDeleted": true,
    "ScoreDelta": -1,
    "NewScore": 14
  },
  "AffectedColumns": "IsDeleted,DeletedAt,VoteCount"
}
```

#### Vote Resurrected
```json
{
  "EntityName": "QuestionVote",
  "EntityId": "vote-guid",
  "Action": "Updated",
  "Description": "User resurrected upvote on question with direction change",
  "OldValues": {
    "IsDeleted": true
  },
  "NewValues": {
    "VoteId": "vote-guid",
    "EntityId": "question-guid",
    "EntityType": "Question",
    "IsUpvote": false,
    "IsDeleted": false,
    "DirectionChanged": true,
    "ScoreDelta": -1,
    "NewScore": 13
  },
  "AffectedColumns": "IsDeleted,DeletedAt,IsUpvote,ModifiedAt,VoteCount"
}
```

## Benefits

### 1. Observable (Domain Events)
- Every vote action raises an event
- Events can be subscribed to by multiple handlers
- Decoupled architecture - handlers don't know about each other
- Easy to add new event handlers (notifications, analytics, etc.)

### 2. Testable
- Aggregate logic is pure and testable
- Event handlers can be tested independently
- Mock event handlers for unit tests
- Integration tests can verify event dispatch

### 3. Traceable (Audit Trail)
- Complete history of all vote changes
- JSON-serialized old/new values for comparison
- Affected columns tracked
- User attribution for every action
- Timestamp for every change

### 4. Analytics-Ready
Audit logs enable powerful analytics queries:

```sql
-- Vote velocity (trending questions)
SELECT EntityId, COUNT(*) as VoteCount
FROM AuditLogs
WHERE EntityName = 'QuestionVote'
  AND Action = 'Created'
  AND CreatedAt > DATEADD(hour, -24, GETDATE())
GROUP BY EntityId
ORDER BY VoteCount DESC;

-- User voting patterns
SELECT UserId, 
       SUM(CASE WHEN JSON_VALUE(NewValues, '$.IsUpvote') = 'true' THEN 1 ELSE 0 END) as Upvotes,
       SUM(CASE WHEN JSON_VALUE(NewValues, '$.IsUpvote') = 'false' THEN 1 ELSE 0 END) as Downvotes
FROM AuditLogs
WHERE EntityName IN ('QuestionVote', 'AnswerVote')
  AND Action = 'Created'
GROUP BY UserId;

-- Vote changes (indecisive users)
SELECT UserId, COUNT(*) as ChangeCount
FROM AuditLogs
WHERE EntityName IN ('QuestionVote', 'AnswerVote')
  AND Action = 'Updated'
  AND AffectedColumns LIKE '%IsUpvote%'
GROUP BY UserId
ORDER BY ChangeCount DESC;
```

## Event Flow

```
User Action (Vote)
    ↓
VoteQuestionCommandHandler / VoteAnswerCommandHandler
    ↓
Business Logic (Create/Change/Remove/Resurrect)
    ↓
VoteAggregate raises Domain Event
    ↓
SaveChangesAsync() (atomic transaction)
    ↓
DispatchDomainEventsAsync()
    ↓
VoteAuditEventHandler.HandleAsync()
    ↓
Create AuditLog entry
    ↓
SaveChangesAsync() (audit log persisted)
```

## Extensibility

Easy to add new event handlers:

### Example: Vote Notification Handler
```csharp
public class VoteNotificationHandler : IDomainEventHandler<VoteCreatedEvent>
{
    public async Task HandleAsync(VoteCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        if (domainEvent.IsUpvote)
        {
            // Notify question/answer author of upvote
            await _notificationService.NotifyAsync(
                authorId,
                $"Your {domainEvent.EntityType.ToLower()} received an upvote!");
        }
    }
}
```

### Example: Vote Analytics Handler
```csharp
public class VoteAnalyticsHandler : IDomainEventHandler<VoteCreatedEvent>
{
    public async Task HandleAsync(VoteCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        // Track vote in analytics system
        await _analyticsService.TrackEventAsync("vote_created", new
        {
            entity_type = domainEvent.EntityType,
            entity_id = domainEvent.EntityId,
            is_upvote = domainEvent.IsUpvote,
            score_delta = domainEvent.ScoreDelta
        });
    }
}
```

## Registration

```csharp
// DependencyInjection.cs
services.AddScoped<VoteAuditEventHandler>();
services.AddScoped<IDomainEventHandler<VoteCreatedEvent>, VoteAuditEventHandler>();
services.AddScoped<IDomainEventHandler<VoteChangedEvent>, VoteAuditEventHandler>();
services.AddScoped<IDomainEventHandler<VoteRemovedEvent>, VoteAuditEventHandler>();
services.AddScoped<IDomainEventHandler<VoteResurrectedEvent>, VoteAuditEventHandler>();

// Add more handlers as needed
services.AddScoped<IDomainEventHandler<VoteCreatedEvent>, VoteNotificationHandler>();
services.AddScoped<IDomainEventHandler<VoteCreatedEvent>, VoteAnalyticsHandler>();
```

## Query Examples

### Get Vote History for a Question
```csharp
var voteHistory = await _context.Set<AuditLog>()
    .Where(a => a.EntityName == "QuestionVote" && 
                JSON_VALUE(a.NewValues, "$.EntityId") == questionId.ToString())
    .OrderByDescending(a => a.CreatedAt)
    .ToListAsync();
```

### Get User's Vote Activity
```csharp
var userVotes = await _context.Set<AuditLog>()
    .Where(a => a.UserId == userId &&
                (a.EntityName == "QuestionVote" || a.EntityName == "AnswerVote"))
    .OrderByDescending(a => a.CreatedAt)
    .ToListAsync();
```

### Trending Content (Most Votes in Last 24h)
```csharp
var trending = await _context.Set<AuditLog>()
    .Where(a => a.EntityName == "QuestionVote" &&
                a.Action == "Created" &&
                a.CreatedAt > DateTimeOffset.UtcNow.AddHours(-24))
    .GroupBy(a => EF.Functions.JsonValue(a.NewValues, "$.EntityId"))
    .Select(g => new { EntityId = g.Key, VoteCount = g.Count() })
    .OrderByDescending(x => x.VoteCount)
    .Take(10)
    .ToListAsync();
```

## Performance Considerations

1. **Async Event Handling** - Events processed asynchronously
2. **Separate Transactions** - Audit logging doesn't block vote operation
3. **Indexed Queries** - AuditLog table should have indexes on:
   - UserId
   - EntityName
   - EntityId (computed from JSON)
   - CreatedAt
4. **JSON Columns** - SQL Server JSON functions for efficient querying

## Compliance & Security

- **GDPR Compliance** - Complete audit trail for data access
- **SOC 2 Compliance** - All changes tracked with user attribution
- **Forensics** - Investigate suspicious voting patterns
- **Rollback** - Audit trail enables vote history reconstruction

## Future Enhancements

1. **Event Sourcing** - Store events as primary source of truth
2. **CQRS** - Separate read/write models for votes
3. **Real-time Analytics** - Stream events to analytics platform
4. **Machine Learning** - Detect vote manipulation patterns
5. **Reputation System** - Calculate user reputation from vote history
