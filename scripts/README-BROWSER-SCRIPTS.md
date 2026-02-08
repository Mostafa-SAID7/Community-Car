# Browser Storage & Cache Management Scripts

PowerShell scripts for managing browser storage, cache, and session data during development.

## Scripts Overview

### 1. `clear-browser-storage.ps1`
Comprehensive browser storage cleanup for all storage types.

**Usage:**
```powershell
# Clear all storage types for all browsers
.\clear-browser-storage.ps1

# Clear specific browser
.\clear-browser-storage.ps1 -Browser Chrome
.\clear-browser-storage.ps1 -Browser Edge
.\clear-browser-storage.ps1 -Browser Firefox

# Clear specific storage type
.\clear-browser-storage.ps1 -StorageType Session
.\clear-browser-storage.ps1 -StorageType Local
.\clear-browser-storage.ps1 -StorageType Cookies
.\clear-browser-storage.ps1 -StorageType Cache
.\clear-browser-storage.ps1 -StorageType IndexedDB
.\clear-browser-storage.ps1 -StorageType ServiceWorkers

# Combine parameters
.\clear-browser-storage.ps1 -Browser Chrome -StorageType Session
```

**Storage Types Cleared:**
- ✅ Session Storage
- ✅ Local Storage
- ✅ Cookies
- ✅ Cache Storage
- ✅ IndexedDB
- ✅ Service Workers
- ✅ File System Storage
- ✅ Shared Storage
- ✅ Storage Buckets

### 2. `clear-localhost-storage.ps1`
Targeted cleanup for localhost:5010 (CommunityCar app) only.

**Usage:**
```powershell
# Clear localhost storage for all browsers
.\clear-localhost-storage.ps1

# Clear specific browser
.\clear-localhost-storage.ps1 -Browser Chrome
.\clear-localhost-storage.ps1 -Browser Edge
```

**What It Clears:**
- ✅ Local Storage entries for localhost
- ✅ Session Storage entries for localhost
- ✅ IndexedDB databases for localhost
- ✅ Service Workers registered for localhost
- ✅ Shared Storage for localhost

**Benefits:**
- Preserves storage for other websites
- Faster execution
- Ideal for development testing

### 3. `clear-browser-cache.ps1`
Comprehensive cache cleanup including all cache types.

**Usage:**
```powershell
# Clear cache for all browsers
.\clear-browser-cache.ps1

# Clear specific browser
.\clear-browser-cache.ps1 -Browser Chrome
.\clear-browser-cache.ps1 -Browser Edge
.\clear-browser-cache.ps1 -Browser Firefox
```

**Cache Types Cleared:**
- ✅ HTTP Cache (images, CSS, JS, etc.)
- ✅ Code Cache (compiled JavaScript, WebAssembly)
- ✅ GPU Cache (WebGL, Canvas)
- ✅ Media Cache (audio, video)
- ✅ Application Cache (deprecated but still used)
- ✅ Storage Cache
- ✅ Startup Cache (Firefox)
- ✅ Offline Cache (Firefox)
- ✅ Thumbnails (Firefox)

**Shows:**
- Cache size before clearing (in MB)
- Individual cache type sizes
- Total space freed

## Common Use Cases

### Development Testing
```powershell
# Test fresh user experience
.\clear-browser-storage.ps1 -Browser Chrome -StorageType All

# Test without cached resources
.\clear-browser-cache.ps1 -Browser Chrome

# Quick localhost reset
.\clear-localhost-storage.ps1 -Browser Chrome
```

### Debugging Issues
```powershell
# Clear stale session data
.\clear-browser-storage.ps1 -Browser Chrome -StorageType Session

# Clear outdated cached files
.\clear-browser-cache.ps1 -Browser Chrome

# Reset authentication state
.\clear-browser-storage.ps1 -Browser Chrome -StorageType Cookies
```

### Performance Testing
```powershell
# Test cold start performance
.\clear-browser-cache.ps1 -Browser All

# Test warm start with cache
.\clear-browser-storage.ps1 -Browser Chrome -StorageType Session
```

### Before Deployment
```powershell
# Verify fresh install experience
.\clear-browser-storage.ps1 -Browser All -StorageType All
.\clear-browser-cache.ps1 -Browser All
```

## Browser Storage Locations

### Chrome
```
%LOCALAPPDATA%\Google\Chrome\User Data\Default\
├── Session Storage\
├── Local Storage\
├── IndexedDB\
├── Service Worker\
├── Cache\
├── Code Cache\
├── GPUCache\
├── Shared Storage\
└── Cookies
```

### Edge
```
%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\
├── Session Storage\
├── Local Storage\
├── IndexedDB\
├── Service Worker\
├── Cache\
├── Code Cache\
├── GPUCache\
├── Shared Storage\
└── Cookies
```

### Firefox
```
%APPDATA%\Mozilla\Firefox\Profiles\{profile}\
├── sessionstore.jsonlz4
├── webappsstore.sqlite (Local Storage)
├── cookies.sqlite
├── cache2\
├── storage\default\ (IndexedDB)
└── startupCache\
```

## Important Notes

### Browser Must Be Closed
All scripts automatically close browser processes before clearing storage. This is required because:
- Browsers lock storage files while running
- Changes may not take effect until restart
- Prevents data corruption

### Hard Refresh After Clearing
After running scripts, perform a hard refresh:
- **Chrome/Edge:** `Ctrl + Shift + R` or `Ctrl + F5`
- **Firefox:** `Ctrl + Shift + R` or `Ctrl + F5`

This ensures:
- All cached resources are reloaded
- Service Workers are re-registered
- Fresh content is fetched from server

### Storage Types Explained

#### Session Storage
- Temporary storage per tab/window
- Cleared when tab/window closes
- Used for: temporary form data, UI state

#### Local Storage
- Persistent storage per domain
- Survives browser restarts
- Used for: user preferences, settings, tokens

#### Cookies
- Small text files per domain
- Can have expiration dates
- Used for: authentication, tracking, preferences

#### IndexedDB
- Large-scale structured data storage
- Transactional database
- Used for: offline data, large datasets

#### Service Workers
- Background scripts for PWAs
- Enable offline functionality
- Cache API for resource caching

#### Cache Storage
- HTTP cache for resources
- Speeds up page loads
- Stores: images, CSS, JS, fonts

#### Shared Storage
- Cross-origin storage (experimental)
- Privacy-preserving data sharing
- Used for: A/B testing, frequency capping

## Troubleshooting

### Script Execution Policy Error
```powershell
# Check current policy
Get-ExecutionPolicy

# Set policy for current user (recommended)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Or run with bypass (one-time)
powershell -ExecutionPolicy Bypass -File .\clear-browser-storage.ps1
```

### Access Denied Errors
- Ensure browser is fully closed
- Run PowerShell as Administrator
- Check antivirus isn't blocking file deletion

### Storage Not Cleared
- Verify browser profile path exists
- Check for multiple browser profiles
- Some storage may require manual deletion

### Browser Won't Start After Clearing
- Rare, but browser may need to rebuild cache
- Wait 30 seconds and try again
- Reinstall browser if persistent

## Best Practices

### During Development
1. Clear localhost storage frequently to test fresh state
2. Clear cache when CSS/JS changes aren't appearing
3. Clear session storage when testing authentication flows

### Before Testing
1. Clear all storage to simulate new user
2. Clear cache to test cold start performance
3. Document which storage was cleared for reproducibility

### Before Deployment
1. Test with cleared storage to verify first-run experience
2. Verify offline functionality after clearing Service Workers
3. Test authentication flow after clearing cookies

## Integration with Development Workflow

### VS Code Task
Add to `.vscode/tasks.json`:
```json
{
  "label": "Clear Browser Storage",
  "type": "shell",
  "command": "powershell",
  "args": [
    "-ExecutionPolicy", "Bypass",
    "-File", "${workspaceFolder}/scripts/clear-localhost-storage.ps1",
    "-Browser", "Chrome"
  ],
  "problemMatcher": []
}
```

### Pre-commit Hook
Add to `.git/hooks/pre-commit`:
```bash
#!/bin/sh
# Clear localhost storage before committing
powershell -ExecutionPolicy Bypass -File ./scripts/clear-localhost-storage.ps1 -Browser Chrome
```

### npm Scripts
Add to `package.json`:
```json
{
  "scripts": {
    "clear:storage": "powershell -ExecutionPolicy Bypass -File ./scripts/clear-browser-storage.ps1",
    "clear:cache": "powershell -ExecutionPolicy Bypass -File ./scripts/clear-browser-cache.ps1",
    "clear:localhost": "powershell -ExecutionPolicy Bypass -File ./scripts/clear-localhost-storage.ps1"
  }
}
```

## Security Considerations

### What Gets Deleted
- ✅ Authentication tokens (requires re-login)
- ✅ User preferences (reset to defaults)
- ✅ Cached credentials (requires re-entry)
- ✅ Session data (loses unsaved work)

### What's Preserved
- ✅ Browser bookmarks
- ✅ Browser history
- ✅ Saved passwords (in browser password manager)
- ✅ Browser extensions
- ✅ Browser settings

### Privacy
- Scripts only access local browser storage
- No data is sent over network
- No logging of cleared data
- Safe to use on development machines

## Performance Impact

### After Clearing Storage
- First page load: **Slower** (no cached resources)
- Subsequent loads: **Normal** (cache rebuilt)
- Memory usage: **Lower** (less cached data)
- Disk space: **More available**

### After Clearing Cache
- Initial load: **Significantly slower** (all resources downloaded)
- Images/CSS/JS: **Re-downloaded**
- Service Workers: **Re-registered**
- IndexedDB: **Preserved** (unless explicitly cleared)

## Related Documentation

- [AJAX 302 Redirect Fix](../docs/INTERACTION_AUDIT_IMPLEMENTATION_SUMMARY.md)
- [SignalR Configuration](../docs/CHAT_IMPLEMENTATION_SUMMARY.md)
- [Culture Helper](../src/CommunityCar.Mvc/wwwroot/js/culture-helper.js)

## Support

For issues or questions:
1. Check browser console for errors
2. Verify browser profile paths
3. Run scripts with `-Verbose` flag
4. Check Windows Event Viewer for system errors

## Version History

- **v1.0** - Initial release with basic storage clearing
- **v1.1** - Added cache size reporting
- **v1.2** - Added localhost-specific clearing
- **v1.3** - Added Shared Storage and Storage Buckets support
