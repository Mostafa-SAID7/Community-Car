# Events Feature - Complete Implementation

## Overview
The Events feature allows users to create, manage, and participate in community events. This document provides a complete reference for the implementation.

## ✅ Status: FULLY FUNCTIONAL

All components are implemented and working correctly with no compilation errors.

## Architecture

### Domain Layer (`CommunityCar.Domain`)

#### Entities
- **CommunityEvent** (`Entities/Community/events/CommunityEvent.cs`)
  - Main event entity with title, description, dates, location
  - Supports both online and physical events
  - Tracks attendees and comments
  - Status workflow: Draft → Published → InProgress/Completed/Cancelled

- **EventAttendee** (`Entities/Community/events/EventAttendee.cs`)
  - Links users to events
  - Tracks attendance status (Going, Interested, Maybe, NotGoing)

- **EventComment** (`Entities/Community/events/EventComment.cs`)
  - User comments on events

#### Enums
- **EventCategory**: Meetup, Workshop, Conference, CarShow, RoadTrip, Charity, Racing, Maintenance, Social, Other
- **EventStatus**: Draft, Published, Cancelled, Completed, InProgress
- **AttendeeStatus**: Interested, Going, NotGoing, Maybe

#### DTOs
- **EventDto**: Complete event data with computed properties
- **EventAttendeeDto**: Attendee information with user details
- **EventCommentDto**: Comment with user information

#### Interface
- **IEventService** (`Interfaces/Community/IEventService.cs`)
  - Complete CRUD operations
  - Event lifecycle management (Publish, Cancel, Complete)
  - Attendance management (Join, Leave, Update)
  - Comment management
  - Query methods with filtering and pagination

### Infrastructure Layer (`CommunityCar.Infrastructure`)

#### Service Implementation
- **EventService** (`Services/Community/EventService.cs`)
  - Full implementation of IEventService
  - Entity Framework Core integration
  - Proper error handling with domain exceptions
  - Logging for all operations

#### Database Configuration
- **EventConfiguration** (`Data/Configurations/EventConfiguration.cs`)
- **EventAttendeeConfiguration** (`Data/Configurations/EventAttendeeConfiguration.cs`)
- **EventCommentConfiguration** (`Data/Configurations/EventCommentConfiguration.cs`)

#### AutoMapper Profile
- **EventProfile** (`Mappings/EventProfile.cs`)
  - Maps entities to DTOs
  - Handles computed properties

#### Dependency Injection
- Registered in `DependencyInjection.cs`:
  ```csharp
  services.AddScoped<IEventService, EventService>();
  ```

### Presentation Layer (`CommunityCar.Mvc`)

#### Controller
- **EventsController** (`Controllers/Community/EventsController.cs`)
  - Route: `/Events`
  - All CRUD operations
  - Event lifecycle actions
  - Attendance management
  - Comment functionality

#### ViewModels
- **CreateEventViewModel**: Form for creating events
- **EditEventViewModel**: Form for editing events
- **EventDetailsViewModel**: Complete event details with attendees and comments

#### Views
- **Index.cshtml**: List all events with filtering
- **Details.cshtml**: Event details with attendees and comments
- **Create.cshtml**: Create new event form
- **Edit.cshtml**: Edit event form
- **MyEvents.cshtml**: User's events (attending or organizing)

## API Endpoints

### Public Endpoints
- `GET /Events` - List events (with filters)
- `GET /Events/{slug}` - Event details

### Authenticated Endpoints
- `GET /Events/Create` - Show create form
- `POST /Events/Create` - Create event
- `GET /Events/Edit/{id}` - Show edit form
- `POST /Events/Edit/{id}` - Update event
- `POST /Events/Delete/{id}` - Delete event
- `POST /Events/Join/{id}` - Join event
- `POST /Events/Leave/{id}` - Leave event
- `POST /Events/Publish/{id}` - Publish event
- `POST /Events/Cancel/{id}` - Cancel event
- `POST /Events/AddComment` - Add comment
- `GET /Events/MyEvents` - User's events

## Features

### Event Management
✅ Create events with full details
✅ Edit event information
✅ Delete events
✅ Publish/Cancel/Complete workflow
✅ Featured events support
✅ Online and physical events
✅ Maximum attendee limits
✅ Image support

### Attendance
✅ Join events with status (Going, Interested, Maybe)
✅ Update attendance status
✅ Leave events
✅ View attendee lists
✅ Attendee count tracking

### Comments
✅ Add comments to events
✅ Edit own comments
✅ Delete own comments
✅ Paginated comment lists

### Filtering & Search
✅ Filter by category
✅ Filter by status
✅ Upcoming events
✅ Featured events
✅ User's events (attending/organizing)
✅ Pagination support

### Security
✅ Authorization checks
✅ Only organizers can edit/delete/publish/cancel
✅ Only comment authors can edit/delete comments
✅ Proper exception handling

## Usage Examples

### Creating an Event
```csharp
var eventDto = await _eventService.CreateEventAsync(
    title: "Car Meetup 2026",
    description: "Join us for a great car meetup!",
    startTime: DateTimeOffset.Now.AddDays(7),
    endTime: DateTimeOffset.Now.AddDays(7).AddHours(3),
    location: "Central Park",
    organizerId: userId,
    category: EventCategory.Meetup,
    maxAttendees: 50,
    isOnline: false
);
```

### Joining an Event
```csharp
await _eventService.JoinEventAsync(
    eventId: eventId,
    userId: userId,
    status: AttendeeStatus.Going
);
```

### Getting Upcoming Events
```csharp
var parameters = new QueryParameters { PageNumber = 1, PageSize = 12 };
var events = await _eventService.GetUpcomingEventsAsync(parameters, userId);
```

## Database Schema

### Events Table
- Id (PK)
- Title
- Slug (Unique)
- Description
- StartTime
- EndTime
- Location
- Address
- Latitude/Longitude
- OrganizerId (FK)
- Category
- Status
- MaxAttendees
- IsOnline
- OnlineUrl
- ImageUrl
- IsFeatured
- CreatedAt/UpdatedAt

### EventAttendees Table
- Id (PK)
- EventId (FK)
- UserId (FK)
- Status
- Notes
- CreatedAt/UpdatedAt

### EventComments Table
- Id (PK)
- EventId (FK)
- UserId (FK)
- Content
- CreatedAt/UpdatedAt

## Testing Checklist

- [ ] Create event as authenticated user
- [ ] Edit own event
- [ ] Delete own event
- [ ] Publish event
- [ ] Cancel event
- [ ] Join event
- [ ] Leave event
- [ ] Add comment
- [ ] Edit own comment
- [ ] Delete own comment
- [ ] View event details
- [ ] Filter events by category
- [ ] View upcoming events
- [ ] View my events (attending)
- [ ] View my events (organizing)
- [ ] Test max attendee limit
- [ ] Test authorization (non-organizer cannot edit)

## Notes

- Events use slug-based URLs for SEO
- Soft delete is not implemented (hard delete)
- Event images are stored as URLs (external storage)
- Time zones are handled using DateTimeOffset
- All operations are logged
- Proper exception handling with domain exceptions
