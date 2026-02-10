# Architecture Refactoring Plan

## Overview
Refactoring the CommunityCar application to follow best practices with centralized patterns for data access and real-time communication.

## Goals
1. **Replace direct `ApplicationDbContext` usage with `IUnitOfWork` pattern**
2. **Use Repository pattern consistently**
3. **Create a generic centralized SignalR Hub**
4. **Improve testability, maintainability, and scalability**

---

## Part 1: Enhanced Unit of Work Pattern

### Current State
- Services directly inject `ApplicationDbContext`
- Manual `SaveChangesAsync()` calls in each service
- No centralized transaction management
- Difficult to test and mock

### Target State
- Services inject `IUnitOfWork`
- Centralized transaction and change tracking management
- Repository access through UnitOfWork
- Easy to test with mocked dependencies

### Implementation

#### 1.1 Enhanced IUnitOfWork Interface
```csharp
public interface IUnitOfWork : IDisposable
{
    // Repository access
    IRepository<T> Repository<T>() where T : class, IEntity;
    
    // Transaction management
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    
    // Change tracking
    void ClearTracker();
    
    // Direct DbContext access (for complex queries when needed)
    ApplicationDbContext Context { get; }
}
```

#### 1.2 Enhanced UnitOfWork Implementation
- Lazy repository instantiation
- Transaction support
- Repository caching per UnitOfWork instance

---

## Part 2: Service Refactoring

### Services to Refactor
1. ✅ ReviewService
2. EventService
3. GuideService
4. NewsService
5. MapService
6. PostService
7. QuestionService
8. FriendshipService
9. SecurityAlertService
10. HealthService
11. AuditLogService
12. KPIService
13. LocalizationService
14. SettingsService
15. ContentActivityService

### Refactoring Pattern

**Before:**
```csharp
public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<ReviewService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Review> CreateReviewAsync(...)
    {
        var review = new Review(...);
        _context.Set<Review>().Add(review);
        await _context.SaveChangesAsync();
        return review;
    }
}
```

**After:**
```csharp
public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<ReviewService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Review> CreateReviewAsync(...)
    {
        var review = new Review(...);
        await _uow.Repository<Review>().AddAsync(review);
        await _uow.SaveChangesAsync();
        return review;
    }
}
```

---

## Part 3: Generic Centralized SignalR Hub

### Current State
- Multiple specific hubs: `FriendHub`, `PostHub`, `ChatHub`, `NotificationHub`, `QuestionHub`
- Duplicate code for connection management
- Duplicate code for user tracking
- Hard to maintain and extend

### Target State
- Single generic `CommunityHub` with typed message routing
- Centralized connection management
- Event-based architecture
- Easy to extend with new notification types

### Hub Architecture

#### 3.1 Generic Community Hub
```csharp
public class CommunityHub : Hub
{
    // Centralized connection tracking
    // Generic message routing
    // Event-based notifications
}
```

#### 3.2 Hub Event Types
- `Friend` events (requests, accepts, blocks, etc.)
- `Post` events (likes, comments, shares, etc.)
- `Chat` events (messages, typing, read receipts)
- `Notification` events (system, user, content)
- `Question` events (answers, votes, etc.)
- `Review` events (new reviews, helpful votes)
- `Group` events (joins, leaves, posts)

#### 3.3 Hub Service Interface
```csharp
public interface IHubNotificationService
{
    Task NotifyUserAsync<T>(Guid userId, string eventType, T data);
    Task NotifyUsersAsync<T>(List<Guid> userIds, string eventType, T data);
    Task NotifyGroupAsync<T>(string groupName, string eventType, T data);
    Task BroadcastAsync<T>(string eventType, T data);
}
```

---

## Part 4: Migration Strategy

### Phase 1: Infrastructure Setup (Priority 1)
1. ✅ Enhance IUnitOfWork interface
2. ✅ Implement enhanced UnitOfWork
3. ✅ Update DependencyInjection registration
4. ✅ Create IHubNotificationService interface
5. ✅ Implement CommunityHub
6. ✅ Implement HubNotificationService

### Phase 2: Service Refactoring (Priority 2)
1. ✅ Refactor ReviewService (template for others)
2. Refactor remaining services one by one
3. Update unit tests
4. Integration testing

### Phase 3: Hub Migration (Priority 3)
1. ✅ Create CommunityHub
2. Update all Hub calls to use CommunityHub
3. Deprecate old hubs
4. Remove old hubs after verification

### Phase 4: Testing & Validation (Priority 4)
1. Unit tests for UnitOfWork
2. Unit tests for refactored services
3. Integration tests for Hub
4. Performance testing
5. Load testing

---

## Benefits

### Data Access Layer
- ✅ **Testability**: Easy to mock IUnitOfWork
- ✅ **Maintainability**: Centralized data access logic
- ✅ **Transaction Management**: Built-in transaction support
- ✅ **Performance**: Repository caching per request
- ✅ **Consistency**: Standardized data access patterns

### SignalR Hub
- ✅ **Maintainability**: Single hub to maintain
- ✅ **Extensibility**: Easy to add new event types
- ✅ **Consistency**: Standardized notification patterns
- ✅ **Performance**: Optimized connection management
- ✅ **Scalability**: Ready for Redis backplane

---

## Implementation Order

1. ✅ Create enhanced IUnitOfWork and implementation
2. ✅ Create IHubNotificationService and CommunityHub
3. ✅ Refactor ReviewService as template
4. Refactor other services (15 services)
5. Update all Hub usage to CommunityHub
6. Create comprehensive tests
7. Documentation and training

---

## Files to Create/Modify

### New Files
- ✅ `src/CommunityCar.Domain/Interfaces/Common/IUnitOfWork.cs` (enhance)
- ✅ `src/CommunityCar.Infrastructure/Uow/Common/UnitOfWork.cs` (enhance)
- ✅ `src/CommunityCar.Domain/Interfaces/Communications/IHubNotificationService.cs`
- ✅ `src/CommunityCar.Infrastructure/Hubs/CommunityHub.cs`
- ✅ `src/CommunityCar.Infrastructure/Services/Communications/HubNotificationService.cs`
- ✅ `.agent/workflows/architecture-refactoring-plan.md`

### Modified Files
- ✅ `src/CommunityCar.Infrastructure/Services/Community/ReviewService.cs`
- `src/CommunityCar.Infrastructure/Services/Community/EventService.cs`
- `src/CommunityCar.Infrastructure/Services/Community/GuideService.cs`
- `src/CommunityCar.Infrastructure/Services/Community/NewsService.cs`
- ... (all other services)
- ✅ `src/CommunityCar.Infrastructure/DependencyInjection.cs`
- `src/CommunityCar.Mvc/Program.cs` (Hub registration)

---

## Status: IN PROGRESS

**Current Phase**: Phase 1 - Infrastructure Setup
**Next Steps**: 
1. Enhance IUnitOfWork interface
2. Implement enhanced UnitOfWork
3. Create CommunityHub and HubNotificationService
4. Refactor ReviewService as template
