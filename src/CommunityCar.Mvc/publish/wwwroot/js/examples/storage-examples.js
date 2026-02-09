/**
 * Storage Manager - Practical Examples for CommunityCar
 * Demonstrates real-world usage patterns
 */

// ============================================================================
// 1. USER PREFERENCES
// ============================================================================
const UserPreferences = {
    /**
     * Save user preferences (theme, language, layout)
     */
    save(preferences) {
        // Store in local storage with 1 year expiry
        LocalStorage.set('userPreferences', preferences, 365 * 24 * 60 * 60 * 1000);
        
        // Also set cookies for server-side rendering
        Cookies.set('theme', preferences.theme, { expires: 365, path: '/' });
        Cookies.set('language', preferences.language, { expires: 365, path: '/' });
        
        console.log('Preferences saved:', preferences);
    },

    /**
     * Load user preferences
     */
    load() {
        return LocalStorage.get('userPreferences', {
            theme: 'light',
            language: 'en',
            sidebarCollapsed: false,
            notificationsEnabled: true,
            emailNotifications: true
        });
    },

    /**
     * Update specific preference
     */
    update(key, value) {
        const prefs = this.load();
        prefs[key] = value;
        this.save(prefs);
    }
};

// ============================================================================
// 2. AUTHENTICATION STATE
// ============================================================================
const AuthManager = {
    /**
     * Store authentication token
     */
    login(token, rememberMe = false) {
        if (rememberMe) {
            // 30 days in local storage
            LocalStorage.set('authToken', token, 30 * 24 * 60 * 60 * 1000);
        } else {
            // Session only
            SessionStorage.set('authToken', token);
        }
        
        // Set HTTP cookie for API calls
        Cookies.set('auth', token, {
            expires: rememberMe ? 30 : null,
            path: '/',
            secure: window.location.protocol === 'https:',
            sameSite: 'Strict'
        });
        
        console.log('User logged in');
    },

    /**
     * Get authentication token
     */
    getToken() {
        return LocalStorage.get('authToken') || SessionStorage.get('authToken');
    },

    /**
     * Check if user is authenticated
     */
    isAuthenticated() {
        return this.getToken() !== null;
    },

    /**
     * Logout user
     */
    logout() {
        LocalStorage.remove('authToken');
        SessionStorage.remove('authToken');
        Cookies.remove('auth', { path: '/' });
        SessionStorage.clear(); // Clear all session data
        console.log('User logged out');
    }
};

// ============================================================================
// 3. FORM AUTO-SAVE
// ============================================================================
const FormAutoSave = {
    /**
     * Enable auto-save for a form
     */
    enable(formId, interval = 5000) {
        const form = document.getElementById(formId);
        if (!form) return;

        // Save on input
        form.addEventListener('input', () => {
            this.save(formId);
        });

        // Auto-save every interval
        setInterval(() => {
            this.save(formId);
        }, interval);

        // Restore on load
        this.restore(formId);
    },

    /**
     * Save form data
     */
    save(formId) {
        const form = document.getElementById(formId);
        if (!form) return;

        const formData = new FormData(form);
        const data = Object.fromEntries(formData);
        
        SessionStorage.set(`form_${formId}`, {
            data: data,
            timestamp: Date.now()
        });
    },

    /**
     * Restore form data
     */
    restore(formId) {
        const saved = SessionStorage.get(`form_${formId}`);
        if (!saved) return;

        const form = document.getElementById(formId);
        if (!form) return;

        Object.keys(saved.data).forEach(name => {
            const input = form.querySelector(`[name="${name}"]`);
            if (input && input.type !== 'file') {
                input.value = saved.data[name];
            }
        });

        // Show notification
        const minutes = Math.floor((Date.now() - saved.timestamp) / 60000);
        console.log(`Form restored from ${minutes} minutes ago`);
    },

    /**
     * Clear saved form data
     */
    clear(formId) {
        SessionStorage.remove(`form_${formId}`);
    }
};

// ============================================================================
// 4. NOTIFICATION CACHE
// ============================================================================
const NotificationCache = {
    /**
     * Cache notifications
     */
    async cache(notifications) {
        await IndexedDB.open('communitycar', 1, (db) => {
            if (!db.objectStoreNames.contains('notifications')) {
                db.createObjectStore('notifications', { keyPath: 'id' });
            }
        });

        for (const notification of notifications) {
            await IndexedDB.put('communitycar', 'notifications', notification);
        }
    },

    /**
     * Get cached notifications
     */
    async get() {
        try {
            return await IndexedDB.getAll('communitycar', 'notifications');
        } catch (error) {
            console.error('Failed to get cached notifications:', error);
            return [];
        }
    },

    /**
     * Mark notification as read
     */
    async markAsRead(notificationId) {
        const notification = await IndexedDB.get('communitycar', 'notifications', notificationId);
        if (notification) {
            notification.isRead = true;
            await IndexedDB.put('communitycar', 'notifications', notification);
        }
    },

    /**
     * Clear old notifications
     */
    async clearOld(daysOld = 30) {
        const notifications = await this.get();
        const cutoff = Date.now() - (daysOld * 24 * 60 * 60 * 1000);

        for (const notification of notifications) {
            if (new Date(notification.createdAt).getTime() < cutoff) {
                await IndexedDB.delete('communitycar', 'notifications', notification.id);
            }
        }
    }
};

// ============================================================================
// 5. OFFLINE QUEUE
// ============================================================================
const OfflineQueue = {
    /**
     * Add action to offline queue
     */
    async add(action) {
        await IndexedDB.open('communitycar', 1, (db) => {
            if (!db.objectStoreNames.contains('offlineQueue')) {
                db.createObjectStore('offlineQueue', { keyPath: 'id', autoIncrement: true });
            }
        });

        await IndexedDB.put('communitycar', 'offlineQueue', {
            ...action,
            timestamp: Date.now()
        });

        console.log('Action queued for offline sync:', action);
    },

    /**
     * Process offline queue when online
     */
    async process() {
        const queue = await IndexedDB.getAll('communitycar', 'offlineQueue');
        
        for (const action of queue) {
            try {
                // Execute action
                await fetch(action.url, {
                    method: action.method,
                    headers: action.headers,
                    body: action.body
                });

                // Remove from queue on success
                await IndexedDB.delete('communitycar', 'offlineQueue', action.id);
                console.log('Offline action synced:', action);
            } catch (error) {
                console.error('Failed to sync offline action:', error);
            }
        }
    },

    /**
     * Get queue size
     */
    async size() {
        const queue = await IndexedDB.getAll('communitycar', 'offlineQueue');
        return queue.length;
    }
};

// ============================================================================
// 6. SEARCH HISTORY
// ============================================================================
const SearchHistory = {
    /**
     * Add search query to history
     */
    add(query, category = 'all') {
        const history = LocalStorage.get('searchHistory', []);
        
        // Add to beginning
        history.unshift({
            query: query,
            category: category,
            timestamp: Date.now()
        });

        // Keep only last 50 searches
        if (history.length > 50) {
            history.pop();
        }

        LocalStorage.set('searchHistory', history);
    },

    /**
     * Get search history
     */
    get(limit = 10) {
        const history = LocalStorage.get('searchHistory', []);
        return history.slice(0, limit);
    },

    /**
     * Clear search history
     */
    clear() {
        LocalStorage.remove('searchHistory');
    },

    /**
     * Get popular searches
     */
    getPopular(limit = 5) {
        const history = LocalStorage.get('searchHistory', []);
        const counts = {};

        history.forEach(item => {
            counts[item.query] = (counts[item.query] || 0) + 1;
        });

        return Object.entries(counts)
            .sort((a, b) => b[1] - a[1])
            .slice(0, limit)
            .map(([query]) => query);
    }
};

// ============================================================================
// 7. DRAFT POSTS
// ============================================================================
const DraftManager = {
    /**
     * Save draft post
     */
    async save(type, content) {
        await IndexedDB.open('communitycar', 1, (db) => {
            if (!db.objectStoreNames.contains('drafts')) {
                db.createObjectStore('drafts', { keyPath: 'id', autoIncrement: true });
            }
        });

        await IndexedDB.put('communitycar', 'drafts', {
            type: type,
            content: content,
            timestamp: Date.now()
        });

        console.log('Draft saved:', type);
    },

    /**
     * Get all drafts
     */
    async getAll() {
        try {
            return await IndexedDB.getAll('communitycar', 'drafts');
        } catch (error) {
            return [];
        }
    },

    /**
     * Get drafts by type
     */
    async getByType(type) {
        const drafts = await this.getAll();
        return drafts.filter(draft => draft.type === type);
    },

    /**
     * Delete draft
     */
    async delete(draftId) {
        await IndexedDB.delete('communitycar', 'drafts', draftId);
    }
};

// ============================================================================
// 8. ANALYTICS TRACKING
// ============================================================================
const Analytics = {
    /**
     * Track page view
     */
    trackPageView(page) {
        const views = LocalStorage.get('pageViews', []);
        views.push({
            page: page,
            timestamp: Date.now(),
            referrer: document.referrer
        });

        // Keep last 100 views
        if (views.length > 100) {
            views.shift();
        }

        LocalStorage.set('pageViews', views);
    },

    /**
     * Track event
     */
    trackEvent(eventName, data = {}) {
        const events = LocalStorage.get('analyticsEvents', []);
        events.push({
            event: eventName,
            data: data,
            timestamp: Date.now()
        });

        // Keep last 100 events
        if (events.length > 100) {
            events.shift();
        }

        LocalStorage.set('analyticsEvents', events);
    },

    /**
     * Sync analytics to server
     */
    async sync() {
        const views = LocalStorage.get('pageViews', []);
        const events = LocalStorage.get('analyticsEvents', []);

        if (views.length > 0 || events.length > 0) {
            try {
                await fetch('/api/analytics', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ views, events })
                });

                // Clear after successful sync
                LocalStorage.remove('pageViews');
                LocalStorage.remove('analyticsEvents');
                console.log('Analytics synced');
            } catch (error) {
                console.error('Failed to sync analytics:', error);
            }
        }
    }
};

// ============================================================================
// 9. BFCACHE HANDLER
// ============================================================================
const BFCacheHandler = {
    /**
     * Initialize BFCache handlers
     */
    init() {
        // Restore dynamic content when page is restored from BFCache
        BFCache.onRestore((event) => {
            console.log('Page restored from BFCache');
            
            // Refresh notifications
            if (typeof window.updateUnreadCount === 'function') {
                window.updateUnreadCount();
            }
            
            // Reconnect SignalR if needed
            this.reconnectSignalR();
            
            // Refresh timestamps
            this.updateTimestamps();
        });

        // Clean up before page is stored in BFCache
        BFCache.onStore((event) => {
            console.log('Page being stored in BFCache');
            // Close any open connections
        });
    },

    /**
     * Reconnect SignalR connections
     */
    reconnectSignalR() {
        // Implementation depends on your SignalR setup
        console.log('Reconnecting SignalR...');
    },

    /**
     * Update relative timestamps
     */
    updateTimestamps() {
        document.querySelectorAll('[data-timestamp]').forEach(el => {
            const timestamp = parseInt(el.dataset.timestamp);
            el.textContent = this.formatRelativeTime(timestamp);
        });
    },

    /**
     * Format relative time
     */
    formatRelativeTime(timestamp) {
        const seconds = Math.floor((Date.now() - timestamp) / 1000);
        
        if (seconds < 60) return 'just now';
        if (seconds < 3600) return Math.floor(seconds / 60) + ' minutes ago';
        if (seconds < 86400) return Math.floor(seconds / 3600) + ' hours ago';
        return Math.floor(seconds / 86400) + ' days ago';
    }
};

// ============================================================================
// 10. STORAGE MONITOR
// ============================================================================
const StorageMonitor = {
    /**
     * Check storage quota and warn if low
     */
    async check() {
        const quota = await StorageManager.getQuota();
        
        if (quota) {
            console.log('Storage usage:', quota.usagePercent + '%');
            
            if (quota.usagePercent > 80) {
                console.warn('Storage almost full! Consider clearing old data.');
                this.showWarning();
            }
        }
    },

    /**
     * Show storage warning to user
     */
    showWarning() {
        if (window.Toast) {
            window.Toast.show(
                'Storage is almost full. Some features may not work properly.',
                'warning',
                'Storage Warning'
            );
        }
    },

    /**
     * Clean up old data
     */
    async cleanup() {
        // Clear old notifications
        await NotificationCache.clearOld(30);
        
        // Clear old analytics
        await Analytics.sync();
        
        // Clear expired local storage items
        LocalStorage.keys().forEach(key => {
            LocalStorage.get(key); // Triggers expiry check
        });
        
        console.log('Storage cleanup completed');
    }
};

// ============================================================================
// INITIALIZATION
// ============================================================================
document.addEventListener('DOMContentLoaded', function() {
    // Initialize BFCache handlers
    BFCacheHandler.init();
    
    // Check storage quota
    StorageMonitor.check();
    
    // Sync analytics periodically
    setInterval(() => Analytics.sync(), 5 * 60 * 1000); // Every 5 minutes
    
    // Process offline queue when online
    window.addEventListener('online', () => {
        console.log('Connection restored');
        OfflineQueue.process();
    });
    
    // Track page view
    Analytics.trackPageView(window.location.pathname);
    
    console.log('Storage Manager initialized');
});

// Export for global use
window.UserPreferences = UserPreferences;
window.AuthManager = AuthManager;
window.FormAutoSave = FormAutoSave;
window.NotificationCache = NotificationCache;
window.OfflineQueue = OfflineQueue;
window.SearchHistory = SearchHistory;
window.DraftManager = DraftManager;
window.Analytics = Analytics;
window.BFCacheHandler = BFCacheHandler;
window.StorageMonitor = StorageMonitor;
