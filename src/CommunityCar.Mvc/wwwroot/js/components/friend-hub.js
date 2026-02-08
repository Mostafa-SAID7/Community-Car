/**
 * FriendHub SignalR Client
 * Handles real-time friend-related notifications
 */

class FriendHubClient {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
    }

    /**
     * Initialize and start the SignalR connection
     */
    async initialize() {
        // Only initialize if user is authenticated (check for friend-related elements)
        const friendElements = document.querySelector('[data-friend-hub]');
        if (!friendElements) {
            console.debug('FriendHub: User not authenticated or not on friend page, skipping initialization');
            return;
        }

        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl('/friendHub')
                .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
                .configureLogging(signalR.LogLevel.Information)
                .build();

            this.setupEventHandlers();
            await this.start();
        } catch (error) {
            console.error('FriendHub: Failed to initialize:', error);
        }
    }

    /**
     * Start the SignalR connection
     */
    async start() {
        try {
            await this.connection.start();
            this.isConnected = true;
            this.reconnectAttempts = 0;
            console.debug('FriendHub: Connected successfully');
        } catch (error) {
            console.error('FriendHub: Connection failed:', error);
            this.isConnected = false;
            
            // Retry connection
            if (this.reconnectAttempts < this.maxReconnectAttempts) {
                this.reconnectAttempts++;
                const delay = Math.min(1000 * Math.pow(2, this.reconnectAttempts), 30000);
                console.debug(`FriendHub: Retrying connection in ${delay}ms (attempt ${this.reconnectAttempts})`);
                setTimeout(() => this.start(), delay);
            }
        }
    }

    /**
     * Setup all SignalR event handlers
     */
    setupEventHandlers() {
        // Connection lifecycle events
        this.connection.onreconnecting((error) => {
            console.debug('FriendHub: Reconnecting...', error);
            this.isConnected = false;
        });

        this.connection.onreconnected((connectionId) => {
            console.debug('FriendHub: Reconnected', connectionId);
            this.isConnected = true;
            this.reconnectAttempts = 0;
        });

        this.connection.onclose((error) => {
            console.debug('FriendHub: Connection closed', error);
            this.isConnected = false;
        });

        // Friend request events
        this.connection.on('ReceiveFriendRequest', (data) => {
            this.handleFriendRequest(data);
        });

        this.connection.on('FriendRequestAccepted', (data) => {
            this.handleFriendRequestAccepted(data);
        });

        this.connection.on('FriendRequestRejected', (data) => {
            this.handleFriendRequestRejected(data);
        });

        // Block/Unblock events
        this.connection.on('UserBlocked', (data) => {
            this.handleUserBlocked(data);
        });

        this.connection.on('UserUnblocked', (data) => {
            this.handleUserUnblocked(data);
        });

        // Friendship events
        this.connection.on('FriendshipRemoved', (data) => {
            this.handleFriendshipRemoved(data);
        });

        // Status events
        this.connection.on('UserOnline', (userId) => {
            this.handleUserOnline(userId);
        });

        this.connection.on('UserOffline', (userId) => {
            this.handleUserOffline(userId);
        });

        this.connection.on('FriendStatusChanged', (data) => {
            this.handleFriendStatusChanged(data);
        });

        // Profile update events
        this.connection.on('FriendProfileUpdated', (data) => {
            this.handleFriendProfileUpdated(data);
        });

        // Suggestion events
        this.connection.on('NewFriendSuggestions', (data) => {
            this.handleNewSuggestions(data);
        });

        // System announcements
        this.connection.on('SystemAnnouncement', (data) => {
            this.handleSystemAnnouncement(data);
        });
    }

    /**
     * Handle incoming friend request
     */
    handleFriendRequest(data) {
        console.debug('FriendHub: Received friend request', data);
        
        // Show notification
        this.showNotification('New Friend Request', `${data.senderName} sent you a friend request`, 'info');
        
        // Update pending request count
        this.updatePendingRequestCount(1);
        
        // Trigger custom event for other components
        document.dispatchEvent(new CustomEvent('friendRequestReceived', { detail: data }));
    }

    /**
     * Handle friend request accepted
     */
    handleFriendRequestAccepted(data) {
        console.debug('FriendHub: Friend request accepted', data);
        
        // Show notification
        this.showNotification('Friend Request Accepted', `${data.friendName} accepted your friend request`, 'success');
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('friendRequestAccepted', { detail: data }));
    }

    /**
     * Handle friend request rejected
     */
    handleFriendRequestRejected(data) {
        console.debug('FriendHub: Friend request rejected', data);
        
        // Show notification
        this.showNotification('Friend Request Rejected', `${data.userName} rejected your friend request`, 'warning');
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('friendRequestRejected', { detail: data }));
    }

    /**
     * Handle user blocked
     */
    handleUserBlocked(data) {
        console.debug('FriendHub: User blocked', data);
        
        // Show notification
        this.showNotification('Blocked', 'You have been blocked by a user', 'warning');
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('userBlocked', { detail: data }));
    }

    /**
     * Handle user unblocked
     */
    handleUserUnblocked(data) {
        console.debug('FriendHub: User unblocked', data);
        
        // Show notification
        this.showNotification('Unblocked', 'You have been unblocked by a user', 'info');
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('userUnblocked', { detail: data }));
    }

    /**
     * Handle friendship removed
     */
    handleFriendshipRemoved(data) {
        console.debug('FriendHub: Friendship removed', data);
        
        // Show notification
        this.showNotification('Friendship Ended', `${data.removedByName} removed you from their friends list`, 'warning');
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('friendshipRemoved', { detail: data }));
    }

    /**
     * Handle user online status
     */
    handleUserOnline(userId) {
        console.debug('FriendHub: User online', userId);
        
        // Update UI to show user is online
        const userElements = document.querySelectorAll(`[data-user-id="${userId}"]`);
        userElements.forEach(element => {
            element.classList.add('user-online');
            element.classList.remove('user-offline');
            
            const statusIndicator = element.querySelector('.status-indicator');
            if (statusIndicator) {
                statusIndicator.classList.add('online');
                statusIndicator.classList.remove('offline');
            }
        });
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('userOnline', { detail: { userId } }));
    }

    /**
     * Handle user offline status
     */
    handleUserOffline(userId) {
        console.debug('FriendHub: User offline', userId);
        
        // Update UI to show user is offline
        const userElements = document.querySelectorAll(`[data-user-id="${userId}"]`);
        userElements.forEach(element => {
            element.classList.remove('user-online');
            element.classList.add('user-offline');
            
            const statusIndicator = element.querySelector('.status-indicator');
            if (statusIndicator) {
                statusIndicator.classList.remove('online');
                statusIndicator.classList.add('offline');
            }
        });
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('userOffline', { detail: { userId } }));
    }

    /**
     * Handle friend status changed
     */
    handleFriendStatusChanged(data) {
        console.debug('FriendHub: Friend status changed', data);
        
        // Update UI to show friend's status
        const userElements = document.querySelectorAll(`[data-user-id="${data.friendId}"]`);
        userElements.forEach(element => {
            const statusIndicator = element.querySelector('.status-indicator');
            if (statusIndicator) {
                statusIndicator.className = 'status-indicator';
                statusIndicator.classList.add(data.status.toLowerCase());
            }
        });
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('friendStatusChanged', { detail: data }));
    }

    /**
     * Handle friend profile updated
     */
    handleFriendProfileUpdated(data) {
        console.debug('FriendHub: Friend profile updated', data);
        
        // Update UI with new profile information
        const userElements = document.querySelectorAll(`[data-user-id="${data.friendId}"]`);
        userElements.forEach(element => {
            const nameElement = element.querySelector('.user-name');
            if (nameElement) {
                nameElement.textContent = data.friendName;
            }
            
            const avatarElement = element.querySelector('.user-avatar');
            if (avatarElement && data.friendProfilePicture) {
                avatarElement.src = data.friendProfilePicture;
            }
        });
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('friendProfileUpdated', { detail: data }));
    }

    /**
     * Handle new friend suggestions
     */
    handleNewSuggestions(data) {
        console.debug('FriendHub: New friend suggestions', data);
        
        // Show notification
        this.showNotification('New Friend Suggestions', `You have ${data.count} new friend suggestions`, 'info');
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('newFriendSuggestions', { detail: data }));
    }

    /**
     * Handle system announcement
     */
    handleSystemAnnouncement(data) {
        console.debug('FriendHub: System announcement', data);
        
        // Show notification
        this.showNotification('System Announcement', data.message, data.type);
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('systemAnnouncement', { detail: data }));
    }

    /**
     * Update pending request count in UI
     */
    updatePendingRequestCount(increment = 0) {
        const countElement = document.querySelector('.friend-request-count');
        if (countElement) {
            const currentCount = parseInt(countElement.textContent) || 0;
            const newCount = currentCount + increment;
            countElement.textContent = newCount;
            
            if (newCount > 0) {
                countElement.style.display = 'inline-block';
            } else {
                countElement.style.display = 'none';
            }
        }
    }

    /**
     * Show notification to user
     */
    showNotification(title, message, type = 'info') {
        // Check if browser supports notifications
        if ('Notification' in window && Notification.permission === 'granted') {
            new Notification(title, {
                body: message,
                icon: '/images/logo.png'
            });
        }
        
        // Also show in-app notification
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show friend-notification`;
        notification.innerHTML = `
            <strong>${title}</strong> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        const container = document.querySelector('.notification-container') || document.body;
        container.appendChild(notification);
        
        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => notification.remove(), 300);
        }, 5000);
    }

    /**
     * Stop the connection
     */
    async stop() {
        if (this.connection) {
            try {
                await this.connection.stop();
                this.isConnected = false;
                console.debug('FriendHub: Connection stopped');
            } catch (error) {
                console.error('FriendHub: Error stopping connection:', error);
            }
        }
    }
}

// Initialize FriendHub when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.friendHub = new FriendHubClient();
    window.friendHub.initialize();
});

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    if (window.friendHub) {
        window.friendHub.stop();
    }
});
