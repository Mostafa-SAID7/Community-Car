# Dashboard Architecture Structure

## Overview

The Dashboard has a **4-section sidebar** with each section having its own **header and tab navigation**:

1. **Overview** - Main dashboard metrics and KPIs
2. **Analytics** - Reports and user activity analysis  
3. **Administration** - Settings, localization, management, security
4. **Monitoring** - Health, system, audit logs

---

## Current Structure

### Sidebar Navigation (_DashboardLeftSidebar.cshtml)

```
├── Overview Section
│   └── Dashboard (Main)
│
├── Analytics Section  
│   └── Analytics (Main)
│
├── Monitoring Section
│   ├── Health
│   ├── Audit
│   └── System
│
└── Administration Section
    ├── Management
    ├── Security
    ├── Localization
    └── Settings
```

---

## Section Details

### 1. OVERVIEW Section

**Sidebar Link:** `/Dashboard`

**Inner Structure:**
- **Header:** `_OverviewHeader.cshtml` (exists ✓)
- **Tab Navigation:** `_OverviewNavTabs.cshtml` (exists ✓)

**Tabs:**
- Overview (Dashboard/Index)
- KPIs (Dashboard/KPIs)
- Trends (Dashboard/Trends)
- Widgets (Dashboard/Widgets)

**Controllers:**
- `DashboardController.cs` - Main overview
- `Overview/KPIsController.cs` - KPI metrics
- `Overview/TrendsController.cs` - Trend analysis
- `Overview/WidgetsController.cs` - Widget management

---

### 2. ANALYTICS Section

**Sidebar Link:** `/Dashboard/Analytics`

**Inner Structure:**
- **Header:** `_AnalyticsHeader.cshtml` (MISSING ❌)
- **Tab Navigation:** `_AnalyticsNavTabs.cshtml` (exists ✓)

**Tabs:**
- Analysis (Analytics/Index)
- Reports (Analytics/Reports/Users or Content)
- User Activities (Analytics/UserActivity)

**Controllers:**
- `Analytics/AnalyticsController.cs` - Main analytics
- `Analytics/Reports/UserReportsController.cs` - User reports
- `Analytics/Reports/ContentReportsController.cs` - Content reports
- `Analytics/UserAnalyticsController.cs` - User activity

**Current Issues:**
- Analytics views have inline headers instead of using `_AnalyticsHeader.cshtml`
- Need to create `_AnalyticsHeader.cshtml` partial

---

### 3. ADMINISTRATION Section

**Sidebar Link:** `/Dashboard/Administration`

**Inner Structure:**
- **Header:** Inline in `Index.cshtml` (needs extraction ⚠️)
- **Tab Navigation:** Inline in `Index.cshtml` (needs extraction ⚠️)

**Tabs:**
- Localization (Administration?tab=localization)
- Management (Administration?tab=management)
- Security (Administration?tab=security)
- Settings (Administration?tab=settings)

**Controllers:**
- `Administration/LocalizationController.cs`
- `Administration/Management/UserManagementController.cs`
- `Administration/Management/ContentManagementController.cs`
- `Administration/Security/SecurityController.cs`
- `Administration/Settings/SettingsController.cs`

**Current Issues:**
- Header and tabs are inline in `Index.cshtml`
- Should extract to `_AdministrationHeader.cshtml` and `_AdministrationNavTabs.cshtml`

---

### 4. MONITORING Section

**Sidebar Link:** `/Dashboard/Monitoring`

**Inner Structure:**
- **Header:** Inline in `Index.cshtml` (needs extraction ⚠️)
- **Tab Navigation:** Inline in `Index.cshtml` (needs extraction ⚠️)

**Tabs:**
- Health (Monitoring?tab=health)
- System (Monitoring?tab=system)
- Audit Logs (Monitoring?tab=audit)

**Controllers:**
- `Monitoring/Health/HealthController.cs`
- `Monitoring/System/SystemController.cs`
- `Monitoring/Audit/AuditLogsController.cs`

**Current Issues:**
- Header and tabs are inline in `Index.cshtml`
- Should extract to `_MonitoringHeader.cshtml` and `_MonitoringNavTabs.cshtml`

---

## Required Fixes

### 1. Create Missing Header Partials

#### `_AnalyticsHeader.cshtml`
```razor
@using Microsoft.AspNetCore.Mvc.Localization
@inject IViewLocalizer Localizer
@{
    var title = ViewData["PageTitle"]?.ToString() ?? Localizer["Analytics"].Value;
    var subtitle = ViewData["PageSubtitle"]?.ToString() ?? Localizer["Analyze platform performance"].Value;
}

<div class="dashboard-page-header">
    <div class="dashboard-header-content">
        <div class="dashboard-header-text">
            <h1 class="dashboard-title">@title</h1>
            <p class="dashboard-subtitle">@subtitle</p>
        </div>
        <div class="dashboard-header-actions">
            <button class="dashboard-btn dashboard-btn-secondary" onclick="location.reload()">
                <i class="fas fa-sync-alt"></i>
                <span>@Localizer["Refresh"]</span>
            </button>
        </div>
    </div>
</div>
```

#### `_AdministrationHeader.cshtml`
```razor
@using Microsoft.AspNetCore.Mvc.Localization
@inject IViewLocalizer Localizer
@{
    var title = ViewData["PageTitle"]?.ToString() ?? Localizer["Administration"].Value;
    var subtitle = ViewData["PageSubtitle"]?.ToString() ?? Localizer["Manage system settings"].Value;
}

<div class="dashboard-page-header">
    <div class="dashboard-header-content">
        <div class="dashboard-header-text">
            <h1 class="dashboard-title">@title</h1>
            <p class="dashboard-subtitle">@subtitle</p>
        </div>
        <div class="dashboard-header-actions">
            <button class="dashboard-btn dashboard-btn-secondary" onclick="location.reload()">
                <i class="fas fa-sync-alt"></i>
                <span>@Localizer["Refresh"]</span>
            </button>
        </div>
    </div>
</div>
```

#### `_MonitoringHeader.cshtml`
```razor
@using Microsoft.AspNetCore.Mvc.Localization
@inject IViewLocalizer Localizer
@{
    var title = ViewData["PageTitle"]?.ToString() ?? Localizer["Monitoring"].Value;
    var subtitle = ViewData["PageSubtitle"]?.ToString() ?? Localizer["Monitor system health"].Value;
}

<div class="dashboard-page-header">
    <div class="dashboard-header-content">
        <div class="dashboard-header-text">
            <h1 class="dashboard-title">@title</h1>
            <p class="dashboard-subtitle">@subtitle</p>
        </div>
        <div class="dashboard-header-actions">
            <button class="dashboard-btn dashboard-btn-secondary" onclick="location.reload()">
                <i class="fas fa-sync-alt"></i>
                <span>@Localizer["Refresh"]</span>
            </button>
        </div>
    </div>
</div>
```

### 2. Extract Tab Navigation Partials

#### `_AdministrationNavTabs.cshtml`
Extract from `Administration/Index.cshtml`

#### `_MonitoringNavTabs.cshtml`
Extract from `Monitoring/Index.cshtml`

### 3. Update Views to Use Partials

All section views should follow this pattern:

```razor
<div class="dashboard-container">
    <!-- Section Header -->
    <partial name="../Shared/_[Section]Header" />

    <!-- Section Navigation Tabs -->
    <partial name="../Shared/_[Section]NavTabs" />

    <!-- Section Content -->
    <div class="dashboard-content">
        <!-- Content here -->
    </div>
</div>
```

---

## Consistent Pattern

Each dashboard section should have:

1. **Sidebar item** in `_DashboardLeftSidebar.cshtml`
2. **Header partial** in `Views/Shared/_[Section]Header.cshtml`
3. **Tab navigation partial** in `Views/Shared/_[Section]NavTabs.cshtml`
4. **Main view** that includes both partials
5. **Sub-views** for each tab

This creates a consistent, maintainable structure across all dashboard sections.
