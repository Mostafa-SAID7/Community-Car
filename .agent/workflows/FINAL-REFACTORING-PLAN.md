# Final Refactoring Plan - Complete Migration to IUnitOfWork

## 🎯 Objective
Convert ALL services from direct `ApplicationDbContext` usage to `IUnitOfWork` pattern for proper architecture.

---

## ✅ What's Already Done

### Infrastructure (100% Complete)
1. ✅ **IUnitOfWork Interface** - Enhanced with Repository access, transactions
2. ✅ **UnitOfWork Implementation** - With repository caching and transaction management
3. ✅ **IRepository<T> Interface** - Generic repository pattern
4. ✅ **Repository<T> Implementation** - Generic repository with LINQ support
5. ✅ **GenericHub** - Centralized SignalR hub
6. ✅ **INotificationHubService** - Service interface for notifications
7. ✅ **NotificationHubService** - Implementation for sending notifications
8. ✅ **DependencyInjection** - All services registered
9. ✅ **Program.cs** - Hub endpoints configured

### Services (1/17 Complete)
1. ✅ **ReviewService** - Fully refactored and working

---

## 🔧 Services to Refactor (16 Remaining)

### Community Services (8 services)
1. ⏳ **GuideService** - Partially done, needs completion
2. ⏳ **PostService** - Uses ApplicationDbContext
3. ⏳ **NewsService** - Uses ApplicationDbContext
4. ⏳ **EventService** - Uses ApplicationDbContext
5. ⏳ **MapService** - Uses ApplicationDbContext
6. ⏳ **FriendshipService** - Uses ApplicationDbContext
7. ⏳ **QuestionService** - Uses ApplicationDbContext
8. ⏳ **GroupService** - Check if uses ApplicationDbContext

### Dashboard Services (7 services)
9. ⏳ **HealthService** - Uses ApplicationDbContext
10. ⏳ **AuditLogService** - Uses ApplicationDbContext
11. ⏳ **KPIService** - Uses ApplicationDbContext
12. ⏳ **SecurityAlertService** - Uses ApplicationDbContext
13. ⏳ **LocalizationService** - Uses ApplicationDbContext
14. ⏳ **SettingsService** - Uses ApplicationDbContext
15. ⏳ **ContentActivityService** - Uses ApplicationDbContext

### Common Services (1 service)
16. ⏳ **SecurityService** - Uses ApplicationDbContext

---

## 📋 Refactoring Checklist (Per Service)

For each service, follow these steps:

### Step 1: Update Constructor
```csharp
// OLD
private readonly ApplicationDbContext _context;
public ServiceName(ApplicationDbContext context, ...)
{
    _context = context;
}

// NEW
private readonly IUnitOfWork _uow;
public ServiceName(IUnitOfWork uow, ...)
{
    _uow = uow;
}
```

### Step 2: Update Using Statements
```csharp
// Remove (if exists)
using CommunityCar.Infrastructure.Data;

// Add (if not exists)
using CommunityCar.Domain.Interfaces.Common;
using Microsoft.EntityFrameworkCore; // For Include(), ToListAsync(), etc.
```

### Step 3: Replace Data Access Patterns

#### Pattern A: Simple FirstOrDefaultAsync
```csharp
// OLD
var entity = await _context.Set<Entity>()
    .FirstOrDefaultAsync(e => e.Id == id);

// NEW
var entity = await _uow.Repository<Entity>()
    .FirstOrDefaultAsync(e => e.Id == id);
```

#### Pattern B: Queries with Include
```csharp
// OLD
var entity = await _context.Set<Entity>()
    .Include(e => e.Related)
    .FirstOrDefaultAsync(e => e.Id == id);

// NEW
var query = _uow.Repository<Entity>().GetQueryable();
var entity = await query
    .Include(e => e.Related)
    .FirstOrDefaultAsync(e => e.Id == id);
```

#### Pattern C: Complex Queries
```csharp
// OLD
var query = _context.Set<Entity>()
    .Include(e => e.Related)
    .Where(e => e.Active)
    .OrderBy(e => e.Name);

// NEW
IQueryable<Entity> query = _uow.Repository<Entity>().GetQueryable()
    .Include(e => e.Related)
    .Where(e => e.Active)
    .OrderBy(e => e.Name);
```

#### Pattern D: Add Operations
```csharp
// OLD
_context.Set<Entity>().Add(entity);
await _context.SaveChangesAsync();

// NEW
await _uow.Repository<Entity>().AddAsync(entity);
await _uow.SaveChangesAsync();
```

#### Pattern E: Update Operations
```csharp
// OLD
_context.Set<Entity>().Update(entity);
await _context.SaveChangesAsync();

// NEW
_uow.Repository<Entity>().Update(entity);
await _uow.SaveChangesAsync();
```

#### Pattern F: Delete Operations
```csharp
// OLD
_context.Set<Entity>().Remove(entity);
await _context.SaveChangesAsync();

// NEW
_uow.Repository<Entity>().Delete(entity);
await _uow.SaveChangesAsync();
```

#### Pattern G: AnyAsync
```csharp
// OLD
if (await _context.Set<Entity>().AnyAsync(e => e.Id == id))

// NEW
if (await _uow.Repository<Entity>().CountAsync(e => e.Id == id) > 0)
```

#### Pattern H: CountAsync
```csharp
// OLD
var count = await _context.Set<Entity>().CountAsync(e => e.Active);

// NEW
var count = await _uow.Repository<Entity>().CountAsync(e => e.Active);
```

---

## 🚀 Execution Strategy

### Phase 1: Fix Current Build Errors (Priority 1)
1. ✅ Revert GuideService to original (already done)
2. ✅ Fix ReviewService completely (already done)
3. ⏳ Build and verify no errors

### Phase 2: Refactor Community Services (Priority 2)
Refactor in this order (high usage first):
1. PostService
2. QuestionService
3. FriendshipService
4. GuideService (complete the refactoring)
5. NewsService
6. EventService
7. MapService
8. GroupService (if needed)

### Phase 3: Refactor Dashboard Services (Priority 3)
1. SecurityAlertService
2. AuditLogService
3. HealthService
4. KPIService
5. SettingsService
6. LocalizationService
7. ContentActivityService

### Phase 4: Refactor Common Services (Priority 4)
1. SecurityService

### Phase 5: Final Testing (Priority 5)
1. Build entire solution
2. Run all tests
3. Manual testing of key features
4. Performance testing

---

## 📝 Service Injection Guidelines

### When to Inject Other Services

Some services may need to inject other services. Here are the rules:

#### ✅ DO Inject:
- `IMapper` - For DTO mapping
- `ILogger<T>` - For logging
- `IUnitOfWork` - For data access
- `INotificationHubService` - For real-time notifications
- Other domain services (e.g., `IEmailService`, `IFileStorageService`)

#### ❌ DON'T Inject:
- `ApplicationDbContext` - Use `IUnitOfWork` instead
- Multiple repositories directly - Use `IUnitOfWork.Repository<T>()` instead
- Circular dependencies

#### Example: Service with Multiple Dependencies
```csharp
public class PostService : IPostService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<PostService> _logger;
    private readonly INotificationHubService _hubService;
    private readonly IFileStorageService _fileStorage;

    public PostService(
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<PostService> logger,
        INotificationHubService hubService,
        IFileStorageService fileStorage)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
        _hubService = hubService;
        _fileStorage = fileStorage;
    }
}
```

---

## 🎯 Success Criteria

### Per Service:
- ✅ No `ApplicationDbContext` references
- ✅ All data access through `IUnitOfWork`
- ✅ Builds without errors
- ✅ All tests pass
- ✅ No performance degradation

### Overall:
- ✅ All 17 services refactored
- ✅ Solution builds successfully
- ✅ All tests pass
- ✅ Documentation updated
- ✅ Code review completed

---

## 📊 Progress Tracking

| Service | Status | Errors | Priority | Notes |
|---------|--------|--------|----------|-------|
| ReviewService | ✅ Done | 0 | High | Template for others |
| GuideService | 🔄 Partial | 58 | High | Needs completion |
| PostService | ⏳ Pending | ? | High | High usage |
| QuestionService | ⏳ Pending | ? | High | Core feature |
| FriendshipService | ⏳ Pending | ? | High | Social features |
| NewsService | ⏳ Pending | ? | Medium | |
| EventService | ⏳ Pending | ? | Medium | |
| MapService | ⏳ Pending | ? | Low | |
| GroupService | ⏳ Pending | ? | Medium | Check if needed |
| HealthService | ⏳ Pending | ? | Low | |
| AuditLogService | ⏳ Pending | ? | Medium | |
| KPIService | ⏳ Pending | ? | Low | |
| SecurityAlertService | ⏳ Pending | ? | Medium | |
| LocalizationService | ⏳ Pending | ? | Low | |
| SettingsService | ⏳ Pending | ? | Medium | |
| ContentActivityService | ⏳ Pending | ? | Low | |
| SecurityService | ⏳ Pending | ? | High | Security critical |

**Total Progress**: 1/17 (5.9%)

---

## 🔄 Current Action

**NOW**: Fix build errors and start systematic refactoring of all services.

**NEXT**: Refactor services one by one, testing after each.

---

**Last Updated**: 2026-02-10
**Status**: Ready to execute - Starting systematic refactoring
