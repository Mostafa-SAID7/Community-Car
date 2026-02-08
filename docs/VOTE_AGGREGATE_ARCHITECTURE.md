# Vote Aggregate Architecture

## Overview
Enterprise-grade voting system using Domain-Driven Design (DDD) with Aggregate Root pattern, Domain Events, CQRS, and comprehensive audit trails.

## Architecture Layers

### 1. Domain Layer (Pure Business Logic)

#### Aggregate Root: VoteAggregate
```csharp
public class VoteAggregate : AggregateRoot
{
    // Encapsulates vote business logic
    // Raises domain events for all state changes
    // Enforces business rules
}
```

**Responsibilities:**
- Manage vote lifecycle (create, switch, remove, resurrect)
- Raise domain events for observability
- Enforce business invariants
- Calculate score deltas

**Domain Events:**
- `VoteCreatedEvent` - New vote added
- `VoteSwitchedEvent` - Vote direction changed
- `VoteRemovedEvent` - Vote soft-deleted
- `VoteResurrectedEvent` - Soft-deleted vote restored

### 2. Application Layer (Use Cases)

#### Generic Command Handler
```csharp
VoteCommandHandler : ICommandHandler<VoteCommand, VoteResult>
```

**Features:**
- **Generic**: Works for Questions, Answers, Posts, any voteable entity
- **Idempotent**: Same request = same result
- **Transactional**: Atomic updates with domain events
- **Extensible**: Easy to add new voteable entities

**Command:**
```csharp
public record VoteCommand(
    Guid EntityId,
    string EntityType,  // "Question", "Answer", "Post"
    Guid UserId,
    bool IsUpvote
);
```

### 3. Infrastructure Layer (Persistence & Events)

#### Event Handler: VoteEventHandler
```csharp
VoteEventHandler : 
    INotificationHandler<VoteCreatedEvent>,
    INotificationHandler<VoteSwitchedEvent>,
    INotificationHandler<VoteRemovedEvent>,
    INotificationHandler<VoteResurrectedEvent>
```

**Responsibilities:**
- Listen to domain events
- Create audit log entries
- Track OldValues → NewValues
- Enable analytics and compliance

#### Analytics Service: VoteAnalyticsService
```csharp
IVoteAnalyticsService
```

**Capabilities:**
- Vote history queries
- Vote statistics
- Trending entities by vote velocity
- Complete audit trail

## Domain Events Flow

```
User Action
    ↓
VoteCommandHandler
    ↓
VoteAggregate.Method()
    ↓
AddDomainEvent(event)
    ↓
SaveChangesAsync()
    ↓
DispatchDomainEventsAsync()
    ↓
MediatR.Publish(event)
    ↓
VoteEventHandler.Handle(event)
    ↓
Create AuditLog Entry
```

## Audit Trail Structure

Every vote action creates an audit log:

```json
{
  "EntityName": "QuestionVote",
  "EntityId": "vote-guid",
  "Action": "Created|Updated|Deleted",
  "UserId": "user-guid",
  "Timestamp": "2026-02-08T...",
  "OldValues": "{\"IsUpvote\":true}",
  "NewValues": "{\"IsUpvote\":false}",
  "AffectedColumns": "IsUpvote,ModifiedAt",
  "Description": "User switched vote from up to down on Question"
}
```

## Observable, Testable, Traceable

### Observable
- **Domain Events**: Every state change emits an event
- **Structured Logging**: All operations logged with context
- **Audit Trail**: Complete history in database

### Testable
- **Unit Tests**: Test aggregate methods in isolation
- **Integration Tests**: Test command handler with in-memory DB
- **Event Tests**: Verify events are raised correctly

### Traceable
- **Audit Logs**: Who, what, when, old/new values
- **Vote History**: Complete timeline per entity/user
- **Analytics**: Trending, statistics, velocity

## Database Schema

### VoteAggregates Table
```sql
CREATE TABLE VoteAggregates (
    Id uniqueidentifier PRIMARY KEY,
    EntityId uniqueidentifier NOT NULL,
    EntityType nvarchar(50) NOT NULL,
    UserId uniqueidentifier NOT NULL,
    IsUpvote bit NOT NULL,
    ScoreDelta int NOT NULL,
    CreatedAt datetimeoffset NOT NULL,
    ModifiedAt datetimeoffset NULL,
    IsDeleted bit NOT NULL DEFAULT 0,
    DeletedAt datetimeoffset NULL,
    DeletedBy nvarchar(max) NULL,
    RowVersion rowversion NOT NULL,
    
    CONSTRAINT UQ_VoteAggregates_Entity_User 
        UNIQUE (EntityId, EntityType, UserId)
);

-- Indexes for analytics
CREATE INDEX IX_VoteAggregates_EntityId_EntityType 
    ON VoteAggregates(EntityId, EntityType);
    
CREATE INDEX IX_VoteAggregates_UserId 
    ON VoteAggregates(UserId);
    
CREATE INDEX IX_VoteAggregates_EntityType_CreatedAt 
    ON VoteAggregates(EntityType, CreatedAt);
```

### AuditLogs Table (Existing)
```sql
CREATE TABLE AuditLogs (
    Id uniqueidentifier PRIMARY KEY,
    UserId uniqueidentifier NULL,
    UserName nvarchar(max) NULL,
    EntityName nvarchar(max) NOT NULL,
    EntityId nvarchar(max) NOT NULL,
    Action nvarchar(max) NOT NULL,
    Description nvarchar(max) NULL,
    OldValues nvarchar(max) NULL,
    NewValues nvarchar(max) NULL,
    AffectedColumns nvarchar(max) NULL,
    CreatedAt datetimeoffset NOT NULL
);
```

## Analytics Queries

### 1. Vote History
```csharp
await _voteAnalytics.GetEntityVoteHistoryAsync(questionId, "Question");
```

Returns complete vote timeline with all changes.

### 2. Vote Statistics
```csharp
await _voteAnalytics.GetEntityVoteStatisticsAsync(questionId, "Question");
```

Returns:
- Total votes
- Upvote/downvote counts
- Net score
- Upvote percentage
- Deleted vote count
- Switched vote count

### 3. Trending Entities
```csharp
await _voteAnalytics.GetTrendingEntitiesAsync(
    "Question", 
    TimeSpan.FromHours(24), 
    limit: 10);
```

Returns entities with highest vote velocity (votes per hour).

### 4. Audit Trail
```csharp
await _voteAnalytics.GetVoteAuditTrailAsync(
    entityId: questionId,
    userId: userId,
    fromDate: DateTime.UtcNow.AddDays(-7));
```

Returns complete audit history with old/new values.

## Extensibility

### Adding New Voteable Entity

1. **Update VoteCommandHandler.ValidateEntityExistsAsync:**
```csharp
"Review" => await _context.Set<Review>()
    .AnyAsync(e => e.Id == entityId, cancellationToken),
```

2. **Update VoteCommandHandler.GetEntityScoreAsync:**
```csharp
"Review" => await _context.Set<Review>()
    .Where(e => e.Id == entityId)
    .Select(e => e.VoteCount)
    .FirstOrDefaultAsync(cancellationToken),
```

3. **Update VoteCommandHandler.UpdateEntityScoreAsync:**
```csharp
case "Review":
    var review = await _context.Set<Review>()
        .FirstOrDefaultAsync(e => e.Id == entityId, cancellationToken);
    review?.UpdateVoteCount(delta);
    break;
```

That's it! No changes to aggregate, events, or audit logging needed.

## Benefits

### 1. Separation of Concerns
- **Domain**: Pure business logic
- **Application**: Use case orchestration
- **Infrastructure**: Technical concerns

### 2. Single Responsibility
- **Aggregate**: Manage vote state
- **Command Handler**: Coordinate operations
- **Event Handler**: Create audit logs
- **Analytics Service**: Query vote data

### 3. Open/Closed Principle
- Open for extension (new voteable entities)
- Closed for modification (core logic unchanged)

### 4. Dependency Inversion
- Domain doesn't depend on infrastructure
- Infrastructure depends on domain interfaces

### 5. Testability
- Mock dependencies easily
- Test each layer independently
- Verify events without database

### 6. Observability
- Every action logged
- Complete audit trail
- Real-time analytics

### 7. Compliance
- GDPR: Track all data changes
- SOX: Audit trail for financial data
- HIPAA: Healthcare data tracking

## Performance Considerations

### Optimizations
1. **Single Query**: Check existing vote once
2. **Atomic Updates**: Score changes in same transaction
3. **Indexed Lookups**: Fast vote queries
4. **Async/Await**: Non-blocking operations
5. **Batch Events**: Dispatch all events together

### Scalability
1. **Read Replicas**: Analytics queries on read-only DB
2. **Event Sourcing**: Optional for high-volume scenarios
3. **CQRS**: Separate read/write models
4. **Caching**: Cache vote counts (Redis)

## Testing Strategy

### Unit Tests
```csharp
[Fact]
public void VoteAggregate_SwitchVote_RaisesSwitchedEvent()
{
    // Arrange
    var vote = new VoteAggregate(entityId, "Question", userId, true);
    vote.ClearDomainEvents();
    
    // Act
    vote.SwitchVote();
    
    // Assert
    vote.DomainEvents.Should().ContainSingle()
        .Which.Should().BeOfType<VoteSwitchedEvent>();
}
```

### Integration Tests
```csharp
[Fact]
public async Task VoteCommandHandler_CreateVote_CreatesAuditLog()
{
    // Arrange
    var command = new VoteCommand(questionId, "Question", userId, true);
    
    // Act
    await _handler.HandleAsync(command);
    
    // Assert
    var auditLog = await _context.AuditLogs
        .FirstOrDefaultAsync(a => a.EntityName == "QuestionVote");
    auditLog.Should().NotBeNull();
    auditLog.Action.Should().Be("Created");
}
```

## Migration Path

### From Existing Vote Entities

1. **Keep existing entities** (QuestionVote, AnswerVote)
2. **Add VoteAggregate** for new votes
3. **Migrate gradually** or run in parallel
4. **Analytics** work on both systems

### Backward Compatibility

VoteAggregate is additive - doesn't break existing code.

## Conclusion

This architecture provides:
- ✅ Enterprise-grade voting system
- ✅ Complete audit trail
- ✅ Observable, testable, traceable
- ✅ Generic and extensible
- ✅ SOLID principles
- ✅ DDD best practices
- ✅ Production-ready
