# CommunityCar Project Structure Documentation

## Overview
This document provides a complete overview of the CommunityCar project structure, including all folders, files, and their purposes.

## Solution Structure

```
CommunityCar/
├── CommunityCar.Domain/          # Domain Layer - Business Logic & Entities
├── CommunityCar.Infrastructure/  # Infrastructure Layer - Data Access & External Services
├── CommunityCar.Web/            # Presentation Layer - ASP.NET Core MVC Application
└── docs/                        # Project Documentation
```

---

## 1. CommunityCar.Domain

The Domain layer contains the core business logic, entities, value objects, and domain services.

### 1.1 Base Components (`/Base`)
Core building blocks for domain-driven design:

- **AggregateRoot.cs** - Base class for aggregate roots
- **BaseEntity.cs** - Base entity with common properties (Id, CreatedAt, etc.)
- **DomainException.cs** - Custom exception for domain rule violations
- **Enumeration.cs** - Base class for enumeration pattern
- **Guard.cs** - Guard clauses for validation
- **PagedResult.cs** - Generic paging result wrapper
- **QueryParameters.cs** - Base query parameters for filtering/sorting
- **Result.cs** - Result pattern implementation
- **ValueObject.cs** - Base class for value objects

#### Interfaces (`/Base/Interfaces`)
- **IAuditable.cs** - Interface for auditable entities
- **IBusinessRule.cs** - Interface for business rules
- **ICommand.cs** - CQRS command interface
- **IDomainEvent.cs** - Domain event interface
- **IEntity.cs** - Base entity interface
- **IQuery.cs** - CQRS query interface
- **ISoftDelete.cs** - Soft delete interface

### 1.2 Commands (`/Commands`)
CQRS command definitions:

#### Identity Commands (`/Commands/Identity`)
- **ChangePasswordCommand.cs** - Change user password
- **LoginCommand.cs** - User login
- **RegisterUserCommand.cs** - User registration
- **UpdateUserProfileCommand.cs** - Update user profile

### 1.3 Queries (`/Queries`)
CQRS query definitions:

#### Identity Queries (`/Queries/Identity`)
- **GetUserByEmailQuery.cs** - Retrieve user by email
- **GetUserByIdQuery.cs** - Retrieve user by ID
- **SearchUsersQuery.cs** - Search users with filters

### 1.4 DTOs (`/DTOs`)
Data Transfer Objects for cross-layer communication:

#### Community DTOs (`/DTOs/Community`)
- **AnswerCommentDto.cs** - Answer comment data
- **AnswerDto.cs** - Answer data
- **CategoryDto.cs** - Category data
- **FriendRequestDto.cs** - Friend request data
- **FriendshipDto.cs** - Friendship data
- **QuestionBookmarkDto.cs** - Question bookmark data
- **QuestionDto.cs** - Question data
- **ReactionSummaryDto.cs** - Reaction summary data
- **TagDto.cs** - Tag data

#### Dashboard DTOs (`/DTOs/Dashboard`)
- **DashboardSummaryDto.cs** - Dashboard summary data
- **KPIValueDto.cs** - KPI value data

#### Identity DTOs (`/DTOs/Identity`)
- **RegisterUserDto.cs** - User registration data
- **UserDto.cs** - User data
- **UserSearchDto.cs** - User search result data

### 1.5 Entities (`/Entities`)
Domain entities organized by bounded context:

#### Communications Entities (`/Entities/Communications`)
- **chats/ChatMessage.cs** - Chat message entity
- **notifications/Notification.cs** - Notification entity

#### Community Entities (`/Entities/Community`)

**Common:**
- **Common/Category.cs** - Category entity
- **Common/Tag.cs** - Tag entity

**Events:**
- **events/CommunityEvent.cs** - Community event entity

**Feed:**
- **feed/FeedItem.cs** - Feed item entity

**Friends:**
- **friends/Friendship.cs** - Friendship entity

**Groups:**
- **groups/CommunityGroup.cs** - Community group entity

**Guides:**
- **guides/Guide.cs** - Guide entity

**Maps:**
- **maps/Location.cs** - Location entity
- **maps/MapPoint.cs** - Map point entity

**News:**
- **news/NewsArticle.cs** - News article entity

**Posts:**
- **post/Post.cs** - Post entity

**Q&A:**
- **qa/Answer.cs** - Answer entity
- **qa/AnswerComment.cs** - Answer comment entity
- **qa/AnswerReaction.cs** - Answer reaction entity
- **qa/AnswerVote.cs** - Answer vote entity
- **qa/Question.cs** - Question entity
- **qa/QuestionBookmark.cs** - Question bookmark entity
- **qa/QuestionReaction.cs** - Question reaction entity
- **qa/QuestionShare.cs** - Question share entity
- **qa/QuestionTag.cs** - Question-Tag relationship entity
- **qa/QuestionVote.cs** - Question vote entity

**Reviews:**
- **reviews/Review.cs** - Review entity

#### Dashboard Entities (`/Entities/Dashboard`)

**Analytics:**
- **analytics/content/** - Content analytics entities
- **analytics/users/** - User analytics entities

**Audit Logs:**
- **AuditLogs/AuditLogEntry.cs** - Audit log entry entity

**Health:**
- **health/SystemHealthMetric.cs** - System health metric entity

**KPIs:**
- **KPIs/KPI.cs** - KPI entity

**Management:**
- **management/content/** - Content management entities
- **management/users/** - User management entities

**Overview:**
- **overview/content/** - Content overview entities
- **overview/users/** - User overview entities

**Reports:**
- **reports/ReportDefinition.cs** - Report definition entity
- **reports/content/** - Content report entities
- **reports/users/** - User report entities

**Security:**
- **security/AuditLog.cs** - Audit log entity
- **security/SecurityAlert.cs** - Security alert entity

**Settings:**
- **settings/system/** - System settings entities
- **settings/users/** - User settings entities

**System:**
- **system/config/** - System configuration entities
- **system/logs/** - System log entities

**Trends:**
- **trends/TrendMetric.cs** - Trend metric entity

**Widgets:**
- **widgets/DashboardWidget.cs** - Dashboard widget entity

#### Identity Entities (`/Entities/Identity`)
- **Permissions/Permission.cs** - Permission entity
- **Profiles/UserProfile.cs** - User profile entity
- **Roles/ApplicationRole.cs** - Application role entity
- **Users/ApplicationUser.cs** - Application user entity

### 1.6 Enums (`/Enums`)
Enumeration types organized by bounded context:

#### Community Enums (`/Enums/Community`)
- **friends/FriendshipStatus.cs** - Friendship status enum
- **qa/ReactionType.cs** - Reaction type enum

### 1.7 Exceptions (`/Exceptions`)
Custom exception types:

- **ConflictException.cs** - Resource conflict exception
- **ForbiddenException.cs** - Forbidden access exception
- **InvalidCredentialsException.cs** - Invalid credentials exception
- **NotFoundException.cs** - Resource not found exception
- **TokenExpiredException.cs** - Token expiration exception
- **UnauthorizedException.cs** - Unauthorized access exception
- **ValidationException.cs** - Validation failure exception

### 1.8 Helpers (`/Helpers`)
Helper utilities:

- **CultureHelper.cs** - Culture/localization helpers
- **FileHelper.cs** - File operation helpers
- **SecurityHelper.cs** - Security-related helpers
- **SlugHelper.cs** - URL slug generation helpers
- **ValidationHelper.cs** - Validation helpers

### 1.9 Utilities (`/Utilities`)
Utility classes (duplicates of Helpers - consider consolidation):

- **CultureHelper.cs**
- **FileHelper.cs**
- **Guard.cs**
- **SecurityHelper.cs**
- **SlugHelper.cs**
- **ValidationHelper.cs**

### 1.10 Interfaces (`/Interfaces`)
Service and repository interfaces:

#### Common Interfaces (`/Interfaces/Common`)
- **ICurrentUserService.cs** - Current user context service
- **IRepository.cs** - Generic repository interface
- **ISecurityService.cs** - Security service interface
- **IUnitOfWork.cs** - Unit of work pattern interface

#### Community Interfaces (`/Interfaces/Community`)
- **ICategoryService.cs** - Category service interface
- **IFriendshipService.cs** - Friendship service interface
- **IQuestionService.cs** - Question service interface
- **ITagService.cs** - Tag service interface

#### Dashboard Interfaces (`/Interfaces/Dashboard`)
- **IDashboardService.cs** - Dashboard service interface

#### Identity Interfaces (`/Interfaces/Identity`)
- **IAuthenticationService.cs** - Authentication service interface
- **IUserRepository.cs** - User repository interface
- **IUserService.cs** - User service interface

#### Handler Interfaces
- **ICommandHandler.cs** - Command handler interface
- **IQueryHandler.cs** - Query handler interface

### 1.11 Models (`/Models`)
Domain models:

- **DashboardModels.cs** - Dashboard-related models
- **ReactionSummary.cs** - Reaction summary model

### 1.12 Constants (`/Constants`)
- **ErrorCodes.cs** - Error code constants

### 1.13 Configuration Files
- **CommunityCar.Domain.csproj** - Project file
- **infra_errors.json** - Infrastructure error definitions

---

## 2. CommunityCar.Infrastructure

The Infrastructure layer implements data access, external services, and cross-cutting concerns.

### 2.1 Data (`/Data`)
Database context and configurations:

- **ApplicationDbContext.cs** - EF Core database context

#### Configurations (`/Data/Configurations`)
Entity Framework configurations:

- **AnswerConfiguration.cs** - Answer entity configuration
- **AnswerReactionConfiguration.cs** - Answer reaction configuration
- **AnswerVoteConfiguration.cs** - Answer vote configuration
- **CategoryConfiguration.cs** - Category entity configuration
- **FriendshipConfiguration.cs** - Friendship entity configuration
- **QuestionBookmarkConfiguration.cs** - Question bookmark configuration
- **QuestionConfiguration.cs** - Question entity configuration
- **QuestionReactionConfiguration.cs** - Question reaction configuration
- **QuestionShareConfiguration.cs** - Question share configuration
- **QuestionTagConfiguration.cs** - Question-Tag configuration
- **QuestionVoteConfiguration.cs** - Question vote configuration
- **TagConfiguration.cs** - Tag entity configuration

#### Extensions (`/Data/Extensions`)
- **ModelBuilderExtensions.cs** - EF Core model builder extensions
- **QueryableExtensions.cs** - IQueryable extension methods

#### Interceptors (`/Data/Interceptors`)
- **AuditableEntityInterceptor.cs** - Automatic audit field population

#### Seed (`/Data/Seed`)
- **DbSeeder.cs** - Database seeding orchestrator
- **FriendshipSeeder.cs** - Friendship data seeder

### 2.2 Migrations (`/Migrations`)
Entity Framework migrations:

- **20260206000644_InitialCreate.cs** - Initial database migration
- **20260206000644_InitialCreate.Designer.cs** - Migration designer file
- **ApplicationDbContextModelSnapshot.cs** - Current model snapshot

### 2.3 Repositories (`/Repos`)
Repository implementations:

#### Common Repositories (`/Repos/Common`)
- **Repository.cs** - Generic repository implementation

#### Identity Repositories (`/Repos/Identity`)
- **UserRepository.cs** - User repository implementation

### 2.4 Services (`/Services`)
Service implementations:

#### Common Services (`/Services/Common`)
- **CurrentUserService.cs** - Current user context service
- **SecurityService.cs** - Security service implementation

#### Community Services (`/Services/Community`)
- **CategoryService.cs** - Category service implementation
- **FriendshipService.cs** - Friendship service implementation
- **QuestionService.cs** - Question service implementation
- **TagService.cs** - Tag service implementation

#### Dashboard Services (`/Services/Dashboard`)
- **DashboardService.cs** - Dashboard service implementation

#### Identity Services (`/Services/Identity`)
- **AuthenticationService.cs** - Authentication service implementation
- **UserService.cs** - User service implementation

### 2.5 Unit of Work (`/Uow`)
- **Common/UnitOfWork.cs** - Unit of work implementation

### 2.6 Mappings (`/Mappings`)
AutoMapper profiles:

- **FriendshipProfile.cs** - Friendship mapping profile
- **IdentityProfile.cs** - Identity mapping profile
- **QuestionProfile.cs** - Question mapping profile

### 2.7 Configuration Files
- **CommunityCar.Infrastructure.csproj** - Project file
- **DependencyInjection.cs** - Dependency injection configuration
- **infra_errors.json** - Infrastructure error definitions

---

## 3. CommunityCar.Web

The Presentation layer - ASP.NET Core MVC web application.

### 3.1 Areas
Feature-based organization using ASP.NET Areas:

#### Communications Area (`/Areas/Communications`)
- **Controllers/chats/** - Chat controllers
- **Controllers/notifications/** - Notification controllers
- **ViewModels/chats/** - Chat view models
- **ViewModels/notifications/** - Notification view models
- **Views/chats/** - Chat views
- **Views/notifications/** - Notification views

#### Community Area (`/Areas/Community`)
- **Controllers/events/** - Event controllers
- **Controllers/feed/** - Feed controllers
- **Controllers/friends/** - Friend controllers
- **Controllers/groups/** - Group controllers
- **Controllers/guides/** - Guide controllers
- **Controllers/maps/** - Map controllers
- **Controllers/news/** - News controllers
- **Controllers/post/** - Post controllers
- **Controllers/qa/** - Q&A controllers
- **Controllers/reviews/** - Review controllers
- **Validators/qa/** - Q&A validators
- **ViewModels/events/** - Event view models
- **ViewModels/feed/** - Feed view models
- **ViewModels/friends/** - Friend view models
- **ViewModels/FriendshipViewModels.cs** - Friendship view models
- **ViewModels/groups/** - Group view models
- **ViewModels/guides/** - Guide view models
- **ViewModels/maps/** - Map view models
- **ViewModels/news/** - News view models
- **ViewModels/post/** - Post view models
- **ViewModels/qa/** - Q&A view models
- **ViewModels/reviews/** - Review view models
- **Views/events/** - Event views
- **Views/feed/** - Feed views
- **Views/friends/** - Friend views
- **Views/groups/** - Group views
- **Views/guides/** - Guide views
- **Views/maps/** - Map views
- **Views/news/** - News views
- **Views/post/** - Post views
- **Views/qa/** - Q&A views
- **Views/Questions/** - Question views
- **Views/reviews/** - Review views
- **Views/_ViewImports.cshtml** - View imports

#### Dashboard Area (`/Areas/Dashboard`)
- **Controllers/analytics/** - Analytics controllers
- **Controllers/AuditLogs/** - Audit log controllers
- **Controllers/health/** - Health monitoring controllers
- **Controllers/KPIs/** - KPI controllers
- **Controllers/management/** - Management controllers
- **Controllers/overview/** - Overview controllers
- **Controllers/reports/** - Report controllers
- **Controllers/security/** - Security controllers
- **Controllers/settings/** - Settings controllers
- **Controllers/system/** - System controllers
- **Controllers/trends/** - Trend controllers
- **Controllers/UserActivity/** - User activity controllers
- **Controllers/widgets/** - Widget controllers
- **ViewModels/** - Dashboard view models
- **Views/** - Dashboard views

#### Identity Area (`/Areas/Identity`)
- **Controllers/** - Identity controllers
- **Validators/** - Identity validators
- **ViewModels/** - Identity view models
- **Views/** - Identity views

### 3.2 Controllers (`/Controllers`)
- **ErrorController.cs** - Error handling controller

### 3.3 Attributes (`/Attributes`)
- **ValidateModelAttribute.cs** - Model validation attribute

### 3.4 Exceptions (`/Exceptions`)
- **WebException.cs** - Web-specific exception

### 3.5 Extensions (`/Extensions`)
- **StringExtensions.cs** - String extension methods

### 3.6 Filters (`/Filters`)
- **GlobalExceptionFilter.cs** - Global exception filter

### 3.7 Helpers (`/Helpers`)
- **AppUrlHelper.cs** - URL generation helpers
- **PaginationHelper.cs** - Pagination helpers
- **ViewBagHelper.cs** - ViewBag helpers

### 3.8 Infrastructure (`/Infrastructure`)
- **Localization/JsonLocalization.cs** - JSON-based localization

### 3.9 Middleware (`/Middleware`)
- **ExceptionHandlerMiddleware.cs** - Exception handling middleware

### 3.10 Models (`/Models`)
- **ErrorViewModel.cs** - Error view model

### 3.11 Resources (`/Resources`)
Localization resources:

- **Areas/Communications/** - Communications localization
- **Areas/Community/** - Community localization
- **Areas/Dashboard/** - Dashboard localization
- **Areas/Identity/** - Identity localization
- **Controllers/ErrorController.ar.resx** - Arabic error messages
- **Controllers/ErrorController.en.resx** - English error messages
- **SharedResources.cs** - Shared resource accessor

### 3.12 Views (`/Views`)
Shared views:

- **Error/** - Error pages (400, 401, 403, 404, 500, 503)
- **Shared/_Layout.cshtml** - Main layout
- **Shared/_Layout.cshtml.css** - Layout styles
- **Shared/_ValidationScriptsPartial.cshtml** - Validation scripts
- **Shared/Error.cshtml** - Generic error view
- **_ViewImports.cshtml** - Global view imports
- **_ViewStart.cshtml** - View start configuration

### 3.13 wwwroot (`/wwwroot`)
Static files:

#### CSS (`/wwwroot/css`)
- **abstracts/** - Variables, mixins, functions
- **base/** - Reset, typography, base styles
- **components/** - Reusable component styles
  - buttons.css
  - cards.css
  - categories.css
  - tags.css
  - theme-toggle.css
- **layout/** - Layout-specific styles
- **pages/** - Page-specific styles
  - qa.css
- **responsive/** - Responsive design styles
- **utilities/** - Utility classes
  - theme-utilities.css
- **site.css** - Main stylesheet

#### JavaScript (`/wwwroot/js`)
- **components/** - Reusable components
  - tag-input.js
  - theme-toggle.js
- **core/** - Core functionality
- **layout/** - Layout scripts
- **pages/** - Page-specific scripts
- **utils/** - Utility functions
- **site.js** - Main JavaScript file

#### Libraries (`/wwwroot/lib`)
Third-party libraries:

- **bootstrap/** - Bootstrap framework
- **chart.js/** - Chart.js library
- **jquery/** - jQuery library
- **jquery-validation/** - jQuery validation
- **jquery-validation-unobtrusive/** - Unobtrusive validation
- **sweetalert2/** - SweetAlert2 library
- **toastr.js/** - Toastr notifications

#### Images (`/wwwroot/images`)
- **logo/** - Logo images

### 3.14 Configuration Files
- **CommunityCar.Web.csproj** - Project file
- **Program.cs** - Application entry point
- **appsettings.json** - Application settings
- **appsettings.Development.json** - Development settings
- **libman.json** - Client-side library configuration
- **Properties/launchSettings.json** - Launch profiles
- **.config/dotnet-tools.json** - .NET tools configuration

---

## 4. Documentation (`/docs`)

- **ARCHITECTURE.md** - Architecture documentation
- **PROJECT_STRUCTURE.md** - This file

---

## 5. Root Files

- **CommunityCar.sln** - Solution file
- **.gitignore** - Git ignore rules
- **CHANGELOG.md** - Project changelog
- **CODE_OF_CONDUCT.md** - Code of conduct
- **CONTRIBUTING.md** - Contribution guidelines
- **LICENSE** - License file
- **README.md** - Project readme
- **web_errors.txt** - Web error log

---

## Architecture Patterns

### Domain-Driven Design (DDD)
- Entities with rich domain logic
- Value objects for immutable concepts
- Aggregate roots for consistency boundaries
- Domain events for cross-aggregate communication

### CQRS (Command Query Responsibility Segregation)
- Separate command and query models
- Command handlers for write operations
- Query handlers for read operations

### Repository Pattern
- Generic repository for common operations
- Specialized repositories for complex queries
- Unit of Work for transaction management

### Clean Architecture
- Domain layer independent of infrastructure
- Infrastructure implements domain interfaces
- Web layer depends on abstractions

### MVC with Areas
- Feature-based organization
- Separation of concerns
- Modular structure

---

## Technology Stack

### Backend
- **Framework:** ASP.NET Core 9.0
- **ORM:** Entity Framework Core
- **Mapping:** AutoMapper
- **Validation:** FluentValidation

### Frontend
- **Framework:** ASP.NET Core MVC (Razor Views)
- **CSS:** Custom CSS with BEM methodology
- **JavaScript:** Vanilla JS with jQuery
- **UI Libraries:** Bootstrap, Chart.js, SweetAlert2, Toastr

### Database
- Entity Framework Core with migrations
- Support for multiple database providers

---

## Key Features by Area

### Community
- Q&A system with questions, answers, comments
- Reactions and voting system
- Categories and tags
- Bookmarks and shares
- Friendship management
- Posts and feed
- Events and groups
- Guides and reviews
- Maps and locations
- News articles

### Communications
- Chat messaging
- Notifications

### Dashboard
- Analytics (content and users)
- KPIs and metrics
- Health monitoring
- Audit logs
- Reports
- Security alerts
- System settings
- Trend analysis
- User activity tracking
- Customizable widgets

### Identity
- User registration and authentication
- User profiles
- Role-based access control
- Permissions management

---

## Development Guidelines

### Naming Conventions
- **Entities:** PascalCase (e.g., `Question`, `Answer`)
- **DTOs:** PascalCase with `Dto` suffix (e.g., `QuestionDto`)
- **Interfaces:** PascalCase with `I` prefix (e.g., `IQuestionService`)
- **View Models:** PascalCase with `ViewModel` suffix
- **Commands:** PascalCase with `Command` suffix
- **Queries:** PascalCase with `Query` suffix

### Folder Organization
- Group by feature/bounded context
- Separate concerns (entities, DTOs, services)
- Keep related files together

### Best Practices
- Follow SOLID principles
- Use dependency injection
- Implement proper error handling
- Write clean, maintainable code
- Document complex logic
- Use async/await for I/O operations

---

## Future Considerations

### Potential Improvements
- Consolidate duplicate helper classes (Helpers vs Utilities)
- Add unit and integration tests
- Implement caching strategy
- Add API documentation (Swagger/OpenAPI)
- Consider microservices for scalability
- Implement real-time features with SignalR
- Add comprehensive logging
- Implement background job processing

---

*Last Updated: February 6, 2026*
