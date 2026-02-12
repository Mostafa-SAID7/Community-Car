# Services, Repositories, and UnitOfWork Organization Review

## Summary
Reviewed and organized the Infrastructure layer architecture following Clean Architecture and DDD principles.

## Changes Made

### 1. Service Organization

#### Moved Services to Proper Locations
- **AssistantService**: Moved from `Services/` to `Services/AI/`
- **AssistantServiceHelpers**: Moved from `Services/` to `Services/AI/`
- Updated namespaces to `CommunityCar.Infrastructure.Services.AI`

#### Created New Feed Service
- **Interface**: `src/CommunityCar.Domain/Interfaces/Community/IFeedService.cs`
- **Implementation**: `src/CommunityCar.Infrastructure/Services/Community/FeedService.cs`
- **DTOs**: 
  - `src/CommunityCar.Domain/DTOs/Community/FeedItemDto.cs`
  - `src/CommunityCar.Domain/DTOs/Community/FeedResultDto.cs`
- **Mapping**: `src/CommunityCar.Infrastructure/Mappings/Community/feed/FeedProfile.cs`

### 2. Service Structure

```
Services/
├── AI/
│   ├── AssistantService.cs
│   └── AssistantServiceHelpers.cs
├── Common/
│   ├── ConnectionManager.cs
│   ├── CurrentUserService.cs
│   ├── FileStorageService.cs
│   ├── NotificationHubService.cs
│   ├── SecurityService.cs
│   ├── SmsSender.cs
│   └── SmsService.cs
├── Communications/
│   ├── ChatService.cs
│   ├── HubNotificationService.cs
│   └── NotificationService.cs
├── Community/
│   ├── CategoryService.cs
│   ├── ChatHubService.cs
│   ├── EventService.cs
│   ├── FeedService.cs ✨ NEW
│   ├── FriendHubService.cs
│   ├── FriendshipService.cs
│   ├── GroupService.cs
│   ├── GuideService.cs
│   ├── MapService.cs
│   ├── NewsService.cs
│   ├── PostHubService.cs
│   ├── PostService.cs
│   ├── QuestionHubService.cs
│   ├── QuestionService.cs
│   ├── ReviewService.cs
│   └── TagService.cs
├── Dashboard/
│   ├── administration/
│   │   ├── localization/LocalizationService.cs
│   │   ├── security/SecurityAlertService.cs
│   │   └── settings/
│   │       ├── SettingsService.cs
│   │       └── SystemSettingService.cs
│   ├── analytics/
│   │   ├── ContentActivityService.cs
│   │   ├── ReportExportService.cs
│   │   └── UserActivityService.cs
│   ├── monitoring/
│   │   ├── audit/AuditLogService.cs
│   │   ├── health/HealthService.cs
│   │   └── system/SystemService.cs
│   └── overview/
│       ├── DashboardService.cs
│       ├── KPIService.cs
│       └── WidgetService.cs
├── Identity/
│   ├── AuthenticationService.cs
│   └── UserService.cs
└── ML/
    ├── MLPipelineService.cs
    ├── PredictionService.cs
    └── SentimentAnalysisService.cs
```

### 3. Repository Structure

```
Repos/
├── Common/
│   └── Repository.cs (Generic Repository<T>)
└── Identity/
    └── UserRepository.cs
```

**Repository Pattern**:
- Generic `Repository<T>` implements `IRepository<T>`
- Provides standard CRUD operations
- Uses EF Core DbContext
- Accessed through UnitOfWork

### 4. Unit of Work Structure

```
Uow/
└── Common/
    └── UnitOfWork.cs
```

**UnitOfWork Pattern**:
- Manages transactions across multiple repositories
- Provides `Repository<TEntity>()` method for accessing repositories
- Handles `SaveChangesAsync()`, transactions, and change tracking
- Implements `IDisposable` for proper resource cleanup

### 5. Dependency Injection Updates

Updated `DependencyInjection.cs`:
- Changed `using CommunityCar.Infrastructure.Services;` to `using CommunityCar.Infrastructure.Services.AI;`
- Added `services.AddScoped<IFeedService, FeedService>();`

### 6. Controller Updates

**FeedController** (`src/CommunityCar.Mvc/Controllers/Community/FeedController.cs`):
- Moved from `Controllers/Content/` to `Controllers/Community/`
- Updated namespace to `CommunityCar.Mvc.Controllers.Community`
- Injected `IFeedService` and `IMapper`
- Implemented async actions using FeedService
- Added filtering and pagination support

## Architecture Patterns

### Clean Architecture Layers
1. **Domain Layer** (`CommunityCar.Domain`)
   - Interfaces (IFeedService, IRepository, IUnitOfWork)
   - DTOs (FeedItemDto, FeedResultDto)
   - Enums (FeedItemType, DateFilterType, FeedSortType)
   - Entities (Post, Question, Event, etc.)

2. **Infrastructure Layer** (`CommunityCar.Infrastructure`)
   - Service Implementations (FeedService, PostService, etc.)
   - Repository Implementation (Repository<T>)
   - UnitOfWork Implementation
   - Data Access (ApplicationDbContext)
   - AutoMapper Profiles

3. **Presentation Layer** (`CommunityCar.Mvc`)
   - Controllers (FeedController)
   - ViewModels (FeedViewModel, FeedItemViewModel)
   - Views (Index.cshtml)

### Repository Pattern
- Generic repository for common operations
- Specialized repositories when needed (UserRepository)
- Accessed through UnitOfWork for transaction management

### Unit of Work Pattern
- Coordinates work of multiple repositories
- Ensures atomic transactions
- Manages DbContext lifecycle

### Service Layer Pattern
- Business logic encapsulation
- Uses UnitOfWork for data access
- Returns DTOs to controllers
- Organized by feature area (Community, Dashboard, Identity, etc.)

## Best Practices Followed

1. **Separation of Concerns**: Each layer has clear responsibilities
2. **Dependency Inversion**: Controllers depend on interfaces, not implementations
3. **Single Responsibility**: Each service handles one feature area
4. **DRY Principle**: Generic repository eliminates code duplication
5. **Testability**: Services can be easily mocked through interfaces
6. **Maintainability**: Clear folder structure and naming conventions

## Service Dependencies

All services follow this pattern:
```csharp
public class ServiceName : IServiceName
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<ServiceName> _logger;
    
    public ServiceName(IUnitOfWork uow, IMapper mapper, ILogger<ServiceName> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }
}
```

## Next Steps

To complete the Feed feature:
1. Implement remaining feed item types (Events, News, Guides, Reviews, Groups)
2. Add caching for frequently accessed feed data
3. Implement real-time feed updates using SignalR
4. Add user preferences for feed customization
5. Implement feed item recommendations based on user activity

## Files Modified

1. `src/CommunityCar.Infrastructure/Services/AI/AssistantService.cs` - Moved and updated namespace
2. `src/CommunityCar.Infrastructure/Services/AI/AssistantServiceHelpers.cs` - Moved and updated namespace
3. `src/CommunityCar.Infrastructure/DependencyInjection.cs` - Updated service registrations
4. `src/CommunityCar.Mvc/Controllers/Community/FeedController.cs` - Moved and enhanced

## Files Created

1. `src/CommunityCar.Domain/Interfaces/Community/IFeedService.cs`
2. `src/CommunityCar.Domain/DTOs/Community/FeedItemDto.cs`
3. `src/CommunityCar.Domain/DTOs/Community/FeedResultDto.cs`
4. `src/CommunityCar.Infrastructure/Services/Community/FeedService.cs`
5. `src/CommunityCar.Infrastructure/Mappings/Community/feed/FeedProfile.cs`
