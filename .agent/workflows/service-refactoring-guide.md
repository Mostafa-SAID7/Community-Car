# Service Refactoring Guide - From DbContext to UnitOfWork

## Overview
This guide documents the refactoring of all services from direct `ApplicationDbContext` usage to the `IUnitOfWork` pattern for better architecture, testability, and maintainability.

## Refactoring Pattern

### Before (Old Pattern)
```csharp
public class SomeService : ISomeService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SomeService> _logger;

    public SomeService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<SomeService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Entity> CreateAsync(...)
    {
        var entity = new Entity(...);
        _context.Set<Entity>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
}
```

### After (New Pattern)
```csharp
public class SomeService : ISomeService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<SomeService> _logger;

    public SomeService(
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<SomeService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Entity> CreateAsync(...)
    {
        var entity = new Entity(...);
        await _uow.Repository<Entity>().AddAsync(entity);
        await _uow.SaveChangesAsync();
        return entity;
    }
}
```

## Key Changes

### 1. Constructor Injection
- **Old**: `ApplicationDbContext context`
- **New**: `IUnitOfWork uow`

### 2. Field Declaration
- **Old**: `private readonly ApplicationDbContext _context;`
- **New**: `private readonly IUnitOfWork _uow;`

### 3. Adding Entities
- **Old**: `_context.Set<Entity>().Add(entity);`
- **New**: `await _uow.Repository<Entity>().AddAsync(entity);`

### 4. Updating Entities
- **Old**: `_context.Set<Entity>().Update(entity);`
- **New**: `_uow.Repository<Entity>().Update(entity);`

### 5. Deleting Entities
- **Old**: `_context.Set<Entity>().Remove(entity);`
- **New**: `_uow.Repository<Entity>().Delete(entity);`

### 6. Querying Entities
- **Old**: `_context.Set<Entity>().Where(...).ToListAsync();`
- **New**: `_uow.Repository<Entity>().GetQueryable().Where(...).ToList();`

### 7. Finding by ID
- **Old**: `await _context.Set<Entity>().FindAsync(id);`
- **New**: `await _uow.Repository<Entity>().GetByIdAsync(id);`

### 8. First or Default
- **Old**: `await _context.Set<Entity>().FirstOrDefaultAsync(x => x.Id == id);`
- **New**: `await _uow.Repository<Entity>().FirstOrDefaultAsync(x => x.Id == id);`

### 9. Counting
- **Old**: `await _context.Set<Entity>().CountAsync(x => x.Active);`
- **New**: `await _uow.Repository<Entity>().CountAsync(x => x.Active);`

### 10. Saving Changes
- **Old**: `await _context.SaveChangesAsync();`
- **New**: `await _uow.SaveChangesAsync();`

### 11. Complex Queries (When Repository is not enough)
- **Old**: `_context.Set<Entity>().Include(x => x.Related).Where(...)`
- **New**: `_uow.Repository<Entity>().GetQueryable().Include(x => x.Related).Where(...)`

## Services to Refactor

### Priority 1 - Community Services
1. ✅ ReviewService (COMPLETED - Template)
2. ⏳ PostService
3. ⏳ GuideService
4. ⏳ NewsService
5. ⏳ EventService
6. ⏳ MapService
7. ⏳ FriendshipService
8. ⏳ QuestionService

### Priority 2 - Dashboard Services
9. ⏳ SecurityAlertService
10. ⏳ HealthService
11. ⏳ AuditLogService
12. ⏳ KPIService
13. ⏳ LocalizationService
14. ⏳ SettingsService
15. ⏳ ContentActivityService
16. ⏳ UserActivityService

### Priority 3 - Common Services
17. ⏳ SecurityService

## Benefits of UnitOfWork Pattern

### 1. Better Testability
- Easy to mock `IUnitOfWork` in unit tests
- No need to mock `DbContext` and `DbSet<T>`
- Cleaner test setup

### 2. Transaction Management
- Built-in transaction support
- `BeginTransactionAsync()`, `CommitTransactionAsync()`, `RollbackTransactionAsync()`
- Ensures data consistency

### 3. Repository Caching
- Repositories are cached per UnitOfWork instance
- Better performance for multiple operations on same entity type

### 4. Separation of Concerns
- Services don't need to know about EF Core specifics
- Easier to switch ORM if needed
- Cleaner architecture

### 5. Centralized Data Access
- All data access goes through UnitOfWork
- Easier to add logging, caching, or other cross-cutting concerns
- Consistent patterns across all services

## Testing Example

### Before (Hard to Test)
```csharp
[Fact]
public async Task CreateEntity_ShouldAddToDatabase()
{
    // Need to mock DbContext, DbSet, and all EF Core internals
    var mockContext = new Mock<ApplicationDbContext>();
    var mockSet = new Mock<DbSet<Entity>>();
    // ... complex setup
}
```

### After (Easy to Test)
```csharp
[Fact]
public async Task CreateEntity_ShouldAddToDatabase()
{
    // Simple mock of IUnitOfWork
    var mockUow = new Mock<IUnitOfWork>();
    var mockRepo = new Mock<IRepository<Entity>>();
    
    mockUow.Setup(u => u.Repository<Entity>()).Returns(mockRepo.Object);
    mockUow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
    
    var service = new SomeService(mockUow.Object, mapper, logger);
    // ... test logic
}
```

## Migration Checklist

For each service:
- [ ] Replace `ApplicationDbContext` with `IUnitOfWork` in constructor
- [ ] Update field declaration
- [ ] Replace all `_context.Set<T>()` with `_uow.Repository<T>()`
- [ ] Replace `_context.SaveChangesAsync()` with `_uow.SaveChangesAsync()`
- [ ] Update all CRUD operations to use Repository methods
- [ ] Test the service thoroughly
- [ ] Update unit tests to use mocked IUnitOfWork

## Status

**Current Phase**: Refactoring Services
**Completed**: 1/17 services (ReviewService)
**Next**: PostService, GuideService, NewsService

---

Last Updated: 2026-02-10
