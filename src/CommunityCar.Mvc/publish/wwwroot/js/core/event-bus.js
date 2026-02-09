/**
 * Event Bus - Centralized event management
 * Eliminates duplicate event handling code
 */

class EventBus {
    constructor() {
        this.events = new Map();
    }

    /**
     * Subscribe to an event
     */
    on(event, callback, context = null) {
        if (!this.events.has(event)) {
            this.events.set(event, []);
        }

        const subscription = {
            callback,
            context,
            id: this.generateId()
        };

        this.events.get(event).push(subscription);
        return subscription.id;
    }

    /**
     * Subscribe to an event (one-time)
     */
    once(event, callback, context = null) {
        const wrappedCallback = (...args) => {
            callback.apply(context, args);
            this.off(event, wrappedCallback);
        };
        return this.on(event, wrappedCallback, context);
    }

    /**
     * Unsubscribe from an event
     */
    off(event, callbackOrId) {
        if (!this.events.has(event)) return;

        const subscriptions = this.events.get(event);
        
        if (typeof callbackOrId === 'string') {
            // Remove by ID
            const index = subscriptions.findIndex(sub => sub.id === callbackOrId);
            if (index !== -1) {
                subscriptions.splice(index, 1);
            }
        } else {
            // Remove by callback
            const index = subscriptions.findIndex(sub => sub.callback === callbackOrId);
            if (index !== -1) {
                subscriptions.splice(index, 1);
            }
        }

        if (subscriptions.length === 0) {
            this.events.delete(event);
        }
    }

    /**
     * Emit an event
     */
    emit(event, ...args) {
        if (!this.events.has(event)) return;

        const subscriptions = this.events.get(event);
        subscriptions.forEach(sub => {
            try {
                sub.callback.apply(sub.context, args);
            } catch (error) {
                console.error(`Error in event handler for "${event}":`, error);
            }
        });
    }

    /**
     * Clear all subscriptions for an event
     */
    clear(event) {
        if (event) {
            this.events.delete(event);
        } else {
            this.events.clear();
        }
    }

    /**
     * Get all event names
     */
    getEvents() {
        return Array.from(this.events.keys());
    }

    /**
     * Generate unique ID
     */
    generateId() {
        return `${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    }
}

// Create global instance
window.eventBus = new EventBus();

// Common application events
window.AppEvents = {
    // User events
    USER_LOGGED_IN: 'user:logged-in',
    USER_LOGGED_OUT: 'user:logged-out',
    USER_PROFILE_UPDATED: 'user:profile-updated',
    
    // Content events
    POST_CREATED: 'post:created',
    POST_UPDATED: 'post:updated',
    POST_DELETED: 'post:deleted',
    COMMENT_ADDED: 'comment:added',
    
    // Notification events
    NOTIFICATION_RECEIVED: 'notification:received',
    NOTIFICATION_READ: 'notification:read',
    
    // Friend events
    FRIEND_REQUEST_SENT: 'friend:request-sent',
    FRIEND_REQUEST_ACCEPTED: 'friend:request-accepted',
    FRIEND_ONLINE: 'friend:online',
    FRIEND_OFFLINE: 'friend:offline',
    
    // UI events
    MODAL_OPENED: 'modal:opened',
    MODAL_CLOSED: 'modal:closed',
    SIDEBAR_TOGGLED: 'sidebar:toggled',
    THEME_CHANGED: 'theme:changed',
    
    // Data events
    DATA_LOADED: 'data:loaded',
    DATA_ERROR: 'data:error'
};
