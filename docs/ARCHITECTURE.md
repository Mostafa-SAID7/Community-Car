# Architecture Overview

Community Car follows Clean Architecture principles with clear separation of concerns across three main layers.

## Architecture Layers

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│                  (CommunityCar.Web)                      │
│  Controllers, Views, ViewModels, Validators, Filters    │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                    │
│              (CommunityCar.Infrastructure)               │
│   Data Access, Services, Repositories, External APIs    │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│                     Domain Layer                         │
│                 (CommunityCar.Domain)                    │
│    Entities, Interfaces, DTOs, Business Logic, Rules    │
└─────────────────────────────────────────────────────────┘
```

## Domain Layer

The core of the application containing business logic and domain entities.

### Key Components

#### Entities
- **BaseEntity**: Base class for all entities with Id, CreatedAt, UpdatedAt
- **AggregateRoot**: Root entities that maintain consistency boundaries
- **Value Objects**: Immutable objects defined by their attributes

#### Interfaces
- **IRepository<T>**: Generic repository pattern
- **IUnitOfWork**: Transaction management
- **Service Interfaces**: Define contracts for business operations

#### DTOs (Data Transfer Objects)
- Transfer data between layers
- Prevent over-posting attacks
- Optimize data transfer

#### Business Rules
- Domain validation logic
- Business constraints
- Invariant enforcement

### Domain Entities Structure

```
Domain/
├── Entities/
│   ├── Identity/
│   │   ├── Users/ApplicationUser.cs
│   │   ├── Roles/ApplicationRole.cs
│   │   ├── Profiles/UserProfile.cs
│ 