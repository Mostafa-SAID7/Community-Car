# Notifications Feature Documentation

## Overview
The Notifications feature provides a comprehensive notification system for the CommunityCar application, allowing users to receive real-time updates about activities, interactions, and important events.

## Features Implemented

### 1. Enhanced NotificationsController
- **Pagination Support**: View notifications with customizable page size
- **Filtering**: Filter by read/unread status
- **Error Handling**: Comprehensive try-catch blocks with logging
- **Time Ago Display**: Human-readable time format (e.g., "2h ago", "3d ago")
- **Multiple Endpoints**: Various endpoints for different use cases

### 2. API Endpoints

#### GET /Communications/Notifications
Main notifications page with pagination and filtering.
- **Parameters**: 
  - `page` (int, default: 1)
  - `pageSize` (int, default: 20)
  - `unreadOnly` (bool?, optional)

#### GET /Communications/Notifications/UnreadCount
Get the count of unread notifications.
- **Returns**: `{ success: bool, count: int }`

#### GET /Communications/Notifications/Latest
Get the latest notifications.
- **Parameters**: `count` (int, default: 10)
- **Returns**: `{ success: bool, notifications: array }`

#### GET /Communications/Notifications/GetAll
Get all notifications with pagination (AJAX endpoint).
- **Parameters**: 
  - `page` (int, default: 1)
  - `pageSize` (int, default: 20)
  - `unreadOnly` (bool?, optional)
- **Returns**: `{ success: bool, notifications: array, totalCount: int, page: int, pageSize: int }`

#### POST /Communications/Notifications/MarkAsRead/{id}
Mark a specific notification as read.
- **Returns**: `{ success: bool, unreadCount: int }`

#### POST /Communications/Notifications/MarkAllAsRead
Mark all notifications as read for the current user.
- **Returns**: `{ success: bool, message: string }`

#### POST /Communications/Notifications/Delete/{id}
Delete a specific notification.
- **Returns**: `{ success: bool, message: string }`

#### GET /Communications/Notifications/Dropdown
Get notifications for dropdown display (limited to 5).
- **Returns**: `{ success: bool, notifications: array, unreadCount: int }`

### 3. Views

#### Index.cshtml
Full-featured notifications page with:
- Tabbed interface (All / Unread)
- Pagination controls
- Mark as read functionality
- Delete notifications
- Auto-refresh capability
- Responsive design
- Empty state handling

#### _NotificationDropdown.cshtml (Partial View)
Reusable dropdown component for navigation bar:
- Shows latest 5 notifications
- Unread count badge
- Auto-refresh every 30 seconds
- Mark all as read button
- Link to full notifications page

### 4. ViewModels

#### NotificationListViewModel
- Notifications list
- Pagination metadata
- Filter settings

#### NotificationItemViewModel
- Individual notification data
- Time ago calculation
- Read status

#### NotificationDropdownViewModel
- Dropdown-specific data
- Unread count

#### CreateNotificationViewModel
- For creating new notifications
- Validation attributes

#### NotificationSettingsViewModel
- User notification preferences
- Toggle different notification types

### 5. Helper Methods

#### GetCurrentUserId()
Safely extracts the current user ID from claims with proper error handling.

#### GetTimeAgo(DateTimeOffset dateTime)
Converts timestamps to human-readable format:
- "just now" (< 1 minute)
- "5m ago" (< 1 hour)
- "2h ago" (< 24 hours)
- "3d ago" (< 7 days)
- "2w ago" (< 30 days)
- "3mo ago" (< 365 days)
- "1y ago" (>= 365 days)

## Integration Guide

### 1. Add Notification Dropdown to Layout

In your `_Layout.cshtml`, add the notification dropdown to the navigation:

```html
<ul class="navbar-nav">
    @await Html.PartialAsync("_NotificationDropdown")
    <!-- Other nav items -->
</ul>
```

### 2. Create Notifications Programmatically

```csharp
// Inject INotificationService
private readonly INotificationService _notificationService;

// Create a notification
await _notificationService.CreateNotificationAsync(
    userId: user.Id,
    title: "New Answer",
    message: "Someone answered your question",
    link: "/Questions/Details/123"
);
```

### 3. Domain-Specific Notifications

The service includes pre-built methods for common scenarios:

```csharp
// Notify about new answer
await _notificationService.NotifyAuthorOfNewAnswerAsync(questionAuthorId, answer);

// Notify about vote
await _notificationService.NotifyAuthorOfQuestionVoteAsync(authorId, question, isUpvote: true);

// Notify about reaction
await _notificationService.NotifyAuthorOfQuestionReactionAsync(authorId, question, "like");
```

## JavaScript Integration

### Load Notifications
```javascript
$.get('/Communications/Notifications/Latest?count=10', function(response) {
    if (response.success) {
        // Handle notifications
        response.notifications.forEach(function(notification) {
            console.log(notification.title, notification.message);
        });
    }
});
```

### Mark as Read
```javascript
$.post('/Communications/Notifications/MarkAsRead/' + notificationId, function(response) {
    if (response.success) {
        console.log('Unread count:', response.unreadCount);
    }
});
```

### Mark All as Read
```javascript
$.post('/Communications/Notifications/MarkAllAsRead', function(response) {
    if (response.success) {
        console.log(response.message);
    }
});
```

## Styling

The notification dropdown includes custom CSS for:
- Shadow effects
- Hover states
- Badge positioning
- Responsive layout
- Smooth transitions

## Security Features

1. **Authorization**: All endpoints require authentication
2. **User Isolation**: Users can only access their own notifications
3. **Input Validation**: All inputs are validated
4. **Error Handling**: Comprehensive error handling with logging
5. **CSRF Protection**: POST endpoints use anti-forgery tokens

## Performance Considerations

1. **Pagination**: Limits data transfer and rendering
2. **Lazy Loading**: Dropdown loads on demand
3. **Caching**: Consider implementing Redis caching for high-traffic scenarios
4. **Indexing**: Database indexes on UserId and IsRead fields
5. **Auto-Refresh**: Configurable intervals (default: 30 seconds)

## Future Enhancements

### High Priority
1. **Real-time Updates**: Integrate SignalR for instant notifications
2. **Push Notifications**: Browser push notifications
3. **Email Notifications**: Send email digests
4. **Notification Preferences**: User settings for notification types
5. **Bulk Actions**: Select multiple notifications for batch operations

### Medium Priority
1. **Notification Categories**: Group by type (mentions, votes, answers, etc.)
2. **Search**: Search within notifications
3. **Archive**: Archive old notifications
4. **Export**: Export notification history
5. **Notification Templates**: Customizable notification templates

### Low Priority
1. **Notification Sounds**: Audio alerts for new notifications
2. **Desktop Notifications**: OS-level notifications
3. **Notification Analytics**: Track notification engagement
4. **Smart Grouping**: Group similar notifications
5. **Snooze**: Temporarily hide notifications

## Troubleshooting

### Notifications Not Appearing
- Check if NotificationService is registered in DI
- Verify database connection
- Check user authentication
- Review application logs

### Unread Count Not Updating
- Verify MarkAsReadAsync is being called
- Check database for IsRead field updates
- Clear browser cache
- Check JavaScript console for errors

### Dropdown Not Loading
- Verify route configuration
- Check JavaScript console for errors
- Ensure jQuery is loaded
- Verify Bootstrap is properly configured

## Testing Checklist

- [ ] View all notifications
- [ ] Filter by unread
- [ ] Mark single notification as read
- [ ] Mark all notifications as read
- [ ] Delete notification
- [ ] Pagination works correctly
- [ ] Dropdown displays correctly
- [ ] Unread count updates
- [ ] Auto-refresh works
- [ ] Time ago displays correctly
- [ ] Links work correctly
- [ ] Mobile responsive
- [ ] Error handling works

## Database Schema

### Notifications Table
- `Id` (PK, Guid)
- `UserId` (FK to Users, Guid)
- `Title` (string, max 200)
- `Message` (string, max 1000)
- `Link` (string, nullable, max 500)
- `IsRead` (bool)
- `CreatedAt` (DateTimeOffset)
- `CreatedBy` (string, nullable)
- `ModifiedAt` (DateTimeOffset, nullable)
- `ModifiedBy` (string, nullable)
- `IsDeleted` (bool)
- `DeletedAt` (DateTimeOffset, nullable)

### Indexes
- `IX_Notifications_UserId`
- `IX_Notifications_IsRead`
- `IX_Notifications_CreatedAt`

## API Response Examples

### Success Response
```json
{
  "success": true,
  "notifications": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "title": "New Answer",
      "message": "John Doe answered your question",
      "link": "/Questions/Details/456",
      "timeAgo": "2h ago",
      "isRead": false
    }
  ],
  "unreadCount": 5
}
```

### Error Response
```json
{
  "success": false,
  "message": "Failed to load notifications"
}
```

## Best Practices

1. **Keep Messages Concise**: Notification messages should be brief and actionable
2. **Provide Context**: Include relevant links for users to take action
3. **Avoid Spam**: Don't send too many notifications for the same event
4. **Respect Preferences**: Honor user notification settings
5. **Clean Up**: Periodically archive or delete old notifications
6. **Test Thoroughly**: Test all notification scenarios
7. **Monitor Performance**: Track notification delivery and engagement

## Support

For issues or questions:
- Check the troubleshooting section
- Review application logs
- Check database for notification records
- Verify service registration in DI container

## Conclusion

The enhanced Notifications feature provides a robust, user-friendly notification system with:
- ✅ Comprehensive API endpoints
- ✅ Full-featured UI with pagination and filtering
- ✅ Reusable dropdown component
- ✅ Error handling and logging
- ✅ Security best practices
- ✅ Performance optimizations
- ✅ Extensible architecture

The system is production-ready and can be easily extended with additional features like real-time updates, push notifications, and user preferences.
