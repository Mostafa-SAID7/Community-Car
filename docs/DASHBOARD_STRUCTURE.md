# Dashboard Structure

## New Hierarchical Organization

The dashboard has been reorganized into a clear hierarchical structure for better navigation and organization. Both Controllers and Views folders now follow the same structure.

### Physical Folder Structure

```
Dashboard/
├── Controllers/
│   ├── Overview/
│   │   ├── OverviewController.cs
│   │   ├── KPIs/
│   │   │   └── KPIsController.cs
│   │   ├── Trends/
│   │   │   └── TrendsController.cs
│   │   └── Widgets/
│   │       └── WidgetsController.cs
│   ├── Analytics/
│   │   ├── ContentAnalyticsController.cs
│   │   ├── UserAnalyticsController.cs
│   │   ├── Reports/
│   │   │   ├── UserReportsController.cs
│   │   │   └── ContentReportsController.cs
│   │   └── UserActivity/
│   │       ├── UserActivityController.cs
│   │       └── ContentActivityController.cs
│   ├── Monitoring/
│   │   ├── Health/
│   │   │   └── HealthController.cs
│   │   ├── Audit/
│   │   │   └── AuditLogsController.cs
│   │   └── System/
│   │       └── SystemController.cs
│   └── Administration/
│       ├── Management/
│       │   ├── UserManagementController.cs
│       │   └── ContentManagementController.cs
│       ├── Security/
│       │   └── SecurityController.cs
│       ├── Localization/
│       │   └── LocalizationController.cs
│       └── Settings/
│           └── SettingsController.cs
└── Views/
    ├── Overview/
    ├── Analytics/
    ├── Monitoring/
    └── Administration/
```

## URL Routes

### Overview Section
- **Dashboard**: `/Dashboard/Overview`
- **KPIs**: `/Dashboard/Overview/KPIs`
- **Trends**: `/Dashboard/Overview/Trends`
- **Widgets**: `/Dashboard/Overview/Widgets`

### Analytics Section
- **User Reports**: `/Dashboard/Analytics/Reports/Users`
- **Content Reports**: `/Dashboard/Analytics/Reports/Content`
- **User Activity**: `/Dashboard/Analytics/UserActivity`
- **Content Activity**: `/Dashboard/Analytics/UserActivity/Content`

### Monitoring Section
- **Health**: `/Dashboard/Monitoring/Health`
- **Audit Logs**: `/Dashboard/Monitoring/Audit`
- **System**: `/Dashboard/Monitoring/System`

### Administration Section
- **User Management**: `/Dashboard/Administration/Management/Users`
- **Content Management**: `/Dashboard/Administration/Management/Content`
- **Security**: `/Dashboard/Administration/Security`
- **Localization**: `/Dashboard/Administration/Localization`
- **Settings**: `/Dashboard/Administration/Settings`

## Navigation

The left sidebar has been updated to reflect this hierarchical structure with four main sections:

1. **Overview** - Main dashboard metrics and visualizations
2. **Analytics** - Reports and activity tracking
3. **Monitoring** - System health, audit logs, and system information
4. **Administration** - Management, security, localization, and settings

## Benefits

- **Clear Organization**: Logical grouping of related functionality
- **Scalability**: Easy to add new features within existing categories
- **User Experience**: Intuitive navigation with clear hierarchy
- **Consistency**: All routes follow the same pattern
