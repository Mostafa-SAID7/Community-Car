# Localization Management Feature

## Overview

The Localization Management feature provides a comprehensive system for managing multi-language translations in the CommunityCar application. It includes a database-backed translation system with a full admin interface for managing translations across multiple languages and cultures.

## Architecture

### Components

1. **Domain Layer**
   - `LocalizationResource` entity - Stores translation data
   - `LocalizationResourceDto` - Data transfer objects
   - `ILocalizationService` - Service interface

2. **Infrastructure Layer**
   - `LocalizationService` - Implementation of translation management
   - Database migration for LocalizationResources table

3. **Presentation Layer**
   - `LocalizationController` - Admin dashboard controller
   - View models for CRUD operations
   - Razor views for UI

## Features

### 1. Translation Management
- **Create** new translations with key, category, culture, and value
- **Edit** existing translations
- **Delete** translations
- **View** translation details and related translations
- **Filter** by key, category, culture, status, or search term
- **Pagination** for large datasets

### 2. Bulk Operations
- **Bulk Import** - Import translations from JSON files
- **Export** - Export all translations to JSON format
- **Sync to Files** - Synchronize database translations to JSON files for runtime use

### 3. Statistics & Analytics
- Total keys count
- Total translations count
- Translations by culture
- Translations by category
- Missing translations count
- Completion percentage

### 4. Missing Translations Detection
- Compare translations between source and target cultures
- Identify missing translations
- Facilitate translation completion

## Database Schema

```sql
CREATE TABLE LocalizationResources (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Key NVARCHAR(200) NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    CultureCode NVARCHAR(10) NOT NULL,
    Value NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastModifiedAt DATETIME2 NULL,
    LastModifiedBy NVARCHAR(100) NULL,
    CONSTRAINT UQ_Key_Culture UNIQUE (Key, CultureCode)
);

CREATE INDEX IX_Category ON LocalizationResources(Category);
CREATE INDEX IX_CultureCode ON LocalizationResources(CultureCode);
CREATE INDEX IX_IsActive ON LocalizationResources(IsActive);
```

## API Endpoints

### Web UI Routes
- `GET /Dashboard/Localization` - List all translations
- `GET /Dashboard/Localization/Create` - Create translation form
- `POST /Dashboard/Localization/Create` - Submit new translation
- `GET /Dashboard/Localization/Edit/{id}` - Edit translation form
- `POST /Dashboard/Localization/Edit/{id}` - Update translation
- `POST /Dashboard/Localization/Delete/{id}` - Delete translation
- `GET /Dashboard/Localization/Details/{id}` - View translation details
- `GET /Dashboard/Localization/Statistics` - View statistics
- `GET /Dashboard/Localization/BulkImport` - Bulk import form
- `POST /Dashboard/Localization/BulkImport` - Submit bulk import
- `GET /Dashboard/Localization/Export` - Export all translations
- `GET /Dashboard/Localization/MissingTranslations` - Find missing translations
- `POST /Dashboard/Localization/SyncToFiles` - Sync to JSON files

### API Routes
- `GET /Dashboard/Localization/Api/Resources/{cultureCode}` - Get all resources for culture
- `GET /Dashboard/Localization/Api/Statistics` - Get statistics JSON

## Usage

### Creating a Translation

1. Navigate to Dashboard → Localization
2. Click "Add Translation"
3. Fill in the form:
   - **Key**: Unique identifier (e.g., `Dashboard.Title`)
   - **Category**: Group name (e.g., `Dashboard`, `Common`)
   - **Culture Code**: Language code (e.g., `en`, `ar`)
   - **Value**: Translated text
   - **Description**: Optional context
4. Click "Create Translation"

### Key Naming Convention

Use dot notation for hierarchical organization:
- `Dashboard.Title` - Dashboard page title
- `Common.Save` - Common save button
- `Validation.Required` - Required field validation message
- `Errors.NotFound` - Not found error message

### Categories

Organize translations into logical categories:
- **Common**: Shared across the application
- **Dashboard**: Admin dashboard specific
- **Validation**: Form validation messages
- **Errors**: Error messages
- **Navigation**: Menu and navigation items
- **Forms**: Form labels and placeholders

### Bulk Import

Import translations from JSON:

```json
{
  "Dashboard.Title": "Dashboard",
  "Dashboard.Welcome": "Welcome to Dashboard",
  "Common.Save": "Save",
  "Common.Cancel": "Cancel"
}
```

1. Navigate to Bulk Import
2. Select culture code
3. Paste JSON content
4. Choose whether to overwrite existing
5. Click Import

### Syncing to Files

After making changes in the database:
1. Click "Sync to Files" button
2. Translations are exported to `Resources/Localization/{culture}.json`
3. Application will use these files at runtime

## Integration with Application

### Using Translations in Code

```csharp
// Inject IStringLocalizer
private readonly IStringLocalizer<SharedResources> _localizer;

// Use in code
var title = _localizer["Dashboard.Title"];
var message = _localizer["Common.WelcomeMessage", userName];
```

### Using Translations in Views

```cshtml
@inject IStringLocalizer<SharedResources> Localizer

<h1>@Localizer["Dashboard.Title"]</h1>
<button>@Localizer["Common.Save"]</button>
```

## Supported Cultures

Default supported cultures:
- **en** - English
- **ar** - Arabic (RTL support)

Add more cultures by:
1. Creating translations with new culture code
2. Updating `Program.cs` to include new culture
3. Syncing to files

## Best Practices

1. **Consistent Naming**: Use dot notation and be consistent
2. **Descriptive Keys**: Make keys self-explanatory
3. **Categories**: Group related translations
4. **Descriptions**: Add context for translators
5. **Regular Sync**: Sync to files after database changes
6. **Missing Translations**: Regularly check for missing translations
7. **Version Control**: Include JSON files in version control

## Security

- Access restricted to SuperAdmin and Admin roles
- CSRF protection on all POST operations
- Input validation on all forms
- SQL injection protection via EF Core

## Performance Considerations

- Translations cached at application startup
- Database indexes on frequently queried columns
- Pagination for large datasets
- Async operations throughout

## Future Enhancements

- [ ] Translation memory and suggestions
- [ ] Machine translation integration
- [ ] Translation workflow (draft, review, approved)
- [ ] Version history for translations
- [ ] Import/export in multiple formats (CSV, XLSX, RESX)
- [ ] Translation quality metrics
- [ ] Pluralization support
- [ ] Context-aware translations
- [ ] Translation comments and discussions

## Troubleshooting

### Translations Not Appearing

1. Check if translation exists in database
2. Verify culture code matches current UI culture
3. Ensure translation is marked as Active
4. Run "Sync to Files" to update JSON files
5. Restart application to reload translations

### Import Failures

1. Verify JSON format is valid
2. Check for duplicate keys
3. Ensure culture code is valid
4. Review error messages in UI

## Support

For issues or questions:
- Check application logs
- Review this documentation
- Contact development team
