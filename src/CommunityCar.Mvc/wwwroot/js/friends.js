// Friends feature JavaScript
(function () {
    'use strict';

    // Live search functionality
    const searchInput = document.getElementById('friendSearch');
    if (searchInput) {
        let searchTimeout;
        searchInput.addEventListener('input', function (e) {
            clearTimeout(searchTimeout);
            const query = e.target.value.trim();

            if (query.length < 2) {
                document.getElementById('searchResults').innerHTML = '';
                return;
            }

            searchTimeout = setTimeout(function () {
                fetch(`/Friends/SearchApi?query=${encodeURIComponent(query)}`)
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            displaySearchResults(data.users);
                        }
                    })
                    .catch(error => console.error('Search error:', error));
            }, 300);
        });
    }

    function displaySearchResults(users) {
        const resultsContainer = document.getElementById('searchResults');
        if (!resultsContainer) return;

        if (users.length === 0) {
            resultsContainer.innerHTML = '<div class="alert alert-info">No users found</div>';
            return;
        }

        let html = '<div class="list-group">';
        users.forEach(user => {
            html += `
                <div class="list-group-item list-group-item-action">
                    <div class="d-flex align-items-center">
                        ${user.profilePictureUrl 
                            ? `<img src="${user.profilePictureUrl}" alt="${user.name}" class="rounded-circle me-3" style="width: 40px; height: 40px; object-fit: cover;">`
                            : `<div class="rounded-circle bg-secondary text-white d-flex align-items-center justify-content-center me-3" style="width: 40px; height: 40px;">
                                <span>${user.name.charAt(0)}</span>
                               </div>`
                        }
                        <div class="flex-grow-1">
                            <h6 class="mb-0">${user.name}</h6>
                            <small class="text-muted">@${user.username}</small>
                        </div>
                        <a href="/Identity/Profiles/${user.slug}" class="btn btn-sm btn-outline-primary">View</a>
                    </div>
                </div>
            `;
        });
        html += '</div>';
        resultsContainer.innerHTML = html;
    }

    // Friendship status checker
    function checkFriendshipStatus(friendId) {
        return fetch(`/Friends/Status/${friendId}`)
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    return data.status;
                }
                return null;
            })
            .catch(error => {
                console.error('Status check error:', error);
                return null;
            });
    }

    // Confirm actions
    document.querySelectorAll('[data-confirm]').forEach(element => {
        element.addEventListener('click', function (e) {
            const message = this.getAttribute('data-confirm');
            if (!confirm(message)) {
                e.preventDefault();
                return false;
            }
        });
    });

    // Auto-dismiss alerts after 5 seconds
    setTimeout(function () {
        const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
        alerts.forEach(alert => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);

    // Friend request counter update
    function updateRequestCount() {
        fetch('/Friends/Requests')
            .then(response => response.text())
            .then(html => {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const count = doc.querySelectorAll('.friend-request-item').length;
                
                const badge = document.querySelector('.request-count-badge');
                if (badge) {
                    if (count > 0) {
                        badge.textContent = count;
                        badge.style.display = 'inline';
                    } else {
                        badge.style.display = 'none';
                    }
                }
            })
            .catch(error => console.error('Request count update error:', error));
    }

    // Update request count every 30 seconds
    if (document.querySelector('.request-count-badge')) {
        setInterval(updateRequestCount, 30000);
    }

    // Toast notifications
    window.showFriendNotification = function (message, type = 'success') {
        const toastContainer = document.getElementById('toastContainer');
        if (!toastContainer) {
            const container = document.createElement('div');
            container.id = 'toastContainer';
            container.className = 'toast-container position-fixed top-0 end-0 p-3';
            document.body.appendChild(container);
        }

        const toastId = 'toast-' + Date.now();
        const toastHtml = `
            <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="toast-header bg-${type} text-white">
                    <strong class="me-auto">
                        <i class="bi bi-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i>
                        ${type === 'success' ? 'Success' : 'Error'}
                    </strong>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `;

        document.getElementById('toastContainer').insertAdjacentHTML('beforeend', toastHtml);
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement);
        toast.show();

        toastElement.addEventListener('hidden.bs.toast', function () {
            toastElement.remove();
        });
    };

})();
