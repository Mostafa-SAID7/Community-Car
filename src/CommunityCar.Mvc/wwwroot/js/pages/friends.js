/**
 * Friends Page JavaScript
 * Handles AJAX operations for friend requests, accepts, rejects, blocks, etc.
 */

(function () {
    'use strict';

    // Helper function to get anti-forgery token
    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }

    // Helper function to show toast notification using global Toast component
    function showToast(message, type = 'success') {
        if (window.Toast) {
            // Map Bootstrap types to Toastr types
            const toastType = type === 'danger' ? 'error' : type;
            window.Toast.show(message, toastType);
        } else {
            // Fallback to console if Toast is not available
            console.log(`[${type.toUpperCase()}] ${message}`);
        }
    }

    // Helper function to remove card with animation
    function removeCard(element) {
        element.style.transition = 'opacity 0.3s, transform 0.3s';
        element.style.opacity = '0';
        element.style.transform = 'scale(0.9)';
        setTimeout(() => {
            element.remove();

            // Check if there are no more cards and show empty state
            const container = document.querySelector('.row.g-4');
            if (container && container.children.length === 0) {
                location.reload(); // Reload to show empty state
            }
        }, 300);
    }

    // Handle Accept Friend Request
    document.addEventListener('submit', function (e) {
        if (e.target.classList.contains('ajax-accept-form')) {
            e.preventDefault();

            const form = e.target;
            const button = form.querySelector('button[type="submit"]');
            const friendId = form.querySelector('input[name="friendId"]').value;
            const card = form.closest('[data-request-id]');

            // Disable button and show loading
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i> Accepting...';

            const url = CultureHelper.addCultureToUrl('/Friends/AcceptRequestJson');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: new URLSearchParams({ friendId: friendId })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        showToast(data.message || 'Friend request accepted!', 'success');

                        // Remove card with animation
                        if (card) {
                            removeCard(card);
                        }

                        // Update badge count
                        const badge = document.querySelector('.friends-nav .badge');
                        if (badge) {
                            const count = parseInt(badge.textContent) - 1;
                            if (count > 0) {
                                badge.textContent = count;
                            } else {
                                badge.remove();
                            }
                        }
                    } else {
                        showToast(data.message || 'Failed to accept friend request', 'danger');
                        button.disabled = false;
                        button.innerHTML = '<i class="fas fa-check me-2"></i> Accept';
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    showToast('An error occurred. Please try again.', 'danger');
                    button.disabled = false;
                    button.innerHTML = '<i class="fas fa-check me-2"></i> Accept';
                });
        }
    });

    // Handle Reject Friend Request
    document.addEventListener('submit', function (e) {
        if (e.target.classList.contains('ajax-reject-form')) {
            e.preventDefault();

            const form = e.target;
            const button = form.querySelector('button[type="submit"]');
            const friendId = form.querySelector('input[name="friendId"]').value;
            const card = form.closest('[data-request-id]');

            // Disable button and show loading
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i> Rejecting...';

            const url = CultureHelper.addCultureToUrl('/Friends/RejectRequestJson');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: new URLSearchParams({ friendId: friendId })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        showToast(data.message || 'Friend request rejected', 'success');

                        // Remove card with animation
                        if (card) {
                            removeCard(card);
                        }

                        // Update badge count
                        const badge = document.querySelector('.friends-nav .badge');
                        if (badge) {
                            const count = parseInt(badge.textContent) - 1;
                            if (count > 0) {
                                badge.textContent = count;
                            } else {
                                badge.remove();
                            }
                        }
                    } else {
                        showToast(data.message || 'Failed to reject friend request', 'danger');
                        button.disabled = false;
                        button.innerHTML = '<i class="fas fa-times me-2"></i> Reject';
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    showToast('An error occurred. Please try again.', 'danger');
                    button.disabled = false;
                    button.innerHTML = '<i class="fas fa-times me-2"></i> Reject';
                });
        }
    });

    // Handle Send Friend Request
    document.addEventListener('submit', function (e) {
        if (e.target.classList.contains('ajax-add-friend-form')) {
            e.preventDefault();

            const form = e.target;
            const button = form.querySelector('button[type="submit"]');
            const friendId = form.querySelector('input[name="friendId"]').value;

            // Disable button and show loading
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i> Sending...';

            const url = CultureHelper.addCultureToUrl('/Friends/SendRequestJson');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: new URLSearchParams({ friendId: friendId })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        showToast(data.message || 'Friend request sent!', 'success');

                        // Update button to show "Sent" state
                        button.className = 'btn btn-secondary w-100 rounded-pill';
                        button.innerHTML = '<i class="fas fa-clock me-2"></i> Sent';
                        button.disabled = true;
                    } else {
                        showToast(data.message || 'Failed to send friend request', 'danger');
                        button.disabled = false;
                        button.innerHTML = '<i class="fas fa-user-plus me-2"></i> Add';
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    showToast('An error occurred. Please try again.', 'danger');
                    button.disabled = false;
                    button.innerHTML = '<i class="fas fa-user-plus me-2"></i> Add';
                });
        }
    });

    // Handle Remove Friend
    document.addEventListener('submit', function (e) {
        if (e.target.classList.contains('ajax-remove-form')) {
            e.preventDefault();

            if (!confirm('Are you sure you want to remove this friend?')) {
                return;
            }

            const form = e.target;
            const button = form.querySelector('button[type="submit"]');
            const friendId = form.querySelector('input[name="friendId"]').value;
            const card = form.closest('.col-md-6, .col-lg-4');

            // Disable button and show loading
            button.disabled = true;
            const originalHtml = button.innerHTML;
            button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i> Removing...';

            const url = CultureHelper.addCultureToUrl('/Friends/RemoveFriendJson');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: new URLSearchParams({ friendId: friendId })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        showToast(data.message || 'Friend removed', 'success');

                        // Remove card with animation
                        if (card) {
                            removeCard(card);
                        }
                    } else {
                        showToast(data.message || 'Failed to remove friend', 'danger');
                        button.disabled = false;
                        button.innerHTML = originalHtml;
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    showToast('An error occurred. Please try again.', 'danger');
                    button.disabled = false;
                    button.innerHTML = originalHtml;
                });
        }
    });

    // Handle Block User
    document.addEventListener('submit', function (e) {
        if (e.target.classList.contains('ajax-block-form')) {
            e.preventDefault();

            if (!confirm('Are you sure you want to block this user? They will not be able to send you friend requests or interact with your content.')) {
                return;
            }

            const form = e.target;
            const button = form.querySelector('button[type="submit"]');
            const friendId = form.querySelector('input[name="friendId"]').value;
            const card = form.closest('.col-md-6, .col-lg-4');

            // Disable button and show loading
            button.disabled = true;
            const originalHtml = button.innerHTML;
            button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i> Blocking...';

            const url = CultureHelper.addCultureToUrl('/Friends/BlockUserJson');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: new URLSearchParams({ friendId: friendId })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        showToast(data.message || 'User blocked', 'success');

                        // Remove card with animation
                        if (card) {
                            removeCard(card);
                        }
                    } else {
                        showToast(data.message || 'Failed to block user', 'danger');
                        button.disabled = false;
                        button.innerHTML = originalHtml;
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    showToast('An error occurred. Please try again.', 'danger');
                    button.disabled = false;
                    button.innerHTML = originalHtml;
                });
        }
    });

    // Handle Unblock User
    document.addEventListener('submit', function (e) {
        if (e.target.classList.contains('ajax-unblock-form')) {
            e.preventDefault();

            const form = e.target;
            const button = form.querySelector('button[type="submit"]');
            const friendId = form.querySelector('input[name="friendId"]').value;
            const card = form.closest('.col-md-6, .col-lg-4');

            // Disable button and show loading
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i> Unblocking...';

            const url = CultureHelper.addCultureToUrl('/Friends/UnblockUserJson');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: new URLSearchParams({ friendId: friendId })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        showToast(data.message || 'User unblocked', 'success');

                        // Remove card with animation
                        if (card) {
                            removeCard(card);
                        }
                    } else {
                        showToast(data.message || 'Failed to unblock user', 'danger');
                        button.disabled = false;
                        button.innerHTML = '<i class="fas fa-unlock me-2"></i> Unblock User';
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    showToast('An error occurred. Please try again.', 'danger');
                    button.disabled = false;
                    button.innerHTML = '<i class="fas fa-unlock me-2"></i> Unblock User';
                });
        }
    });

    // Note: SignalR event listeners are handled by friends-hub.js
    // The hub dispatches custom events that trigger notifications automatically
    // No need to duplicate event listeners here

})();
