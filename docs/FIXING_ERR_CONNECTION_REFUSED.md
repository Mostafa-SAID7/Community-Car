# Fixing ERR_CONNECTION_REFUSED Errors

## Problem
You're seeing this error in the browser console:
```
GET http://localhost:5010/Chats/GetUnreadCount net::ERR_CONNECTION_REFUSED
```

## Root Cause
The JavaScript is calling URLs **without the culture prefix** (`/en/` or `/ar/`), causing:
1. 302 redirect from `/Chats/GetUnreadCount` to `/en/Chats/GetUnreadCount`
2. Browser follows redirect but loses the original request context
3. Results in connection refused or 404 errors

## Solution Applied

### Files Fixed
1. ✅ `src/CommunityCar.Mvc/wwwroot/js/components/notification.js`
2. ✅ `src/CommunityCar.Mvc/Views/Chats/Index.cshtml`
3. ✅ `src/CommunityCar.Mvc/Areas/Communications/Views/chats/Index.cshtml`

### Changes Made
All AJAX/fetch calls now use `CultureHelper.addCultureToUrl()`:

**Before:**
```javascript
fetch('/Chats/GetUnreadCount')
```

**After:**
```javascript
const url = CultureHelper.addCultureToUrl('/Chats/GetUnreadCount');
fetch(url)
```

## How to Apply the Fix

### Step 1: Clear Browser Cache
The browser has cached the old JavaScript files.

**Option A: Hard Refresh (Recommended)**
- Windows/Linux: `Ctrl + Shift + R` or `Ctrl + F5`
- Mac: `Cmd + Shift + R`

**Option B: Clear Cache via DevTools**
1. Open DevTools (`F12`)
2. Right-click the refresh button
3. Select "Empty Cache and Hard Reload"

**Option C: Use PowerShell Script**
```powershell
.\scripts\clear-browser-cache.ps1 -Browser Chrome
```

### Step 2: Verify the Fix
1. Open browser DevTools (`F12`)
2. Go to Network tab
3. Filter by "XHR" or "Fetch"
4. Refresh the page
5. Look for requests - they should now go to `/en/Chats/GetUnreadCount` (with culture prefix)

### Step 3: Check Console
The console should no longer show `ERR_CONNECTION_REFUSED` errors.

## Why This Happens

### Your App Architecture
```
CommunityCar uses culture-based routing:
✅ /en/Chats/Index
✅ /ar/Questions/Details/123
❌ /Chats/GetUnreadCount (missing culture)
```

### The Middleware
`CultureRedirectMiddleware` automatically redirects URLs without culture:
```
/Chats/Index → 302 redirect → /en/Chats/Index
```

This works fine for page navigation, but AJAX calls need the culture prefix from the start.

## Testing the Fix

### Test 1: Check URL in Network Tab
```
Before: GET http://localhost:5010/Chats/GetUnreadCount
After:  GET http://localhost:5010/en/Chats/GetUnreadCount
```

### Test 2: Check Response
```
Status: 200 OK
Response: {"success":true,"count":5}
```

### Test 3: No Console Errors
```
✅ No ERR_CONNECTION_REFUSED
✅ No 302 redirects on AJAX calls
✅ No 404 errors
```

## If Errors Persist

### 1. Verify Server is Running
```powershell
# Check if port 5010 is listening
netstat -ano | findstr :5010

# Should show:
# TCP    0.0.0.0:5010    0.0.0.0:0    LISTENING    12345
```

### 2. Check Process Status
```powershell
# List running processes
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"}
```

### 3. Restart the Server
```powershell
# Stop current process
Ctrl + C

# Rebuild and run
dotnet build
dotnet run --project src/CommunityCar.Mvc/CommunityCar.Mvc.csproj
```

### 4. Clear All Browser Storage
```powershell
# Use the comprehensive script
.\scripts\clear-browser-storage.ps1 -Browser Chrome -StorageType All
```

### 5. Check for Multiple Tabs
Close all browser tabs with `localhost:5010` and open a fresh one.

## Prevention

### For Future Development

**Always use CultureHelper for AJAX URLs:**
```javascript
// ✅ GOOD
const url = CultureHelper.addCultureToUrl('/Controller/Action');
fetch(url);

// ❌ BAD
fetch('/Controller/Action');
```

**Or use Razor helpers in views:**
```javascript
// ✅ GOOD (in .cshtml files)
const url = '@Url.Action("Action", "Controller")';
fetch(url);
```

**Add error handling:**
```javascript
fetch(url)
    .then(r => r.json())
    .then(data => handleSuccess(data))
    .catch(err => {
        console.debug('Request failed:', err.message);
        // Don't spam console with errors during server restarts
    });
```

## Common Scenarios

### Scenario 1: Server Restart During Development
**Symptom:** Temporary ERR_CONNECTION_REFUSED errors
**Solution:** Normal - errors will stop once server is back up
**Prevention:** Add `.catch()` handlers to prevent console spam

### Scenario 2: Browser Cache
**Symptom:** Errors persist after code changes
**Solution:** Hard refresh (`Ctrl + Shift + R`)
**Prevention:** Use cache-busting or versioning for JS files

### Scenario 3: Wrong URL
**Symptom:** Consistent 404 or connection refused
**Solution:** Use `CultureHelper.addCultureToUrl()`
**Prevention:** Always test AJAX endpoints in browser first

## Quick Reference

### Check if Server is Running
```powershell
netstat -ano | findstr :5010
```

### Hard Refresh Browser
```
Ctrl + Shift + R (Windows/Linux)
Cmd + Shift + R (Mac)
```

### Clear Browser Cache
```powershell
.\scripts\clear-browser-cache.ps1 -Browser Chrome
```

### Test Endpoint Manually
```
Open browser: http://localhost:5010/en/Chats/GetUnreadCount
Should return: {"success":true,"count":0}
```

### View Network Requests
```
F12 → Network Tab → Filter: XHR/Fetch
```

## Summary

✅ **Fixed:** All AJAX calls now include culture prefix
✅ **Added:** Error handling to prevent console spam
✅ **Changed:** `console.error()` to `console.debug()` for expected failures
✅ **Documented:** Best practices for AJAX in MVC apps

**Next Step:** Hard refresh your browser (`Ctrl + Shift + R`) to load the updated JavaScript files.

The errors should disappear once the browser loads the fixed code!
