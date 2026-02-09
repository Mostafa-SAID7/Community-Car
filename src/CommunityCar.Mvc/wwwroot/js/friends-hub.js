/**
 * FriendHub SignalR Client
 * Handles real-time friend interactions: requests, accepts, rejects, blocks, and status updates
 */

class FriendHubClient {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
        this.reconnectDelay = 3000;
    }

    /**
     * Initialize and start the SignalR connection
     */
    async init() {
        try {
            // Build the connection
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/friendHub")
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: retryContext => {
                        // Exponential backoff: 2s, 5s, 10s, 30s, then stop
                        if (retryContext.previousRetryCount === 0) return 2000;
                        if (retryContext.previousRetryCount === 1) return 5000;
                        if (retryContext.previousRetryCount === 2) return 10000;
                        if (retryContext.previousRetryCount === 3) return 30000;
                        return null; // Stop reconnecting
                    }
                })
                .configureLogging(signalR.LogLevel.Warning)
                .build();

            // Register event handlers
            this.registerEventHandlers();

            // Start the connection
            await this.start();

        } catch (error) {
            console.error('Failed to initialize FriendHub:', error);
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
            console.log('FriendHub connected successfully');
            this.onConnected();
        } catch (error) {
            console.debug('Failed to start FriendHub connection:', error.message);
            this.isConnected = false;

            // Retry connection
            if (this.reconnectAttempts < this.maxReconnectAttempts) {
                this.reconnectAttempts++;
                console.debug(`Retrying connection (${this.reconnectAttempts}/${this.maxReconnectAttempts})...`);
                setTimeout(() => this.start(), this.reconnectDelay);
            }
        }
    }

    /**
     * Register all SignalR event handlers
     */
    registerEventHandlers() {
        // Connection lifecycle events
        this.connection.onreconnecting(() => {
            console.log('FriendHub reconnecting...');
            this.isConnected = false;
            this.onReconnecting();
        });

        this.connection.onreconnected(() => {
            console.log('FriendHub reconnected');
            this.isConnected = true;
            this.onReconnected();
        });

        this.connection.onclose((error) => {
            console.debug('FriendHub connection closed:', error?.message || 'Connection closed normally');
            this.isConnected = false;
            this.onDisconnected();
        });

        // Friend request events
        this.connection.on('ReceiveFriendRequest', (data) => {
            this.onReceiveFriendRequest(data);
        });

        this.connection.on('FriendRequestAccepted', (data) => {
            this.onFriendRequestAccepted(data);
        });

        this.connection.on('FriendRequestRejected', (data) => {
            this.onFriendRequestRejected(data);
        });

        // Block/Unblock events
        this.connection.on('UserBlocked', (data) => {
            this.onUserBlocked(data);
        });

        this.connection.on('UserUnblocked', (data) => {
            this.onUserUnblocked(data);
        });

        // Online/Offline status events to silence warnings
        this.connection.on('UserOnline', (userId) => {
            console.debug('FriendHub: User online', userId);
        });

        this.connection.on('UserOffline', (userId) => {
            console.debug('FriendHub: User offline', userId);
        });

        this.connection.on('NewFriendSuggestions', (data) => {
            this.onNewFriendSuggestions(data);
        });

        this.connection.on('FriendProfileUpdated', (data) => {
            this.onFriendProfileUpdated(data);
        });

        this.connection.on('FriendStatusChanged', (data) => {
            this.onFriendStatusChanged(data);
        });

        this.connection.on('SystemAnnouncement', (data) => {
            this.onSystemAnnouncement(data);
        });

        // Friendship events
        this.connection.on('FriendshipRemoved', (data) => {
            this.onFriendshipRemoved(data);
        });

        // Status events
        this.connection.on('UserOnline', (userId) => {
            this.onUserOnline(userId);
        });

        this.connection.on('UserOffline', (userId) => {
            this.onUserOffline(userId);
        });

        this.connection.on('FriendStatusChanged', (data) => {
            this.onFriendStatusChanged(data);
        });

        // Profile update events
        this.connection.on('FriendProfileUpdated', (data) => {
            this.onFriendProfileUpdated(data);
        });

        // Suggestion events
        this.connection.on('NewFriendSuggestions', (data) => {
            this.onNewFriendSuggestions(data);
        });

        // System events
        this.connection.on('SystemAnnouncement', (data) => {
            this.onSystemAnnouncement(data);
        });
    }

    /**
     * Event handler: Connection established
     */
    onConnected() {
        // Update UI to show online status
        this.updateConnectionStatus('connected');
    }

    /**
     * Event handler: Connection lost, attempting to reconnect
     */
    onReconnecting() {
        this.updateConnectionStatus('reconnecting');
    }

    /**
     * Event handler: Connection re-established
     */
    onReconnected() {
        this.updateConnectionStatus('connected');
        console.debug('FriendHub reconnected - refresh suggested if data is stale');
        // Removed automatic refresh to prevent reload loops
        // this.refreshFriendList();
    }

    /**
     * Event handler: Connection closed
     */
    onDisconnected() {
        this.updateConnectionStatus('disconnected');
    }

    /**
     * Event handler: Received a friend request
     */
    onReceiveFriendRequest(data) {
        console.log('Received friend request:', data);

        // Show notification
        this.showNotification(
            'New Friend Request',
            `${data.SenderName} sent you a friend request`,
            'info',
            data.SenderProfilePicture
        );

        // Update friend request count badge
        this.updateFriendRequestCount(1);

        // Play notification sound
        this.playNotificationSound();

        // Trigger custom event for other components
        document.dispatchEvent(new CustomEvent('friendRequestReceived', { detail: data }));
    }

    /**
     * Event handler: Friend request was accepted
     */
    onFriendRequestAccepted(data) {
        console.log('Friend request accepted:', data);

        // Show notification
        this.showNotification(
            'Friend Request Accepted',
            `${data.FriendName} accepted your friend request`,
            'success',
            data.FriendProfilePicture
        );

        // Refresh friend list UI
        this.refreshFriendList();

        // Trigger custom event
        document.dispatchEvent(new CustomEvent('friendRequestAccepted', { detail: data }));
    }

    /**
     * Event handler: Friend request was rejected
     */
    onFriendRequestRejected(data) {
        console.log('Friend request rejected:', data);

        // Show notification (optional, might not want to notify user of rejection)
        // this.showNotification('Friend Request', `${data.UserName} declined your friend request`, 'warning');

        // Trigger custom event
        document.dispatchEvent(new CustomEvent('friendRequestRejected', { detail: data }));
    }

    /**
     * Event handler: User was blocked
     */
    onUserBlocked(data) {
        console.log('You were blocked by user:', data.BlockedBy);

        // Remove from friend list if displayed
        this.removeFriendFromUI(data.BlockedBy);

        // Trigger custom event
        document.dispatchEvent(new CustomEvent('userBlocked', { detail: data }));
    }

    /**
     * Event handler: User was unblocked
     */
    onUserUnblocked(data) {
        console.debug('User unblocked:', data);
    }

    /**
     * Event handler: Friendship was removed
     */
    onFriendshipRemoved(data) {
        console.log('Friendship removed:', data);

        // Show notification
        this.showNotification(
            'Friendship Ended',
            `${data.RemovedByName} removed you as a friend`,
            'warning'
        );

        // Remove from friend list
        this.removeFriendFromUI(data.RemovedBy);

        // Trigger custom event
        document.dispatchEvent(new CustomEvent('friendshipRemoved', { detail: data }));
    }

    /**
     * Event handler: User came online
     */
    onUserOnline(userId) {
        console.log('User came online:', userId);
        this.updateUserOnlineStatus(userId, true);

        // Trigger custom event
        document.dispatchEvent(new CustomEvent('userOnline', { detail: { userId } }));
    }

    /**
     * Event handler: User went offline
     */
    onUserOffline(userId) {
        console.log('User went offline:', userId);
        this.updateUserOnlineStatus(userId, false);

        // Trigger custom event
        document.dispatchEvent(new CustomEvent('userOffline', { detail: { userId } }));
    }

    /**
     * Event handler: Friend status changed (online/offline/busy)
     */
    onFriendStatusChanged(data) {
        console.log('Friend status changed:', data);
        this.updateFriendStatus(data.FriendId, data.Status);

        // Trigger custom event
        document.dispatchEvent(new CustomEvent('friendStatusChanged', { detail: data }));
    }

    /**
     * Event handler: Friend updated their profile
     */
    onFriendProfileUpdated(data) {
        console.log('Friend profile updated:', data);

        // Update friend profile in UI
        this.updateFriendProfile(data.FriendId, data.FriendName, data.FriendProfilePicture);

        // Trigger custom event
        document.dispatchEvent(new CustomEvent('friendProfileUpdated', { detail: data }));
    }

    /**
     * Event handler: New friend suggestions available
     */
    onNewFriendSuggestions(data) {
        console.log('New friend suggestions:', data);

        // Show notification
        if (data.Count > 0) {
            this.showNotification(
                'Friend Suggestions',
                `${data.Count} new friend suggestions available`,
                'info'
            );
        }

        // Trigger custom event
        document.dispatchEvent(new CustomEvent('newFriendSuggestions', { detail: data }));
    }

    /**
     * Event handler: System announcement
     */
    onSystemAnnouncement(data) {
        console.log('System announcement:', data);

        // Show notification
        this.showNotification(
            'System Announcement',
            data.Message,
            data.Type || 'info'
        );

        // Trigger custom event
        document.dispatchEvent(new CustomEvent('systemAnnouncement', { detail: data }));
    }

    /**
     * Update connection status indicator in UI
     */
    updateConnectionStatus(status) {
        const statusElement = document.getElementById('friend-hub-status');
        if (statusElement) {
            statusElement.className = `connection-status ${status}`;
            statusElement.title = `Friend Hub: ${status}`;
        }
    }

    /**
     * Update friend request count badge
     */
    updateFriendRequestCount(increment = 0) {
        // If global update function exists, use it to fetch exact count from server
        if (typeof window.updateFriendRequestCount === 'function') {
            window.updateFriendRequestCount();
            return;
        }

        const badge = document.getElementById('friendRequestBadge');
        if (badge) {
            const currentCount = parseInt(badge.textContent) || 0;
            const newCount = Math.max(0, currentCount + increment);
            badge.textContent = newCount > 99 ? '99+' : newCount;
            if (newCount > 0) {
                badge.classList.remove('d-none');
            } else {
                badge.classList.add('d-none');
            }
        }
    }

    /**
     * Update user online status indicator
     */
    updateUserOnlineStatus(userId, isOnline) {
        const statusIndicators = document.querySelectorAll(`[data-user-id="${userId}"] .online-status`);
        statusIndicators.forEach(indicator => {
            if (isOnline) {
                indicator.classList.add('online');
                indicator.classList.remove('offline');
            } else {
                indicator.classList.add('offline');
                indicator.classList.remove('online');
            }
        });
    }

    /**
     * Update friend status (online/offline/busy/away)
     */
    updateFriendStatus(friendId, status) {
        const statusElements = document.querySelectorAll(`[data-friend-id="${friendId}"] .friend-status`);
        statusElements.forEach(element => {
            element.className = `friend-status ${status.toLowerCase()}`;
            element.textContent = status;
        });
    }

    /**
     * Update friend profile information in UI
     */
    updateFriendProfile(friendId, name, profilePicture) {
        // Update name
        const nameElements = document.querySelectorAll(`[data-friend-id="${friendId}"] .friend-name`);
        nameElements.forEach(element => {
            element.textContent = name;
        });

        // Update profile picture
        if (profilePicture) {
            const pictureElements = document.querySelectorAll(`[data-friend-id="${friendId}"] .friend-picture`);
            pictureElements.forEach(element => {
                element.src = profilePicture;
            });
        }
    }

    /**
     * Remove friend from UI
     */
    removeFriendFromUI(friendId) {
        const friendElements = document.querySelectorAll(`[data-friend-id="${friendId}"]`);
        friendElements.forEach(element => {
            element.remove();
        });
    }

    /**
     * Refresh friend list
     */
    refreshFriendList() {
        // Trigger page refresh or AJAX reload
        if (window.location.pathname.includes('/Friends')) {
            window.location.reload();
        }
    }

    /**
     * Show notification to user
     */
    showNotification(title, message, type = 'info', imageUrl = null) {
        // Use browser notification if permitted
        if ('Notification' in window && Notification.permission === 'granted') {
            const options = {
                body: message,
                icon: imageUrl || '/images/logo.png',
                badge: '/images/badge.png',
                tag: 'friend-notification',
                requireInteraction: false
            };
            new Notification(title, options);
        }

        // Also show in-app notification
        this.showInAppNotification(title, message, type);
    }

    /**
     * Show in-app notification using global Toast system
     */
    showInAppNotification(title, message, type) {
        // Use global Toast if available
        if (window.Toast) {
            const toastType = type === 'danger' ? 'error' : type;
            const fullMessage = title ? `${title}: ${message}` : message;
            window.Toast.show(fullMessage, toastType);
        } else {
            console.log(`[${type.toUpperCase()}] ${title}: ${message}`);
        }
    }

    /**
     * Play notification sound
     */
    playNotificationSound() {
        try {
            const audio = new Audio('/sounds/notification.mp3');
            audio.volume = 0.5;
            audio.play().catch(err => console.debug('Could not play notification sound:', err));
        } catch (error) {
            console.debug('Notification sound not available');
        }
    }

    /**
     * Request browser notification permission
     */
    async requestNotificationPermission() {
        if ('Notification' in window && Notification.permission === 'default') {
            const permission = await Notification.requestPermission();
            console.log('Notification permission:', permission);
            return permission === 'granted';
        }
        return Notification.permission === 'granted';
    }

    /**
     * Stop the connection
     */
    async stop() {
        if (this.connection) {
            await this.connection.stop();
            this.isConnected = false;
            console.log('FriendHub connection stopped');
        }
    }
}

// Initialize FriendHub when DOM is ready
document.addEventListener('DOMContentLoaded', async () => {
    // Only initialize if user is authenticated
    const isAuthenticated = document.body.dataset.authenticated === 'true';

    if (isAuthenticated) {
        window.friendHub = new FriendHubClient();
        await window.friendHub.init();

        // Request notification permission
        await window.friendHub.requestNotificationPermission();
    }
});
