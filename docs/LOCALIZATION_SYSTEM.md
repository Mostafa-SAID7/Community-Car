# Localization System Documentation

## Overview
The CommunityCar application uses a comprehensive localization system that supports multiple file formats and provides a centralized management interface through the Dashboard.

## Supported Formats

### 1. Database (Primary Storage)
- All translations are stored in the `LocalizationResource` table
- Provides centralized management and querying capabilities
- Supports versioning and audit trails

### 2. JSON Files
- Location: `src/CommunityCar.Mvc/Resources/Localization/`
- Format: `{culture}.json` (e.g., `en.json`, `ar.json`, `es.json`)
- Structure: Flat key-value pairs
```json
{
  "Common.Welcome": "Welcome",
  "Common.Home": "Home"
}
```

### 3. RESX Files
- Location: `src/CommunityCar.Mvc/Resources/`
- Format: `{ControllerName}.{culture}.resx`
- Structure: XML-based resource files
- Organized by Controllers, Views, and Areas

## Features

### Dashboard Management
Access: `http://localhost:5000/en/Dashboard/Localization`

#### Available Actions:
1. **Create Translation** - Add individual translations
2. **Bulk Import** - Import multiple translations from JSON/CSV
3. **Export** - Export translations to JSON/CSV
4. **Sync From JSON Files** - Import translations from JSON files to database
5. **Sync To JSON Files** - Export database translations to JSON files
6. **Sync From RESX Files** - Import translations from RESX files to database
7. **Sync To RESX Files** - Export database translations to RESX files
8. **Missing Translations** - View translations missing in target cultures
9. **Statistics** - View completion percentages and translation counts

### Key Features:
- **Multi-language Support**: English (en), Arabic (ar), Spanish (es)
- **Category Organization**: Translations grouped by category
- **Search & Filter**: Find translations by key, value, culture, or category
- **Completion Tracking**: Monitor translation progress per language
- **Bidirectional Sync**: Sync between database, JSON, and RESX files

## Workflow

### Adding New Translations

#### Method 1: Through Dashboard UI
1. Navigate to Dashboard → Localization
2. Click "Create Translation"
3. Fill in Key, Culture, Value, Category, and Description
4. Save

#### Method 2: Bulk Import
1. Prepare JSON or CSV file with translations
2. Navigate to Dashboard → Localization
3. Click "Import" → "Bulk Import"
4. Upload file and select options
5. Submit

#### Method 3: Edit Files Directly
1. Edit JSON files in `Resources/Localization/`
2. Or edit RESX files in `Resources/` subfolders
3. Navigate to Dashboard → Localization
4. Click "Import" → "Sync From JSON Files" or "Sync From RESX Files"

### Exporting Translations

#### To JSON Files:
1. Navigate to Dashboard → Localization
2. Click "Export" → "Sync To JSON Files"
3. Files will be created/updated in `Resources/Localization/`

#### To RESX Files:
1. Navigate to Dashboard → Localization
2. Click "Export" → "Sync To RESX Files"
3. Files will be created/updated in `Resources/` with proper folder structure

### Finding Missing Translations
1. Navigate to Dashboard → Localization
2. Click "Missing Translations"
3. Select target culture
4. View missing keys grouped by category
5. Add missing translations through UI or bulk import

## Translation Key Naming Convention

### Format: `{Category}.{SubCategory}.{Key}`

Examples:
- `Common.Welcome` - Common category, Welcome key
- `Dashboard.Title` - Dashboard category, Title key
- `Posts.Create` - Posts category, Create key
- `Validation.Required` - Validation category, Required key

### RESX File Mapping:
- File: `Controllers/Content/PostsController.en.resx`
- Key in file: `CreateSuccess`
- Database key: `Controllers.Content.PostsController.CreateSuccess`

## Supported Languages

### Currently Configured:
1. **English (en)** - Default language
2. **Arabic (ar)** - RTL support
3. **Spanish (es)** - Additional language

### Adding New Language:
1. Create JSON file: `Resources/Localization/{culture}.json`
2. Add translations using same keys as English
3. Sync to database using "Sync From JSON Files"
4. Or create RESX files following naming convention
5. Sync using "Sync From RESX Files"

## API Endpoints

### Public API:
- `GET /api/localization/cultures` - Get available cultures
- `GET /api/localization/resources/{culture}` - Get all resources for culture

### Dashboard API:
- `GET /{culture}/Dashboard/Localization` - Main management page
- `GET /{culture}/Dashboard/Localization/Statistics` - View statistics
- `GET /{culture}/Dashboard/Localization/MissingTranslations` - View missing
- `POST /{culture}/Dashboard/Localization/SyncFromFiles` - Import from JSON
- `POST /{culture}/Dashboard/Localization/SyncToFiles` - Export to JSON
- `POST /{culture}/Dashboard/Localization/SyncFromResx` - Import from RESX
- `POST /{culture}/Dashboard/Localization/SyncToResx` - Export to RESX

## Best Practices

### 1. Use Descriptive Keys
```
✅ Good: "Posts.CreateSuccess", "Validation.EmailRequired"
❌ Bad: "msg1", "error2"
```

### 2. Organize by Category
Group related translations together using category prefixes

### 3. Keep Translations Synchronized
- After editing JSON/RESX files, sync to database
- After editing in dashboard, sync to files for version control

### 4. Use Placeholders for Dynamic Content
```json
{
  "Posts.ViewCount": "Viewed {0} times",
  "Users.WelcomeMessage": "Welcome back, {0}!"
}
```

### 5. Regular Backups
- Export translations regularly
- Commit JSON files to version control
- Database backups include translations

## Troubleshooting

### Translations Not Appearing
1. Check if translation exists in database
2. Verify culture code matches current UI culture
3. Check if translation is marked as Active
4. Clear cache and refresh

### Sync Issues
1. Verify file paths are correct
2. Check file permissions
3. Review logs for specific errors
4. Ensure JSON/RESX files are valid format

### Missing Translations
1. Use "Missing Translations" feature to identify gaps
2. Copy from source language and translate
3. Use "Sync Missing Keys" to copy structure

## Statistics & Monitoring

The system tracks:
- Total unique keys
- Total translations across all languages
- Translations per culture
- Translations per category
- Missing translations count
- Completion percentage

Access statistics at: `/{culture}/Dashboard/Localization/Statistics`

## Integration with Application

### In Controllers:
```csharp
private readonly IStringLocalizer<MyController> _localizer;

public IActionResult Index()
{
    ViewBag.Message = _localizer["WelcomeMessage"];
    return View();
}
```

### In Views:
```razor
@inject IStringLocalizer<MyView> Localizer

<h1>@Localizer["PageTitle"]</h1>
<p>@Localizer["Description"]</p>
```

### With Parameters:
```csharp
_localizer["Posts.ViewCount", viewCount]
// Output: "Viewed 42 times"
```

## Future Enhancements

Potential improvements:
- Machine translation integration
- Translation memory
- Glossary management
- Translation workflow (draft → review → approved)
- Version history per translation
- Collaborative translation interface
- Import/Export to XLIFF format
- Integration with translation services (Google Translate, DeepL)
