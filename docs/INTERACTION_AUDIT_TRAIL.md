# Interaction Audit Trail - Domain Events Architecture

## Overview
Comprehensive audit trail system for all user interactions: Likes, Comments, Bookmarks, and Ratings. Built on Domain-Driven Design (DDD) patterns with Domain Events for observability, testability, and traceability.

## Architecture Components

### 1. Domain Events (Observable)

#### Like Events
- **LikeCreatedEvent** - User likes content (Post, News, Guide, MapPoint, etc.)
- **LikeRemovedEvent** - User unlikes content

#### Comment Events
- **CommentCreatedEvent** - User creates comment or reply
- **CommentUpdatedEvent** - User edits comment
- **CommentDeletedEvent** - User soft-deletes comment

#### Bookmark Events
- **BookmarkCreatedEvent** - User bookmarks content (Question, Post, Guide, etc.)
- **BookmarkRemovedEvent** - User removes bookmark

#### Rating Events
- **RatingCreatedEvent** - User rates content (MapPoint, Review, etc.)
- **RatingUpdatedEvent** - User changes rating value
- **RatingRemovedEvent** - User removes rating

### 2. Event Handlers

#### LikeAuditEventHandler
Handles like/unlike events and creates audit logs:

```csharp
public class LikeAuditEventHandler :
    IDomainEventHandler<LikeCreatedEvent>,
    IDomainEventHandler<LikeRemovedEvent>
{
    // Creates AuditLog entries with:
    // - EntityName (PostLike, NewsLike, GuideLike, etc.)
    // - EntityId (Like ID)
    // - Action (Created/Deleted)
    // - UserId
    // - NewValues (JSON with like details)
    // - AffectedColumns (LikeCount)
}
```

#### CommentAuditEventHandler
Handles comment lifecycle events:

```csharp
public class CommentAuditEventHandler :
    IDomainEventHandler<CommentCreatedEvent>,
    IDomainEventHandler<CommentUpdatedEvent>,
    IDomainEventHandler<CommentDeletedEvent>
{
    // Tracks:
    // - Comment creation (including replies)
    // - Content updates (old vs new)
    // - Soft deletes
    // - Comment counts
}
```

#### BookmarkAuditEventHandler
Handles bookmark events:

```csharp
public class BookmarkAuditEventHandler :
    IDomainEventHandler<BookmarkCreatedEvent>,
    IDomainEventHandler<BookmarkRemovedEvent>
{
    // Tracks user bookmarking behavior
    // Useful for content recommendation algorithms
}
```

#### RatingAuditEventHandler
Handles rating events:

```csharp
public class RatingAuditEventHandler :
    IDomainEventHandler<RatingCreatedEvent>,
    IDomainEventHandler<RatingUpdatedEvent>,
    IDomainEventHandler<RatingRemovedEvent>
{
    // Tracks:
    // - Initial ratings
    // - Rating changes (old vs new value)
    // - Rating removals
    // - Average rating updates
}
```

## Audit Log Examples

### Like Created
```json
{
  "EntityName": "PostLike",
  "EntityId": "like-guid",
  "Action": "Created",
  "Description": "User liked post",
  "UserId": "user-guid",
  "OldValues": null,
  "NewValues": {
    "LikeId": "like-guid",
    "EntityId": "post-guid",
    "EntityType": "Post",
    "NewLikeCount": 42
  },
  "AffectedColumns": "LikeCount",
  "CreatedAt": "2026-02-08T15:30:00Z"
}
```

### Comment Created (Reply)
```json
{
  "EntityName": "PostComment",
  "EntityId": "comment-guid",
  "Action": "Created",
  "Description": "User replied to comment on post",
  "UserId": "user-guid",
  "OldValues": null,
  "NewValues": {
    "CommentId": "comment-guid",
    "EntityId": "post-guid",
    "EntityType": "Post",
    "Content": "Great post! I totally agree with...",
    "ParentCommentId": "parent-comment-guid",
    "IsReply": true,
    "NewCommentCount": 15
  },
  "AffectedColumns": "Content,CommentCount",
  "CreatedAt": "2026-02-08T15:31:00Z"
}
```

### Comment Updated
```json
{
  "EntityName": "PostComment",
  "EntityId": "comment-guid",
  "Action": "Updated",
  "Description": "User updated comment on post",
  "UserId": "user-guid",
  "OldValues": {
    "Content": "Original comment text..."
  },
  "NewValues": {
    "CommentId": "comment-guid",
    "EntityId": "post-guid",
    "EntityType": "Post",
    "Content": "Updated comment text..."
  },
  "AffectedColumns": "Content,ModifiedAt",
  "CreatedAt": "2026-02-08T15:35:00Z"
}
```

### Bookmark Created
```json
{
  "EntityName": "QuestionBookmark",
  "EntityId": "bookmark-guid",
  "Action": "Created",
  "Description": "User bookmarked question",
  "UserId": "user-guid",
  "OldValues": null,
  "NewValues": {
    "BookmarkId": "bookmark-guid",
    "EntityId": "question-guid",
    "EntityType": "Question"
  },
  "AffectedColumns": "BookmarkCount",
  "CreatedAt": "2026-02-08T15:32:00Z"
}
```

### Rating Created
```json
{
  "EntityName": "MapPointRating",
  "EntityId": "rating-guid",
  "Action": "Created",
  "Description": "User rated mappoint with 5 stars",
  "UserId": "user-guid",
  "OldValues": null,
  "NewValues": {
    "RatingId": "rating-guid",
    "EntityId": "mappoint-guid",
    "EntityType": "MapPoint",
    "RatingValue": 5,
    "NewAverageRating": 4.7,
    "NewRatingCount": 23
  },
  "AffectedColumns": "RatingValue,AverageRating,RatingCount",
  "CreatedAt": "2026-02-08T15:33:00Z"
}
```

### Rating Updated
```json
{
  "EntityName": "MapPointRating",
  "EntityId": "rating-guid",
  "Action": "Updated",
  "Description": "User changed rating on mappoint from 5 to 4 stars",
  "UserId": "user-guid",
  "OldValues": {
    "RatingValue": 5
  },
  "NewValues": {
    "RatingId": "rating-guid",
    "EntityId": "mappoint-guid",
    "EntityType": "MapPoint",
    "RatingValue": 4,
    "NewAverageRating": 4.6
  },
  "AffectedColumns": "RatingValue,AverageRating,ModifiedAt",
  "CreatedAt": "2026-02-08T15:40:00Z"
}
```

## Integration with Command Handlers

### Example: LikePostCommandHandler
```csharp
public class LikePostCommandHandler : ICommandHandler<LikePostCommand, LikePostResult>
{
    private readonly IDomainEventHandler<LikeCreatedEvent> _likeCreatedHandler;
    private readonly IDomainEventHandler<LikeRemovedEvent> _likeRemovedHandler;

    public async Task<Result<LikePostResult>> HandleAsync(
        LikePostCommand command, 
        CancellationToken cancellationToken = default)
    {
        // ... business logic ...
        
        // Save changes atomically
        await _context.SaveChangesAsync(cancellationToken);
        
        // Dispatch domain events for audit trail
        await DispatchDomainEventsAsync(isLiked, likeId, post, command, result, cancellationToken);
        
        return Result<LikePostResult>.Success(result);
    }
}
```

## Analytics Queries

### Most Liked Content (Last 24h)
```sql
SELECT 
    JSON_VALUE(NewValues, '$.EntityId') as EntityId,
    JSON_VALUE(NewValues, '$.EntityType') as EntityType,
    COUNT(*) as LikeCount
FROM AuditLogs
WHERE EntityName LIKE '%Like'
  AND Action = 'Created'
  AND CreatedAt > DATEADD(hour, -24, GETDATE())
GROUP BY 
    JSON_VALUE(NewValues, '$.EntityId'),
    JSON_VALUE(NewValues, '$.EntityType')
ORDER BY LikeCount DESC;
```

### Most Active Commenters
```sql
SELECT 
    UserId,
    COUNT(*) as CommentCount
FROM AuditLogs
WHERE EntityName LIKE '%Comment'
  AND Action = 'Created'
  AND CreatedAt > DATEADD(day, -7, GETDATE())
GROUP BY UserId
ORDER BY CommentCount DESC;
```

### User Bookmarking Patterns
```sql
SELECT 
    JSON_VALUE(NewValues, '$.EntityType') as ContentType,
    COUNT(*) as BookmarkCount
FROM AuditLogs
WHERE EntityName LIKE '%Bookmark'
  AND Action = 'Created'
  AND UserId = @UserId
GROUP BY JSON_VALUE(NewValues, '$.EntityType');
```

### Average Rating Trends
```sql
SELECT 
    JSON_VALUE(NewValues, '$.EntityId') as EntityId,
    AVG(CAST(JSON_VALUE(NewValues, '$.RatingValue') as FLOAT)) as AvgRating,
    COUNT(*) as RatingCount
FROM AuditLogs
WHERE EntityName LIKE '%Rating'
  AND Action IN ('Created', 'Updated')
GROUP BY JSON_VALUE(NewValues, '$.EntityId')
HAVING COUNT(*) >= 5
ORDER BY AvgRating DESC;
```

### Comment Edit Frequency
```sql
SELECT 
    UserId,
    COUNT(*) as EditCount
FROM AuditLogs
WHERE EntityName LIKE '%Comment'
  AND Action = 'Updated'
  AND CreatedAt > DATEADD(day, -30, GETDATE())
GROUP BY UserId
ORDER BY EditCount DESC;
```

## Benefits

### 1. Observable
- Every interaction raises a domain event
- Multiple handlers can subscribe to same event
- Decoupled architecture
- Easy to add new handlers (notifications, analytics, ML)

### 2. Testable
- Event handlers can be tested independently
- Mock handlers for unit tests
- Integration tests verify event dispatch
- Audit logs provide test verification

### 3. Traceable
- Complete history of all interactions
- JSON-serialized values for comparison
- User attribution for every action
- Timestamp for every change
- Supports compliance (GDPR, SOC 2)

### 4. Analytics-Ready
- User engagement metrics
- Content popularity tracking
- Recommendation algorithms
- Trending content detection
- User behavior analysis
- A/B testing support

## Use Cases

### Content Recommendation
```csharp
// Find similar content based on user bookmarks
var userBookmarks = await _context.Set<AuditLog>()
    .Where(a => a.UserId == userId &&
                a.EntityName.EndsWith("Bookmark") &&
                a.Action == "Created")
    .Select(a => new {
        EntityType = EF.Functions.JsonValue(a.NewValues, "$.EntityType"),
        EntityId = EF.Functions.JsonValue(a.NewValues, "$.EntityId")
    })
    .ToListAsync();
```

### Engagement Scoring
```csharp
// Calculate user engagement score
var engagementScore = await _context.Set<AuditLog>()
    .Where(a => a.UserId == userId &&
                a.CreatedAt > DateTimeOffset.UtcNow.AddDays(-30))
    .GroupBy(a => a.EntityName)
    .Select(g => new {
        InteractionType = g.Key,
        Count = g.Count()
    })
    .ToListAsync();

// Weights: Comment=5, Like=1, Bookmark=3, Rating=2
var score = engagementScore.Sum(e => 
    e.InteractionType.Contains("Comment") ? e.Count * 5 :
    e.InteractionType.Contains("Like") ? e.Count * 1 :
    e.InteractionType.Contains("Bookmark") ? e.Count * 3 :
    e.InteractionType.Contains("Rating") ? e.Count * 2 : 0);
```

### Spam Detection
```csharp
// Detect suspicious comment patterns
var recentComments = await _context.Set<AuditLog>()
    .Where(a => a.UserId == userId &&
                a.EntityName.EndsWith("Comment") &&
                a.Action == "Created" &&
                a.CreatedAt > DateTimeOffset.UtcNow.AddMinutes(-5))
    .CountAsync();

if (recentComments > 10)
{
    // Flag for review - possible spam
}
```

### Content Quality Metrics
```csharp
// Calculate content quality score
var contentMetrics = await _context.Set<AuditLog>()
    .Where(a => EF.Functions.JsonValue(a.NewValues, "$.EntityId") == contentId)
    .GroupBy(a => a.EntityName)
    .Select(g => new {
        MetricType = g.Key,
        Count = g.Count()
    })
    .ToListAsync();

// Quality = (Likes * 1) + (Comments * 2) + (Bookmarks * 3) + (AvgRating * 10)
```

## Registration (DependencyInjection.cs)

```csharp
// Domain Event Handlers - Like
services.AddScoped<LikeAuditEventHandler>();
services.AddScoped<IDomainEventHandler<LikeCreatedEvent>, LikeAuditEventHandler>();
services.AddScoped<IDomainEventHandler<LikeRemovedEvent>, LikeAuditEventHandler>();

// Domain Event Handlers - Comment
services.AddScoped<CommentAuditEventHandler>();
services.AddScoped<IDomainEventHandler<CommentCreatedEvent>, CommentAuditEventHandler>();
services.AddScoped<IDomainEventHandler<CommentUpdatedEvent>, CommentAuditEventHandler>();
services.AddScoped<IDomainEventHandler<CommentDeletedEvent>, CommentAuditEventHandler>();

// Domain Event Handlers - Bookmark
services.AddScoped<BookmarkAuditEventHandler>();
services.AddScoped<IDomainEventHandler<BookmarkCreatedEvent>, BookmarkAuditEventHandler>();
services.AddScoped<IDomainEventHandler<BookmarkRemovedEvent>, BookmarkAuditEventHandler>();

// Domain Event Handlers - Rating
services.AddScoped<RatingAuditEventHandler>();
services.AddScoped<IDomainEventHandler<RatingCreatedEvent>, RatingAuditEventHandler>();
services.AddScoped<IDomainEventHandler<RatingUpdatedEvent>, RatingAuditEventHandler>();
services.AddScoped<IDomainEventHandler<RatingRemovedEvent>, RatingAuditEventHandler>();
```

## Performance Considerations

1. **Async Event Handling** - Events processed asynchronously
2. **Separate Transactions** - Audit logging doesn't block main operation
3. **Indexed Queries** - AuditLog table indexes:
   - UserId
   - EntityName
   - CreatedAt
   - JSON computed columns for EntityId, EntityType
4. **Content Truncation** - Comment content limited to 100 chars in audit logs
5. **Batch Processing** - Consider batching audit log inserts for high-volume scenarios

## Future Enhancements

1. **Real-time Notifications** - Notify users when their content is liked/commented
2. **Machine Learning** - Predict user preferences from interaction patterns
3. **Recommendation Engine** - Suggest content based on similar users' interactions
4. **Gamification** - Award badges/points for engagement milestones
5. **Content Moderation** - Auto-flag suspicious interaction patterns
6. **A/B Testing** - Track interaction metrics for feature experiments
7. **Event Streaming** - Stream events to analytics platforms (Kafka, Azure Event Hub)
8. **CQRS Read Models** - Materialized views for fast analytics queries

## Compliance & Security

- **GDPR Compliance** - Complete audit trail for data access and modifications
- **Right to be Forgotten** - Audit logs can be anonymized when user deleted
- **SOC 2 Compliance** - All changes tracked with user attribution
- **Forensics** - Investigate suspicious patterns (spam, abuse, manipulation)
- **Data Retention** - Configurable retention policies for audit logs
- **Privacy** - Sensitive content truncated in audit logs

## Summary

The interaction audit trail system provides:
- ✅ Complete observability of all user interactions
- ✅ Testable and maintainable architecture
- ✅ Traceable history for compliance and analytics
- ✅ Foundation for advanced features (recommendations, ML, gamification)
- ✅ Decoupled design for easy extensibility
- ✅ Production-ready with performance optimizations
