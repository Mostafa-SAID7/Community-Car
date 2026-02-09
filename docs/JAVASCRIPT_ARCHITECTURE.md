# JavaScript Clean Architecture Guide

## Overview

The CommunityCar application uses a clean, modular JavaScript architecture to eliminate code duplication and improve maintainability.

## Architecture Layers

### 1. Core Layer (`/js/core/`)

Foundation modules that provide essential functionality.

#### `api-client.js`
Centralized HTTP client with interceptors and error handling.

```javascript
// Usage
await window.api.get('/api/posts');
await window.api.post('/api/posts', { title: 'New Post' });
await window.api.put('/api/posts/1', { title: 'Updated' }
│   ├── validators.js       # Form validation utilities
│   ├── error-monitor.js    # Error monitoring system
│   ├── dev-tools.js        # Developer tools panel
│   └── theme-manager.js    # Theme management
│
├── components/              # Reusable UI components
│   ├── modal.js            # Modal component
│   ├── toast.js            # Toast notifications
│   ├── dropdown.js         # Dropdown component
│   ├── notification.js     # Notification system
│   └── tag-input.js        # Tag input component
│
├── pages/                   # Page-specific modules
│   ├── home.js             # Home page logic
│   ├── dashboard.js        # Dashboard page logic
│   ├── friends.js          # Friends page logic
│   └── login.js            # Login page logic
│
└── layout/                  # Layout components
    ├── header.js           # Header component
    ├── sidebar.js          # Sidebar component
    └── navigation.js       # Navigation component
```

## Core Modules

### 1. App (app.js)

Main application controller that manages initialization and module registration.

**Usage:**
```javascript
// Access the app instance
window.app

// Register a module
window.app.register('myModule', myModuleInstance);

// Get a module
const api = window.app.get('api');

// Check debug mode
if (window.app.isDebug()) {
    console.log('Debug mode enabled');
}
```

### 2. API Client (api-client.js)

Centralized HTTP client that eliminates duplicate AJAX code.

**Usage:**
```javascript
// GET request
const data = await window.api.get('/api/posts');

// POST request
const result = await window.api.post('/api/posts', {
    title: 'My Post',
    content: 'Post content'
});

// PUT request
await window.api.put('/api/posts/123', { title: 'Updated' });

// DELETE request
await window.api.delete('/api/posts/123');

// Upload file
const formData = new FormData();
formData.append('file', fileInput.files[0]);
await window.api.upload('/api/upload', formData);

// Add interceptors
window.api.addRequestInterceptor(async (config) => {
    // Modify request before sending
    config.headers['X-Custom-Header'] = 'value';
    return config;
});

window.api.addResponseInterceptor(async (response) => {
    // Process response
    return response;
});

window.api.addErrorInterceptor(async (error, config) => {
    // Handle errors globally
    console.error('API Error:', error);
});
```

### 3. Event Bus (event-bus.js)

Centralized event management system for decoupled communication.

**Usage:**
```javascript
// Subscribe to event
const subscriptionId = window.eventBus.on('user:logged-in', (user) => {
    console.log('User logged in:', user);
});

// Subscribe once
window.eventBus.once('modal:opened', () => {
    console.log('Modal opened (one-time)');
});

// Emit event
window.eventBus.emit('post:created', { id: 123, title: 'New Post' });

// Unsubscribe
window.eventBus.off('user:logged-in', subscriptionId);

// Use predefined events
window.eventBus.on(window.AppEvents.USER_LOGGED_IN, handleLogin);
```

## Utility Modules

### 1. Common Utils (common-utils.js)

Shared utility functions for common tasks.

**Usage:**
```javascript
// Debounce function
const debouncedSearch = window.utils.debounce(searchFunction, 300);

// Throttle function
const throttledScroll = window.utils.throttle(scrollHandler, 100);

// Format date
const formatted = window.utils.formatDate(new Date(), 'relative'); // "2 hours ago"

// Truncate text
const short = window.utils.truncate(longText, 100);

// Strip HTML
const plain = window.utils.stripHtml('<p>Hello</p>'); // "Hello"

// Copy to clipboard
await window.utils.copyToClipboard('Text to copy');

// Validate email
if (window.utils.isValidEmail(email)) {
    // Valid email
}

// Format file size
const size = window.utils.formatFileSize(1024000); // "1 MB"

// Retry with backoff
const result = await window.utils.retry(async () => {
    return await fetchData();
}, 3, 1000);
```

### 2. DOM Utils (dom-utils.js)

DOM manipulation utilities to replace jQuery-like operations.

**Usage:**
```javascript
// Query selectors
const element = window.dom.$('#myElement');
const elements = window.dom.$$('.my-class');

// Create element
const div = window.dom.createElement('div', {
    className: 'my-class',
    id: 'my-id',
    dataset: { value: '123' },
    onClick: handleClick
}, ['Child text']);

// Class manipulation
window.dom.addClass(element, 'active', 'visible');
window.dom.removeClass(element, 'hidden');
window.dom.toggleClass(element, 'expanded');

// Show/hide
window.dom.show(element);
window.dom.hide(element);
window.dom.toggle(element);

// Attributes
window.dom.attr(element, 'data-id', '123');
const id = window.dom.data(element, 'id');

// Content
window.dom.html(element, '<p>HTML content</p>');
window.dom.text(element, 'Text content');
const value = window.dom.val(input);

// Events
window.dom.on(button, 'click', handleClick);
window.dom.off(button, 'click', handleClick);
window.dom.trigger(element, 'custom-event', { data: 'value' });

// Event delegation
window.dom.delegate(parent, '.child', 'click', handleClick);

// Scroll to element
window.dom.scrollTo(element, { smooth: true });

// Check if in viewport
if (window.dom.isInViewport(element)) {
    // Element is visible
}

// Form serialization
const formData = window.dom.serializeForm(form);
```

## Best Practices

### 1. Avoid Code Duplication

**❌ Bad - Duplicate AJAX code:**
```javascript
// In multiple files
fetch('/api/posts', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
})
.then(response => response.json())
.then(data => console.log(data))
.catch(error => console.error(error));
```

**✅ Good - Use API client:**
```javascript
// Everywhere
const data = await window.api.post('/api/posts', postData);
```

### 2. Use Event Bus for Communication

**❌ Bad - Tight coupling:**
```javascript
// Component A directly calls Component B
window.ComponentB.updateData(newData);
```

**✅ Good - Loose coupling with events:**
```javascript
// Component A emits event
window.eventBus.emit('data:updated', newData);

// Component B listens for event
window.eventBus.on('data:updated', (data) => {
    this.updateData(data);
});
```

### 3. Use Utility Functions

**❌ Bad - Duplicate utility code:**
```javascript
// In multiple files
function formatDate(date) {
    return new Date(date).toLocaleDateString();
}
```

**✅ Good - Use shared utilities:**
```javascript
// Everywhere
const formatted = window.utils.formatDate(date);
```

### 4. Module Pattern

**✅ Good - Modular structure:**
```javascript
// my-module.js
class MyModule {
    constructor() {
        this.data = [];
    }

    init() {
        this.bindEvents();
        this.loadData();
    }

    bindEvents() {
        window.dom.on(this.getButton(), 'click', () => this.handleClick());
    }

    async loadData() {
        this.data = await window.api.get('/api/data');
    }

    handleClick() {
        window.eventBus.emit('module:clicked');
    }

    getButton() {
        return window.dom.$('#myButton');
    }
}

// Register with app
window.app.register('myModule', new MyModule());
```

### 5. Page-Specific Code

**✅ Good - Page module:**
```javascript
// pages/friends.js
class FriendsPage {
    constructor() {
        this.friends = [];
    }

    async init() {
        await this.loadFriends();
        this.bindEvents();
    }

    async loadFriends() {
        this.friends = await window.api.get('/api/friends');
        this.render();
    }

    bindEvents() {
        window.dom.delegate(document, '.friend-card', 'click', (e) => {
            this.handleFriendClick(e);
        });
    }

    render() {
        const container = window.dom.$('#friends-container');
        window.dom.empty(container);
        
        this.friends.forEach(friend => {
            const card = this.createFriendCard(friend);
            window.dom.append(container, card);
        });
    }

    createFriendCard(friend) {
        return window.dom.createElement('div', {
            className: 'friend-card',
            dataset: { id: friend.id }
        }, [friend.name]);
    }

    handleFriendClick(e) {
        const card = e.target.closest('.friend-card');
        const friendId = window.dom.data(card, 'id');
        window.eventBus.emit('friend:selected', friendId);
    }
}

// Auto-register if on friends page
if (document.body.dataset.page === 'friends') {
    window.FriendsPage = FriendsPage;
}
```

## Script Loading Order

Add scripts to `_Layout.cshtml` in this order:

```html
<!-- Core utilities (load first) -->
<script src="~/js/utils/common-utils.js"></script>
<script src="~/js/utils/dom-utils.js"></script>

<!-- Core modules -->
<script src="~/js/core/api-client.js"></script>
<script src="~/js/core/event-bus.js"></script>

<!-- Components -->
<script src="~/js/components/toast.js"></script>
<script src="~/js/components/modal.js"></script>
<script src="~/js/components/notification.js"></script>

<!-- Layout -->
<script src="~/js/layout/header.js"></script>
<script src="~/js/layout/sidebar.js"></script>

<!-- Error monitoring (development) -->
<script src="~/js/utils/error-monitor.js"></script>
<script src="~/js/utils/dev-tools.js"></script>

<!-- Main app (load last) -->
<script src="~/js/core/app.js"></script>

<!-- Page-specific scripts -->
@await RenderSectionAsync("Scripts", required: false)
```

## Migration Guide

### Migrating Existing Code

1. **Identify duplicate code** across files
2. **Extract to utilities** if it's a common function
3. **Use API client** instead of raw fetch/XMLHttpRequest
4. **Use event bus** instead of direct function calls
5. **Use DOM utils** instead of vanilla DOM manipulation

### Example Migration

**Before:**
```javascript
// Duplicate in multiple files
function showNotification(message) {
    const div = document.createElement('div');
    div.className = 'notification';
    div.textContent = message;
    document.body.appendChild(div);
    setTimeout(() => div.remove(), 3000);
}

fetch('/api/posts', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ title: 'Post' })
})
.then(r => r.json())
.then(data => showNotification('Post created'));
```

**After:**
```javascript
// Use shared utilities
const data = await window.api.post('/api/posts', { title: 'Post' });
window.Toast.show('Post created', 'success');
window.eventBus.emit(window.AppEvents.POST_CREATED, data);
```

## Debugging

### Enable Debug Mode

Add `?debug=true` to any URL to enable:
- Detailed console logging
- API request/response logging
- Event bus activity logging
- Performance metrics

### Developer Tools

Press `Ctrl+Shift+D` to open the developer tools panel with:
- Error monitoring
- Network request tracking
- Performance metrics
- Console access

## Testing

### Unit Testing

```javascript
// Test utility function
describe('CommonUtils', () => {
    it('should format date correctly', () => {
        const date = new Date('2024-01-01');
        const formatted = window.utils.formatDate(date, 'short');
        expect(formatted).toBe('1/1/2024');
    });
});
```

### Integration Testing

```javascript
// Test API client
describe('ApiClient', () => {
    it('should make GET request', async () => {
        const data = await window.api.get('/api/test');
        expect(data).toBeDefined();
    });
});
```

## Performance Optimization

1. **Lazy load** page-specific modules
2. **Debounce** expensive operations
3. **Throttle** scroll/resize handlers
4. **Cache** API responses when appropriate
5. **Use event delegation** instead of multiple listeners

## Support

For questions or issues with the JavaScript architecture:
1. Check this documentation
2. Review the code examples
3. Check browser console for errors
4. Use developer tools (`Ctrl+Shift+D`)
5. Contact the development team
