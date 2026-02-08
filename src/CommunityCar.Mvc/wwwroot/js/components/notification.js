/**
 * Notification Component Logic
 */

// Define global functions first to avoid reference errors
window.updateUnreadCount = function () {
    const badge = document.getElementById('notificationBadge');
    if (!badge) return;

    const url = CultureHelper.addCultureToUrl('/Communications/Notifications/UnreadCount');
    fetch(url)
        .then(r => r.json())
        .then(data => {
            const count = data.count;
            if (count > 0) {
                badge.innerText = count > 99 ? '99+' : count;
                badge.classList.remove('d-none');
            } else {
                badge.classList.add('d-none');
            }
        })
        .catch(err => console.debug("Failed to update unread count:", err));
};

window.updateChatUnreadCount = function () {
    const badge = document.getElementById('chatUnreadBadge');
    if (!badge) return;

    const url = CultureHelper.addCultureToUrl('/Chats/GetUnreadCount');
    fetch(url)
        .then(r => r.json())
        .then(data => {
            const count = data.count;
            if (count > 0) {
                badge.innerText = count > 99 ? '99+' : count;
                badge.classList.remove('d-none');
            } else {
                badge.classList.add('d-none');
            }
        })
        .catch(err => console.debug("Failed to update chat unread count:", err));
};

window.updateFriendRequestCount = function () {
    const badge = document.getElementById('friendRequestBadge');
    if (!badge) return;

    const url = CultureHelper.addCultureToUrl('/Friends/GetPendingRequestCount');
    fetch(url)
        .then(r => r.json())
        .then(data => {
            const count = data.count;
            if (count > 0) {
                badge.innerText = count > 99 ? '99+' : count;
                badge.classList.remove('d-none');
            } else {
                badge.classList.add('d-none');
            }
        })
        .catch(err => console.error("Failed to update friend request count:", err));
};

window.loadLatestNotifications = function () {
    const list = document.getElementById('notificationList');
    if (!list) return;

    // Show spinner
    list.innerHTML = '<div class="p-4 text-center"><div class="spinner-border spinner-border-sm text-primary" role="status"></div></div>';

    const url = CultureHelper.addCultureToUrl('/Communications/Notifications/Latest');
    fetch(url)
        .then(r => {
            if (!r.ok) throw new Error('Network response was not ok');
            return r.json();
        })
        .then(data => {
            const notifications = data.notifications || data; // Handle both {success, notifications} and direct array
            if (!notifications || notifications.length === 0) {
                list.innerHTML = `
                    <div class="p-4 text-center text-muted">
                        <i class="fas fa-bell-slash fa-2x mb-2 opacity-25"></i>
                        <p class="mb-0 small">${window.I18n?.translations.notifications.noNewNotifications || 'No new notifications'}</p>
                    </div>`;
                return;
            }

            list.innerHTML = notifications.map(n => {
                const isFriendReq = n.title === 'New Friend Request' || n.Title === 'New Friend Request' || (n.link && n.link.includes('/Profiles/'));
                let requesterId = '';
                if (isFriendReq && n.link) {
                    const parts = n.link.split('/');
                    requesterId = parts[parts.length - 1];
                }

                const normalizedLink = (n.link || 'javascript:void(0)').replace('/Identity/Profiles/Index/', '/Identity/Profiles/');
                return `
                <div class="dropdown-item p-3 border-bottom notification-item ${n.isRead ? '' : 'unread'} position-relative" data-id="${n.id}">
                    <div class="d-flex align-items-start gap-2">
                        <div class="flex-grow-1">
                            <div class="d-flex justify-content-between align-items-center mb-1">
                                <span class="fw-bold small">${n.title}</span>
                                <span class="text-muted" style="font-size: 0.7rem;">${n.timeAgo || n.createdAt}</span>
                            </div>
                            <p class="mb-1 text-truncate-2 small text-wrap">${n.message}</p>
                            
                            ${isFriendReq && !n.isRead ? `
                            <div class="d-flex gap-2 mt-2 notification-actions">
                                <form action="/Friends/AcceptRequest" method="post" class="ajax-accept-form flex-grow-1" data-notification-id="${n.id}">
                                    <input type="hidden" name="__RequestVerificationToken" value="${document.querySelector('input[name=\'__RequestVerificationToken\']')?.value || ''}" />
                                    <input type="hidden" name="friendId" value="${requesterId}" />
                                    <button type="submit" class="btn btn-primary btn-xs py-1 rounded-pill w-100 text-xs text-white">${window.I18n?.translations.notifications.accept || 'Accept'}</button>
                                </form>
                                <form action="/Friends/RejectRequest" method="post" class="ajax-reject-form flex-grow-1" data-notification-id="${n.id}">
                                    <input type="hidden" name="__RequestVerificationToken" value="${document.querySelector('input[name=\'__RequestVerificationToken\']')?.value || ''}" />
                                    <input type="hidden" name="friendId" value="${requesterId}" />
                                    <button type="submit" class="btn btn-outline-secondary btn-xs py-1 rounded-pill w-100 text-xs">${window.I18n?.translations.notifications.decline || 'Decline'}</button>
                                </form>
                            </div>
                            ` : ''}
                        </div>
                        ${n.isRead ? '' : '<span class="unread-dot bg-primary rounded-circle" style="width: 8px; height: 8px; flex-shrink: 0; margin-top: 5px;"></span>'}
                    </div>
                    <a href="${normalizedLink}" 
                       onclick="markAsRead('${n.id}', event)"
                       class="stretched-link ${isFriendReq && !n.isRead ? 'd-none' : ''}"></a>
                </div>`;
            }).join('');
        })
        .catch(err => {
            console.error("Failed to load notifications:", err);
            list.innerHTML = `
                <div class="p-4 text-center text-muted">
                    <i class="fas fa-exclamation-circle fa-2x mb-2 text-danger opacity-50"></i>
                    <p class="mb-0 small">${window.I18n?.translations.notifications.failedToLoadNotifications || 'Failed to load notifications'}</p>
                    <button class="btn btn-link btn-sm text-primary p-0 mt-1" onclick="loadLatestNotifications()">${window.I18n?.translations.common.tryAgain || 'Try again'}</button>
                </div>`;
        });
};

window.markAsRead = function (id, event) {
    if (event) event.stopPropagation();
    const url = CultureHelper.addCultureToUrl(`/Communications/Notifications/MarkAsRead/${id}`);
    fetch(url, { method: 'POST' })
        .then(() => window.updateUnreadCount())
        .catch(err => console.debug("Failed to mark notification as read:", err));
};

window.markAllNotificationsAsRead = function () {
    const url = CultureHelper.addCultureToUrl('/Communications/Notifications/MarkAllAsRead');
    fetch(url, { method: 'POST' })
        .then(() => {
            window.updateUnreadCount();
            window.loadLatestNotifications();
        })
        .catch(err => console.debug("Failed to mark all as read:", err));
};

document.addEventListener('DOMContentLoaded', function () {
    const notificationDropdown = document.getElementById('notificationDropdown');
    
    // Only initialize SignalR if user is authenticated (notification dropdown exists)
    if (notificationDropdown) {
        // Initialize SignalR connection for notifications
        const notificationConnection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect()
            .build();

        notificationConnection.on("ReceiveNotification", function (notification) {
            window.updateUnreadCount();

            // If it's a friend request, also update the friend request badge
            if (notification.title === 'New Friend Request' || (notification.Title === 'New Friend Request')) {
                window.updateFriendRequestCount();
            }

            // Show a small toast for real-time alert
            if (window.Toast) {
                window.Toast.show(notification.message || notification.Message || notification.Title, 'info', notification.title || notification.Title);
            }

            if (notificationDropdown.classList.contains('show')) {
                window.loadLatestNotifications();
            }
        });

        notificationConnection.start().catch(err => console.error("NotificationHub connection failed:", err));

        // Initialize SignalR connection for chats (to update badge globally)
        const chatBadge = document.getElementById('chatUnreadBadge');
        if (chatBadge) {
            const chatConnection = new signalR.HubConnectionBuilder()
                .withUrl("/chatHub")
                .withAutomaticReconnect()
                .build();

            chatConnection.on("ReceiveMessage", function (senderId, message) {
                // Only update if we are NOT on the conversation page with this user
                // (The conversation page has its own logic)
                if (typeof receiverId === 'undefined' || receiverId !== senderId) {
                    window.updateChatUnreadCount();
                    if (window.Toast) {
                        window.Toast.show(window.I18n?.translations.chat.newMessage || "New message received", 'info', "Chat");
                    }
                }
            });

            chatConnection.start().catch(err => console.error("ChatHub connection failed:", err));
        }

        // Load unread counts on page load
        window.updateUnreadCount();
        window.updateChatUnreadCount();
        window.updateFriendRequestCount();

        // Load latest notifications when dropdown is opened
        notificationDropdown.addEventListener('show.bs.dropdown', function () {
            window.loadLatestNotifications();
        });
    }
});
