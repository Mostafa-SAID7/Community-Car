# Storage Manager - Complete Guide

## Overview
Comprehensive browser storage utilities supporting all modern storage mechanisms:
- **Session Storage** - Temporary storage (cleared on tab close)
- **Local Storage** - Persistent storage with optional TTL
- **Cookies** - HTTP cookies with full options support
- **Cache Storage** - Service Worker cache API
- **IndexedDB** - Large structured data storage
- **Back/Forward Cache (BFCache)** - Browser navigation cache utilities

## Installation

Add to your layout file:

```html
<script src="~/js/storage-manager.js"></script>
```

## Usage Examples

### 1. Session Storage (Temporary)

```javascript
// Store data (cleared when tab closes)
SessionStorage.set('currentUser', { id: 123, name: 'John' });
SessionStorage.set('cartItems', ['item1', 'item2', 'item3']);

// Retrieve data
const user = SessionStorage.get('currentUser');
const cart = SessionStorage.get('cartItems', []); // with default value

// Check if exists
if (SessionStorage.has('currentUser')) {
    console.log('User is logged in');
}

// Remove item
SessionStorage.remove('cartItems');

// Clear all
SessionStorage.clear();

// Get all keys
const keys = SessionStorage.keys();
```

### 2. Local Storage (Persistent with TTL)

```javascript
// Store data permanently
LocalStorage.set('userPreferences', { theme: 'dark', language: 'en' });

// Store with expiry (1 hour = 3600000 ms)
LocalStorage.set('tempToken', 'abc123', 3600000);

// Store with expiry (7 days)
LocalStorage.set('rememberMe', true, 7 * 24 * 60 * 60 * 1000);

// Retrieve data (automatically checks expiry)
const prefs = LocalStorage.get('userPreferences');
const token = LocalStorage.get('tempToken'); // Returns null if expired

// Check if exists and not expired
if (LocalStorage.has('tempToken')) {
    console.log('Token is still valid');
}

// Remove item
LocalStorage.remove('userPreferences');

// Clear all
LocalStorage.clear();
```

### 3. Cookies

```javascript
// Set simple cookie
Cookies.set('sessionId', 'xyz789');

// Set cookie with options
Cookies.set('authToken', 'token123', {
    expires: 7,           // Days
    path: '/',
    secure: true,         // HTTPS only
    sameSite: 'Strict'    // CSRF protection
});

// Set cookie with specific expiry date
Cookies.set('promo', 'SAVE20', {
    expires: new Date('2026-12-31'),
    path: '/'
});

// Get cookie
const sessionId = Cookies.get('sessionId');

// Check if cookie exists
if (Cookies.has('authToken')) {
    console.log('User is authenticated');
}

// Get all cookies
const allCookies = Cookies.getAll();
console.log(allCookies);

// Remove cookie
Cookies.remove('sessionId');
Cookies.remove('authToken', { path: '/' }); // Match path if set
```

### 4. Cache Storage (Service Worker Cache)

```javascript
// Add response to cache
const response = await fetch('/api/data');
await CacheStorage.add('api-cache', '/api/data', response.clone());

// Get from cache
const cachedResponse = await CacheStorage.get('api-cache', '/api/data');
if (cachedResponse) {
    const data = await cachedResponse.json();
    console.log('Cached data:', data);
}

// Remove from cache
await CacheStorage.remove('api-cache', '/api/data');

// Delete entire cache
await CacheStorage.delete('api-cache');

// Get all cache names
const cacheNames = await CacheStorage.keys();
console.log('Available caches:', cacheNames);

// Check if cache exists
const exists = await CacheStorage.has('api-cache');
```

### 5. IndexedDB (Large Structured Data)

```javascript
// Open database and create object store
const db = await IndexedDB.open('myDatabase', 1, (db, event) => {
    if (!db.objectStoreNames.contains('users')) {
        db.createObjectStore('users', { keyPath: 'id', autoIncrement: true });
    }
});

// Add/Update data
await IndexedDB.put('myDatabase', 'users', {
    id: 1,
    name: 'John Doe',
    email: 'john@example.com'
});

// Get data
const user = await IndexedDB.get('myDatabase', 'users', 1);
console.log('User:', user);

// Get all data
const allUsers = await IndexedDB.getAll('myDatabase', 'users');
console.log('All users:', allUsers);

// Delete data
await IndexedDB.delete('myDatabase', 'users', 1);
```

### 6. Back/Forward Cache (BFCache)

```javascript
// Check if page was restored from BFCache
if (BFCache.isRestored()) {
    console.log('Page restored from BFCache');
    // Refresh dynamic content
    loadLatestData();
}

// Listen for BFCache restore
BFCache.onRestore((event) => {
    console.log('Page restored from BFCache');
    // Reconnect WebSockets
    reconnectWebSocket();
    // Refresh notifications
    updateNotifications();
});

// Listen for BFCache store
BFCache.onStore((event) => {
    console.log('Page being stored in BFCache');
    // Clean up resources
    closeConnections();
});

// Prevent page from being cached (use sparingly)
BFCache.preventCaching();
```

### 7. Storage Manager (Unified Interface)

```javascript
// Access all storage types
StorageManager.session.set('key', 'value');
StorageManager.local.set('key', 'value');
StorageManager.cookies.set('key', 'value');

// Get storage quota information
const quota = await StorageManager.getQuota();
console.log('Storage usage:', quota.usagePercent + '%');
console.log('Available:', quota.available + ' bytes');

// Request persistent storage (prevents eviction)
const isPersistent = await StorageManager.requestPersistent();
if (isPersistent) {
    console.log('Storage will not be evicted');
}

// Check if storage is persistent
const persisted = await StorageManager.isPersistent();

// Clear all storage types
await StorageManager.clearAll();
```

## Real-World Use Cases

### User Authentication

```javascript
// Store auth token with expiry
function login(token, rememberMe) {
    if (rememberMe) {
        // 30 days in local storage
        LocalStorage.set('authToken', token, 30 * 24 * 60 * 60 * 1000);
    } else {
        // Session only
        SessionStorage.set('authToken', token);
    }
    
    // Also set HTTP-only cookie for API calls
    Cookies.set('auth', token, {
        expires: rememberMe ? 30 : null,
        secure: true,
        sameSite: 'Strict'
    });
}

function getAuthToken() {
    return LocalStorage.get('authToken') || SessionStorage.get('authToken');
}

function logout() {
    LocalStorage.remove('authToken');
    SessionStorage.remove('authToken');
    Cookies.remove('auth');
}
```

### Shopping Cart

```javascript
// Persistent cart with IndexedDB
async function addToCart(product) {
    const cart = await IndexedDB.get('shop', 'cart', 'current') || { items: [] };
    cart.items.push(product);
    await IndexedDB.put('shop', 'cart', cart, 'current');
    
    // Also cache in session for quick access
    SessionStorage.set('cartCount', cart.items.length);
}

async function getCart() {
    return await IndexedDB.get('shop', 'cart', 'current') || { items: [] };
}
```

### Offline Support

```javascript
// Cache API responses
async function fetchWithCache(url) {
    // Try cache first
    const cached = await CacheStorage.get('api-cache', url);
    if (cached) {
        return await cached.json();
    }
    
    // Fetch from network
    const response = await fetch(url);
    
    // Cache for next time
    await CacheStorage.add('api-cache', url, response.clone());
    
    return await response.json();
}
```

### User Preferences

```javascript
// Store user preferences
function savePreferences(prefs) {
    LocalStorage.set('userPrefs', prefs);
    
    // Also set cookie for server-side rendering
    Cookies.set('theme', prefs.theme, { expires: 365 });
    Cookies.set('language', prefs.language, { expires: 365 });
}

function getPreferences() {
    return LocalStorage.get('userPrefs', {
        theme: 'light',
        language: 'en',
        notifications: true
    });
}
```

### Form Auto-Save

```javascript
// Auto-save form data
function autoSaveForm(formId) {
    const form = document.getElementById(formId);
    const inputs = form.querySelectorAll('input, textarea, select');
    
    inputs.forEach(input => {
        input.addEventListener('input', () => {
            const formData = new FormData(form);
            const data = Object.fromEntries(formData);
            SessionStorage.set(`form_${formId}`, data);
        });
    });
}

// Restore form data
function restoreForm(formId) {
    const data = SessionStorage.get(`form_${formId}`);
    if (data) {
        Object.keys(data).forEach(name => {
            const input = document.querySelector(`[name="${name}"]`);
            if (input) input.value = data[name];
        });
    }
}
```

### Analytics Tracking

```javascript
// Track user behavior
function trackEvent(eventName, data) {
    const events = LocalStorage.get('analytics_events', []);
    events.push({
        event: eventName,
        data: data,
        timestamp: Date.now()
    });
    
    // Keep last 100 events
    if (events.length > 100) {
        events.shift();
    }
    
    LocalStorage.set('analytics_events', events);
}

// Send analytics when online
async function syncAnalytics() {
    const events = LocalStorage.get('analytics_events', []);
    if (events.length > 0) {
        await fetch('/api/analytics', {
            method: 'POST',
            body: JSON.stringify(events)
        });
        LocalStorage.remove('analytics_events');
    }
}
```

## Browser Support

| Feature | Chrome | Firefox | Safari | Edge |
|---------|--------|---------|--------|------|
| Session Storage | ✅ | ✅ | ✅ | ✅ |
| Local Storage | ✅ | ✅ | ✅ | ✅ |
| Cookies | ✅ | ✅ | ✅ | ✅ |
| Cache Storage | ✅ | ✅ | ✅ | ✅ |
| IndexedDB | ✅ | ✅ | ✅ | ✅ |
| BFCache | ✅ | ✅ | ✅ | ✅ |
| Storage Manager API | ✅ | ✅ | ⚠️ | ✅ |

## Storage Limits

| Storage Type | Limit | Eviction |
|--------------|-------|----------|
| Session Storage | ~5-10 MB | Tab close |
| Local Storage | ~5-10 MB | Manual only |
| Cookies | ~4 KB per cookie | Expiry/Manual |
| Cache Storage | ~50% of disk | LRU when full |
| IndexedDB | ~50% of disk | LRU when full |

## Best Practices

1. **Use appropriate storage type**:
   - Session Storage: Temporary UI state
   - Local Storage: User preferences, small data
   - Cookies: Authentication, server-side data
   - Cache Storage: API responses, assets
   - IndexedDB: Large datasets, offline data

2. **Always handle errors**:
   ```javascript
   try {
       LocalStorage.set('key', 'value');
   } catch (error) {
       console.error('Storage failed:', error);
   }
   ```

3. **Set expiry for sensitive data**:
   ```javascript
   LocalStorage.set('token', 'abc', 3600000); // 1 hour
   ```

4. **Clear old data**:
   ```javascript
   // Clear expired items periodically
   setInterval(() => {
       LocalStorage.keys().forEach(key => {
           LocalStorage.get(key); // Triggers expiry check
       });
   }, 60000); // Every minute
   ```

5. **Monitor storage quota**:
   ```javascript
   const quota = await StorageManager.getQuota();
   if (quota.usagePercent > 80) {
       console.warn('Storage almost full!');
   }
   ```

## Security Considerations

1. **Never store sensitive data in plain text**
2. **Use secure cookies for authentication**
3. **Set appropriate SameSite and Secure flags**
4. **Validate data before storing**
5. **Sanitize data when retrieving**
6. **Use HTTPS for secure cookies**
7. **Implement CSRF protection**

## Performance Tips

1. **Batch operations** when possible
2. **Use IndexedDB for large datasets**
3. **Cache API responses** to reduce network calls
4. **Compress data** before storing
5. **Use Web Workers** for heavy IndexedDB operations
6. **Implement lazy loading** for cached data

## Troubleshooting

### Storage Quota Exceeded
```javascript
try {
    LocalStorage.set('key', largeData);
} catch (error) {
    if (error.name === 'QuotaExceededError') {
        // Clear old data
        LocalStorage.clear();
        // Or request persistent storage
        await StorageManager.requestPersistent();
    }
}
```

### Cookie Not Setting
```javascript
// Ensure path and domain match
Cookies.set('key', 'value', {
    path: '/',
    domain: window.location.hostname,
    secure: window.location.protocol === 'https:'
});
```

### BFCache Not Working
```javascript
// Avoid these to enable BFCache:
// - unload event listeners
// - beforeunload event listeners
// - Open connections (WebSocket, IndexedDB transactions)
// - Cache-Control: no-store header
```

## Summary

The Storage Manager provides a unified, type-safe interface for all browser storage mechanisms with:
- ✅ Consistent API across all storage types
- ✅ Automatic expiry handling
- ✅ Error handling and fallbacks
- ✅ TypeScript-friendly
- ✅ Production-ready
- ✅ Well-documented
- ✅ Browser-compatible

Use it to build robust, offline-capable web applications with proper data persistence and caching strategies.
