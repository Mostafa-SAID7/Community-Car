# Architecture Refactoring Guide

## Overview
This guide documents the refactoring of CommunityCar services from direct `ApplicationDbContext` usage to the **Unit of Work (UoW)** and **Repository** patterns, plus centralization of SignalR Hubs.

---

## ✅ What We've Implemented

### 1. Enhanced Unit of Work Pattern
**Location**: `src/CommunityCar.Domain/Interfaces/Common/IUnitOfWork.cs`

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    void ClearTracker();
}
```

**Benefits**:
- ✅ Centralized transaction management
- ✅ Repository caching per request
- ✅ Easy to mock for testing
- ✅ Consistent data access patterns

### 2. Generic Centralized SignalR Hub
**Location**: `src/CommunityCar.Infrastructure/Hubs/GenericHub.cs`

**Features**:
- Single hub for all real-time communications
- Connection tracking with multi-device support
- Group management
- Generic notification methods
- Backward-compatible specific event handlers

**Benefits**:
- ✅ Single point of maintenance
- ✅ Consistent connection management
- ✅ Easy to extend with new event types
- ✅ Reduced code duplication

### 3. Notification Hub Service
**Location**: `src/CommunityCar.Infrastructure/Services/Common/NotificationHubService.cs`

Provides a clean service layer for sending notifications:
```csharp
public interface INotificationHubService
{
    Task SendToUserAsync(Guid userId, string eventName, object data);
    Task SendToUsersAsync(List<Guid> userIds, string eventName, object data);
    Task SendToGroupAsync(string groupName, string eventName, object data);
    Task BroadcastToAllAsync(string eventName, object data);
    // ... more methods
}
```

---

## 📋 Services to Refactor

### ✅ Completed
1. **ReviewService** - Template implementation

### 🔄 Pending Refactoring

#### Community Services
2. **PostService** - `src/CommunityCar.Infrastructure/Services/Community/PostService.cs`
3. **GuideService** - `src/CommunityCar.Infrastructure/Services/Community/GuideService.cs`
4. **NewsService** - `src/CommunityCar.Infrastructure/Services/Community/NewsService.cs`
5. **EventService** - `src/CommunityCar.Infrastructure/Services/Community/EventService.cs`
6. **MapService** - `src/CommunityCar.Infrastructure/Services/Community/MapService.cs`
7. **FriendshipService** - `src/CommunityCar.Infrastructure/Services/Community/FriendshipService.cs`
8. **QuestionService** - `src/CommunityCar.Infrastructure/Services/Community/QuestionService.cs`

#### Dashboard Services
9. **HealthService** - `src/CommunityCar.Infrastructure/Services/Dashboard/HealthService.cs`
10. **AuditLogService** - `src/CommunityCar.Infrastructure/Services/Dashboard/AuditLogService.cs`
11. **KPIService** - `src/CommunityCar.Infrastructure/Services/Dashboard/KPIService.cs`
12. **SecurityAlertService** - `src/CommunityCar.Infrastructure/Services/Dashboard/SecurityAlertService.cs`
13. **LocalizationService** - `src/CommunityCar.Infrastructure/Services/Dashboard/LocalizationService.cs`
14. **SettingsService** - `src/CommunityCar.Infrastructure/Services/Dashboard/SettingsService.cs`
15. **ContentActivityService** - `src/CommunityCar.Infrastructure/Services/Dashboard/ContentActivityService.cs`

#### Common Services
16. **SecurityService** - `src/CommunityCar.Infrastructure/Services/Common/SecurityService.cs`

---

## 🔧 Refactoring Pattern

### Before (Old Pattern)
```csharp
public class ExampleService : IExampleService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ExampleService> _logger;

    public ExampleService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<ExampleService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Example> CreateAsync(Example entity)
    {
        _context.Set<Example>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Example?> GetByIdAsync(Guid id)
    {
        return await _context.Set<Example>()
            .Include(e => e.RelatedEntity)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
```

### After (New Pattern with UoW)
```csharp
public class ExampleService : IExampleService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<ExampleService> _logger;

    public ExampleService(
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<ExampleService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Example> CreateAsync(Example entity)
    {
        await _uow.Repository<Example>().AddAsync(entity);
        await _uow.SaveChangesAsync();
        return entity;
    }

    public async Task<Example?> GetByIdAsync(Guid id)
    {
        // For simple queries
        return await _uow.Repository<Example>()
            .FirstOrDefaultAsync(e => e.Id == id);
        
        // For complex queries with includes, use GetQueryable()
        var query = _uow.Repository<Example>().GetQueryable();
        return await query
            .Include(e => e.RelatedEntity)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
```

---

## 📝 Step-by-Step Refactoring Checklist

For each service:

### 1. Update Constructor
- [ ] Replace `ApplicationDbContext context` with `IUnitOfWork uow`
- [ ] Update field: `_context` → `_uow`
- [ ] Update constructor assignment

### 2. Update Data Access Methods

#### Simple Operations
```csharp
// OLD
_context.Set<Entity>().Add(entity);
await _context.SaveChangesAsync();

// NEW
await _uow.Repository<Entity>().AddAsync(entity);
await _uow.SaveChangesAsync();
```

#### Queries
```csharp
// OLD
var entity = await _context.Set<Entity>()
    .FirstOrDefaultAsync(e => e.Id == id);

// NEW
var entity = await _uow.Repository<Entity>()
    .FirstOrDefaultAsync(e => e.Id == id);
```

#### Complex Queries with Includes
```csharp
// OLD
var query = _context.Set<Entity>()
    .Include(e => e.Related)
    .Where(e => e.Status == Status.Active);

// NEW
var query = _uow.Repository<Entity>().GetQueryable()
    .Include(e => e.Related)
    .Where(e => e.Status == Status.Active);
```

#### Updates
```csharp
// OLD
_context.Set<Entity>().Update(entity);
await _context.SaveChangesAsync();

// NEW
_uow.Repository<Entity>().Update(entity);
await _uow.SaveChangesAsync();
```

#### Deletes
```csharp
// OLD
_context.Set<Entity>().Remove(entity);
await _context.SaveChangesAsync();

// NEW
_uow.Repository<Entity>().Delete(entity);
await _uow.SaveChangesAsync();
```

### 3. Update Transaction Handling
```csharp
// OLD
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // operations
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}

// NEW
await _uow.BeginTransactionAsync();
try
{
    // operations
    await _uow.SaveChangesAsync();
    await _uow.CommitTransactionAsync();
}
catch
{
    await _uow.RollbackTransactionAsync();
    throw;
}
```

### 4. Test the Service
- [ ] Build the project
- [ ] Run unit tests
- [ ] Test CRUD operations
- [ ] Verify transactions work correctly

---

## 🎯 SignalR Hub Migration

### Old Pattern (Multiple Specific Hubs)
```csharp
// FriendHub, PostHub, ChatHub, NotificationHub, QuestionHub
public class FriendHub : Hub
{
    private static readonly Dictionary<Guid, string> _userConnections = new();
    // ... duplicate connection management code
}
```

### New Pattern (Single Generic Hub)
```csharp
// GenericHub - handles all events
public class GenericHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();
    // ... centralized connection management
    
    // Generic methods
    public Task SendToUser(Guid userId, string eventName, object data);
    public Task SendToGroup(string groupName, string eventName, object data);
    
    // Specific event handlers for backward compatibility
    public Task NotifyFriendRequest(Guid receiverId, object data);
    public Task NotifyNewPost(List<Guid> followerIds, object data);
}
```

### Using NotificationHubService in Services
```csharp
public class ExampleService
{
    private readonly INotificationHubService _hubService;
    
    public async Task CreatePostAsync(Post post)
    {
        // ... create post logic
        
        // Send real-time notification
        await _hubService.NotifyNewPostAsync(followerIds, new
        {
            PostId = post.Id,
            Title = post.Title,
            AuthorName = post.AuthorName
        });
    }
}
```

---

## 🚀 Benefits Summary

### Unit of Work Pattern
1. **Testability**: Easy to mock `IUnitOfWork` in unit tests
2. **Transaction Management**: Built-in transaction support
3. **Performance**: Repository caching per request
4. **Consistency**: Standardized data access across all services
5. **Maintainability**: Single point of change for data access logic

### Generic SignalR Hub
1. **Reduced Code**: ~80% less hub code to maintain
2. **Consistency**: Standardized notification patterns
3. **Extensibility**: Easy to add new event types
4. **Performance**: Optimized connection tracking
5. **Scalability**: Ready for Redis backplane

---

## 📊 Progress Tracking

| Service | Status | Priority | Notes |
|---------|--------|----------|-------|
| ReviewService | ✅ Complete | High | Template implementation |
| PostService | 🔄 Pending | High | High usage |
| GuideService | 🔄 Pending | Medium | |
| NewsService | 🔄 Pending | Medium | |
| EventService | 🔄 Pending | Medium | |
| MapService | 🔄 Pending | Low | |
| FriendshipService | 🔄 Pending | High | |
| QuestionService | 🔄 Pending | High | |
| HealthService | 🔄 Pending | Low | |
| AuditLogService | 🔄 Pending | Medium | |
| KPIService | 🔄 Pending | Low | |
| SecurityAlertService | 🔄 Pending | Medium | |
| LocalizationService | 🔄 Pending | Low | |
| SettingsService | 🔄 Pending | Medium | |
| ContentActivityService | 🔄 Pending | Low | |
| SecurityService | 🔄 Pending | High | |

---

## 🧪 Testing Strategy

### Unit Tests
```csharp
public class ExampleServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IRepository<Example>> _mockRepo;
    private readonly ExampleService _service;

    public ExampleServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IRepository<Example>>();
        
        _mockUow.Setup(u => u.Repository<Example>())
            .Returns(_mockRepo.Object);
        
        _service = new ExampleService(_mockUow.Object, ...);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddAndSave()
    {
        // Arrange
        var entity = new Example();
        
        // Act
        await _service.CreateAsync(entity);
        
        // Assert
        _mockRepo.Verify(r => r.AddAsync(entity), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
```

---

## 📚 Additional Resources

- **Repository Pattern**: https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design
- **Unit of Work Pattern**: https://martinfowler.com/eaaCatalog/unitOfWork.html
- **SignalR Best Practices**: https://docs.microsoft.com/en-us/aspnet/core/signalr/scale

---

## 🎓 Next Steps

1. Review this guide
2. Start with high-priority services (PostService, FriendshipService, QuestionService)
3. Follow the refactoring checklist for each service
4. Test thoroughly after each refactoring
5. Update documentation as needed
6. Consider creating automated tests for refactored services

---

**Last Updated**: 2026-02-10
**Status**: Phase 1 Complete - ReviewService refactored as template
