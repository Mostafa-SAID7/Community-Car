document.addEventListener('DOMContentLoaded', function () {
    const notificationBadge = document.getElementById('notificationBadge');
    const notificationList = document.getElementById('notificationList');
    const notificationDropdown = document.getElementById('notificationDropdown');

    // Initialize SignalR connection for notifications
    const notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .build();

    notificationConnection.on("ReceiveNotification", function (notification) {
        updateUnreadCount();
        if (notificationDropdown.classList.contains('show')) {
            loadLatestNotifications();
        }
        // Show a small toast for real-time alert
        if (window.Toast) {
            window.Toast.show(notification.Title, 'info');
        }
    });

    notificationConnection.start().catch(err => console.error("NotificationHub failed:", err));

    // Load unread count on page load
    updateUnreadCount();

    // Load latest notifications when dropdown is opened
    notificationDropdown.addEventListener('show.bs.dropdown', function () {
        loadLatestNotifications();
    });

    window.updateUnreadCount = function () {
        fetch('/Communications/Notifications/UnreadCount')
            .then(r => r.json())
            .then(data => {
                const count = data.count;
                if (count > 0) {
                    notificationBadge.innerText = count > 99 ? '99+' : count;
                    notificationBadge.classList.remove('d-none');
                } else {
                    notificationBadge.classList.add('d-none');
                }
            });
    };

    window.loadLatestNotifications = function () {
        notificationList.innerHTML = '<div class="p-4 text-center"><div class="spinner-border spinner-border-sm text-primary" role="status"></div></div>';

        fetch('/Communications/Notifications/Latest')
            .then(r => r.json())
            .then(data => {
                if (data.length === 0) {
                    notificationList.innerHTML = `
                        <div class="p-4 text-center text-muted">
                            <i class="fas fa-bell-slash fa-2x mb-2 opacity-25"></i>
                            <p class="mb-0 small">No new notifications</p>
                        </div>`;
                    return;
                }

                notificationList.innerHTML = data.map(n => `
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
            });
    };

    window.markAsRead = function (id, event) {
        // If it's a link, we don't want to prevent default unless we need to do something else
        fetch(`/Communications/Notifications/MarkAsRead/${id}`, { method: 'POST' })
            .then(() => updateUnreadCount());
    };

    window.markAllNotificationsAsRead = function () {
        fetch('/Communications/Notifications/MarkAllAsRead', { method: 'POST' })
            .then(() => {
                updateUnreadCount();
                loadLatestNotifications();
            });
    };
});
