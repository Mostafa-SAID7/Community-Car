# Error Monitoring & Debugging Guide

## Overview

The CommunityCar application includes comprehensive error monitoring and debugging tools to help identify and fix issues in the browser.

## Features

### 1. Error Monitor (`error-monitor.js`)

Automatically captures and logs all JavaScript errors, AJAX errors, and console messages.

**What it captures:**
- JavaScript runtime errors
- Unhandled promise rejections
- Resource loading errors (images, scripts, CSS)
- Console errors and warnings
- AJAX/Fetch request failures
- HTTP error responses (4xx, 5xx)
- Click events on interactive elements

**How to use:**

1. **View Errors in Real-time:**
   - Press `Ctrl+Shift+E` to open the error panel
   - Shows the last 20 errors with details

2. **Export Errors:**
   - Click "Export" button in the error panel
   - Downloads a JSON file with all logged errors

3. **Clear Errors:**
   - Click "Clear" button to reset the error log

4. **Enable Debug Mode:**
   - Add `?debug=true` to any URL
   - Shows detailed click event logging in console

### 2. Developer Tools Panel (`dev-tools.js`)

Advanced debugging panel with multiple tabs for comprehensive monitoring.

**How to use:**

Press `Ctrl+Shift+D` to toggle the developer tools panel.

**Tabs:**

#### 🔴 Errors Tab
- Shows all captured errors with full details
- Stack traces for debugging
- Export and clear functionality
- Color-coded by severity (red = error, yellow = warning)

#### 🌐 Network Tab
- Monitors all AJAX and Fetch requests
- Shows request method, URL, status code, and duration
- Helps identify slow or failing API calls
- Color-coded status (green = success, red = error)

#### ⚡ Performance Tab
- Page load metrics
- DOM ready time
- Response time
- Render time
- Memory usage (if available)

#### 💻 Console Tab
- Quick reference for keyboard shortcuts
- Links to browser DevTools

## Server-Side Logging

Errors are automatically sent to the server for logging.

**Endpoint:** `POST /api/logs/client-error`

**What gets logged:**
- Error type and message
- Stack trace
- URL where error occurred
- User information (if authenticated)
- Timestamp and user agent

**View server logs:**
```bash
# Check application logs
tail -f logs/application.log
```

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+Shift+D` | Toggle Developer Tools Panel |
| `Ctrl+Shift+E` | Show Error Monitor Panel |
| `F12` | Open Browser DevTools |

## Common Error Scenarios

### 1. AJAX Request Fails

**Symptoms:**
- Network tab shows red status code
- Error tab shows "AJAX Error" or "Fetch Error"

**How to debug:**
1. Open Dev Tools (`Ctrl+Shift+D`)
2. Go to Network tab
3. Find the failed request
4. Check status code and duration
5. Look at server logs for backend errors

### 2. JavaScript Error on Button Click

**Symptoms:**
- Button doesn't work
- Error tab shows "JavaScript Error"

**How to debug:**
1. Open Error Monitor (`Ctrl+Shift+E`)
2. Look for the error message and stack trace
3. Note the filename and line number
4. Fix the code at that location

### 3. Resource Not Loading

**Symptoms:**
- Images or scripts missing
- Error tab shows "Resource Load Error"

**How to debug:**
1. Check the error message for the resource URL
2. Verify the file exists in `wwwroot`
3. Check file path and casing (case-sensitive on Linux)

### 4. Unhandled Promise Rejection

**Symptoms:**
- Async operation fails silently
- Error tab shows "Unhandled Promise Rejection"

**How to debug:**
1. Look at the error message and stack trace
2. Add `.catch()` handlers to promises
3. Use `try/catch` with async/await

## Best Practices

### For Development

1. **Always have Dev Tools open** when testing new features
2. **Check the Network tab** after every AJAX call
3. **Monitor the Errors tab** for any warnings or errors
4. **Use debug mode** (`?debug=true`) for detailed logging

### For Production

1. **Disable verbose logging** in production
2. **Monitor server logs** for client errors
3. **Set up alerts** for critical errors
4. **Review error patterns** regularly

## Configuration

### Enable/Disable Features

Edit `error-monitor.js` initialization:

```javascript
window.errorMonitor = new ErrorMonitor({
    enableConsoleCapture: true,    // Capture console.log/error/warn
    enableAjaxCapture: true,       // Capture AJAX/Fetch requests
    enableClickCapture: true,      // Capture click events
    enableErrorCapture: true,      // Capture JavaScript errors
    logToServer: true,             // Send errors to server
    logToConsole: true             // Log to browser console
});
```

### Change Server Endpoint

```javascript
window.errorMonitor = new ErrorMonitor({
    serverEndpoint: '/api/logs/client-error'  // Change this
});
```

## Troubleshooting

### Error Monitor Not Working

1. Check browser console for initialization message:
   ```
   ✅ Error Monitor initialized. Press Ctrl+Shift+E to view errors.
   ```

2. Verify scripts are loaded:
   - Open browser DevTools (F12)
   - Go to Network tab
   - Look for `error-monitor.js` and `dev-tools.js`

3. Check for script errors:
   - Look in browser console for any errors

### Dev Tools Panel Not Showing

1. Press `Ctrl+Shift+D` again to toggle
2. Check if panel is hidden behind other elements
3. Try refreshing the page

### Errors Not Sent to Server

1. Check network tab for failed POST requests
2. Verify server endpoint is correct
3. Check server logs for errors
4. Ensure CORS is configured if needed

## Examples

### Example 1: Debugging a Failed Login

1. Try to login with incorrect credentials
2. Press `Ctrl+Shift+D` to open Dev Tools
3. Go to Network tab
4. Find the POST request to `/Account/Login`
5. Check status code (should be 400 or 401)
6. Look at response for error message

### Example 2: Finding a JavaScript Error

1. Click a button that doesn't work
2. Press `Ctrl+Shift+E` to see errors
3. Look for the most recent error
4. Note the filename and line number
5. Open that file and fix the issue

### Example 3: Monitoring Performance

1. Load a page
2. Press `Ctrl+Shift+D`
3. Go to Performance tab
4. Check page load time
5. If slow, check Network tab for slow requests

## Additional Resources

- [Browser DevTools Documentation](https://developer.chrome.com/docs/devtools/)
- [JavaScript Error Handling](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Control_flow_and_error_handling)
- [Network Request Debugging](https://developer.mozilla.org/en-US/docs/Tools/Network_Monitor)

## Support

If you encounter issues with the error monitoring tools:

1. Check this documentation
2. Review browser console for errors
3. Check server logs
4. Contact the development team
