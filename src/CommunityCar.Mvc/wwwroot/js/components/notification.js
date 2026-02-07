/**
 * Notification Component Logic
 */

// Define global functions first to avoid reference errors
window.updateUnreadCount = function () {
    const badge = document.getElementById('notificationBadge');
    if (!badge) return;

    fetch('/Communications/Notifications/UnreadCount')
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
        .catch(err => console.error("Failed to update unread count:", err));
};

window.loadLatestNotifications = function () {
    const list = document.getElementById('notificationList');
    if (!list) return;

    // Show spinner
    list.innerHTML = '<div class="p-4 text-center"><div class="spinner-border spinner-border-sm text-primary" role="status"></div></div>';

    fetch('/Communications/Notifications/Latest')
        .then(r => {
            if (!r.ok) throw new Error('Network response was not ok');
            return r.json();
        })
        .then(data => {
            if (!data || data.length === 0) {
                list.innerHTML = `
                    <div class="p-4 text-center text-muted">
                        <i class="fas fa-bell-slash fa-2x mb-2 opacity-25"></i>
                        <p class="mb-0 small">No new notifications</p>
                    </div>`;
                return;
            }

            list.innerHTML = data.map(n => `
                <a href="${n.link || 'javascript:void(0)'}" 
                   onclick="markAsRead('${n.id}', event)"
                   class="dropdown-item p-3 border-bottom notification-item ${n.isRead ? '' : 'unread'}">
                    <div class="d-flex align-items-start gap-2">
                        <div class="flex-grow-1">
                            <div class="d-flex justify-content-between align-items-center mb-1">
                                <span class="fw-bold small">${n.title}</span>
                                <span class="text-muted" style="font-size: 0.7rem;">${n.createdAt}</span>
                            </div>
                            <p class="mb-0 text-truncate-2 small text-wrap">${n.message}</p>
                        </div>
                        ${n.isRead ? '' : '<span class="unread-dot bg-primary rounded-circle" style="width: 8px; height: 8px; flex-shrink: 0; margin-top: 5px;"></span>'}
                    </div>
                </a>
            `).join('');
        })
        .catch(err => {
            console.error("Failed to load notifications:", err);
            list.innerHTML = `
                <div class="p-4 text-center text-muted">
                    <i class="fas fa-exclamation-circle fa-2x mb-2 text-danger opacity-50"></i>
                    <p class="mb-0 small">Failed to load notifications</p>
                    <button class="btn btn-link btn-sm text-primary p-0 mt-1" onclick="loadLatestNotifications()">Try again</button>
                </div>`;
        });
};

window.markAsRead = function (id, event) {
    fetch(`/Communications/Notifications/MarkAsRead/${id}`, { method: 'POST' })
        .then(() => window.updateUnreadCount())
        .catch(err => console.error("Failed to mark notification as read:", err));
};

window.markAllNotificationsAsRead = function () {
    fetch('/Communications/Notifications/MarkAllAsRead', { method: 'POST' })
        .then(() => {
            window.updateUnreadCount();
            window.loadLatestNotifications();
        })
        .catch(err => console.error("Failed to mark all as read:", err));
};

document.addEventListener('DOMContentLoaded', function () {
    const notificationDropdown = document.getElementById('notificationDropdown');
    if (!notificationDropdown) return;

    // Initialize SignalR connection for notifications
    const notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .build();

    notificationConnection.on("ReceiveNotification", function (notification) {
        window.updateUnreadCount();
        if (notificationDropdown.classList.contains('show')) {
            window.loadLatestNotifications();
        }
        // Show a small toast for real-time alert
        if (window.Toast) {
            window.Toast.show(notification.Title, 'info');
        }
    });

    notificationConnection.start().catch(err => console.error("NotificationHub failed:", err));

    // Load unread count on page load
    window.updateUnreadCount();

    // Load latest notifications when dropdown is opened
    notificationDropdown.addEventListener('show.bs.dropdown', function () {
        window.loadLatestNotifications();
    });
});
