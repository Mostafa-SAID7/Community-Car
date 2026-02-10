/**
 * Friends Page JavaScript
 * Handles AJAX operations for friend requests, accepts, rejects, blocks, etc.
 * Also handles live search functionality
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

    // ===== LIVE SEARCH FUNCTIONALITY =====
    let searchTimeout;
    const searchInput = document.getElementById('friendSearchInput');
    const searchSpinner = document.getElementById('searchSpinner');
    const liveSearchResults = document.getElementById('liveSearchResults');
    const originalResults = document.getElementById('originalResults');
    const searchResultsGrid = document.getElementById('searchResultsGrid');

    if (searchInput) {
        searchInput.addEventListener('input', function(e) {
            const query = e.target.value.trim();

            // Clear previous timeout
            clearTimeout(searchTimeout);

            // If query is empty, show original results
            if (query.length === 0) {
                if (liveSearchResults) liveSearchResults.style.display = 'none';
                if (originalResults) originalResults.style.display = 'block';
                if (searchSpinner) searchSpinner.style.display = 'none';
                return;
            }

            // Show spinner
            if (searchSpinner) searchSpinner.style.display = 'block';

            // Debounce search - wait 500ms after user stops typing
            searchTimeout = setTimeout(() => {
                performLiveSearch(query);
            }, 500);
        });

        // Prevent form submission on Enter key
        const searchForm = document.getElementById('friendSearchForm');
        if (searchForm) {
            searchForm.addEventListener('submit', function(e) {
                e.preventDefault();
                const query = searchInput.value.trim();
                if (query.length > 0) {
                    performLiveSearch(query);
                }
            });
        }
    }

    function performLiveSearch(query) {
        const url = CultureHelper.addCultureToUrl(`/Friends/SearchApi?query=${encodeURIComponent(query)}`);

        fetch(url)
            .then(response => response.json())
            .then(data => {
                if (searchSpinner) searchSpinner.style.display = 'none';

                if (data.success) {
                    displaySearchResults(data.users, query);
                } else {
                    showToast(data.message || 'Failed to search users', 'danger');
                }
            })
            .catch(error => {
                console.error('Search error:', error);
                if (searchSpinner) searchSpinner.style.display = 'none';
                showToast('An error occurred while searching', 'danger');
            });
    }

    function displaySearchResults(users, query) {
        if (!searchResultsGrid || !liveSearchResults || !originalResults) return;

        // Hide original results, show live results
        originalResults.style.display = 'none';
        liveSearchResults.style.display = 'block';

        // Clear previous results
        searchResultsGrid.innerHTML = '';

        if (users.length === 0) {
            searchResultsGrid.innerHTML = `
                <div class="col-12">
                    <div class="friends-empty">
                        <div class="friends-empty-icon">
                            <i class="fas fa-search"></i>
                        </div>
                        <h3 class="fw-bold mb-2">No Results Found</h3>
                        <p class="text-muted mb-4">Try searching with different keywords.</p>
                    </div>
                </div>
            `;
            return;
        }

        // Display results
        users.forEach(user => {
            const userCard = createUserCard(user);
            searchResultsGrid.appendChild(userCard);
        });
    }

    function createUserCard(user) {
        const col = document.createElement('div');
        col.className = 'col-md-6 col-lg-4';

        const avatarHtml = user.profilePictureUrl 
            ? `<img src="${user.profilePictureUrl}" alt="${user.name}" class="friend-avatar">`
            : `<div class="friend-avatar-placeholder">${user.name.substring(0, 1)}</div>`;

        const profileUrl = CultureHelper.addCultureToUrl(`/Identity/Profiles/Index/${user.id}`);

        col.innerHTML = `
            <div class="friend-card">
                <div class="friend-avatar-container">
                    ${avatarHtml}
                    <div class="friend-info">
                        <a href="${profileUrl}" class="friend-name">
                            ${user.name}
                        </a>
                        <div class="friend-meta">
                            ${user.username}
                        </div>
                    </div>
                </div>
                <div class="friend-actions mt-auto d-flex gap-2">
                    <a href="${profileUrl}" class="btn btn-outline-secondary rounded-pill flex-grow-1">
                        <i class="fas fa-user me-2"></i> Profile
                    </a>
                    <form class="flex-grow-1 ajax-add-friend-form">
                        <input type="hidden" name="__RequestVerificationToken" value="${getAntiForgeryToken()}" />
                        <input type="hidden" name="friendId" value="${user.id}" />
                        <button type="submit" class="btn btn-primary w-100 rounded-pill">
                            <i class="fas fa-user-plus me-2"></i> Add
                        </button>
                    </form>
                </div>
            </div>
        `;

        return col;
    }

    // ===== FRIEND REQUEST HANDLERS =====

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
            button.innerHTML = `<i class="fas fa-spinner fa-spin me-2"></i> ${window.I18n?.translations.friends.accepting || 'Accepting...'}`;

            const url = CultureHelper.addCultureToUrl('/Friends/AcceptRequestJson');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: new URLSearchParams({ 
                    friendId: friendId,
                    __RequestVerificationToken: getAntiForgeryToken()
                })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        showToast(data.message || window.I18n?.translations.notifications.accept || 'Friend request accepted!', 'success');

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
                        showToast(data.message || window.I18n?.translations.friends.failedToAccept || 'Failed to accept friend request', 'danger');
                        button.disabled = false;
                        button.innerHTML = `<i class="fas fa-check me-2"></i> ${window.I18n?.translations.notifications.accept || 'Accept'}`;
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    showToast(window.I18n?.translations.common.error || 'An error occurred. Please try again.', 'danger');
                    button.disabled = false;
                    button.innerHTML = `<i class="fas fa-check me-2"></i> ${window.I18n?.translations.notifications.accept || 'Accept'}`;
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
            button.innerHTML = `<i class="fas fa-spinner fa-spin me-2"></i> ${window.I18n?.translations.friends.rejecting || 'Rejecting...'}`;

            const url = CultureHelper.addCultureToUrl('/Friends/RejectRequestJson');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: new URLSearchParams({ 
                    friendId: friendId,
                    __RequestVerificationToken: getAntiForgeryToken()
                })
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
                        showToast(data.message || window.I18n?.translations.friends.failedToReject || 'Failed to reject friend request', 'danger');
                        button.disabled = false;
                        button.innerHTML = `<i class="fas fa-times me-2"></i> ${window.I18n?.translations.notifications.decline || 'Reject'}`;
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    showToast(window.I18n?.translations.common.error || 'An error occurred. Please try again.', 'danger');
                    button.disabled = false;
                    button.innerHTML = `<i class="fas fa-times me-2"></i> ${window.I18n?.translations.notifications.decline || 'Reject'}`;
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
            button.innerHTML = `<i class="fas fa-spinner fa-spin me-2"></i> ${window.I18n?.translations.friends.sending || 'Sending...'}`;

            const url = CultureHelper.addCultureToUrl('/Friends/SendRequestJson');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: new URLSearchParams({ 
                    friendId: friendId,
                    __RequestVerificationToken: getAntiForgeryToken()
                })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        showToast(data.message || window.I18n?.translations.friends.sent || 'Friend request sent!', 'success');

                        // Update button to show "Sent" state
                        button.className = 'btn btn-secondary w-100 rounded-pill';
                        button.innerHTML = `<i class="fas fa-clock me-2"></i> ${window.I18n?.translations.friends.sent || 'Sent'}`;
                        button.disabled = true;
                    } else {
                        showToast(data.message || window.I18n?.translations.friends.failedToSend || 'Failed to send friend request', 'danger');
                        button.disabled = false;
                        button.innerHTML = `<i class="fas fa-user-plus me-2"></i> ${window.I18n?.translations.friends.addFriend || 'Add'}`;
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    showToast(window.I18n?.translations.common.error || 'An error occurred. Please try again.', 'danger');
                    button.disabled = false;
                    button.innerHTML = `<i class="fas fa-user-plus me-2"></i> ${window.I18n?.translations.friends.addFriend || 'Add'}`;
                });
        }
    });

    // Handle Remove Friend
    document.addEventListener('submit', function (e) {
        if (e.target.classList.contains('ajax-remove-form')) {
            e.preventDefault();

            if (!confirm(window.I18n?.translations.friends.confirmRemove || 'Are you sure you want to remove this friend?')) {
                return;
            }

            const form = e.target;
            const button = form.querySelector('button[type="submit"]');
            const friendId = form.querySelector('input[name="friendId"]').value;
            const card = form.closest('.col-md-6, .col-lg-4');

            // Disable button and show loading
            button.disabled = true;
            const originalHtml = button.innerHTML;
            button.innerHTML = `<i class="fas fa-spinner fa-spin me-2"></i> ${window.I18n?.translations.friends.removing || 'Removing...'}`;

            const url = CultureHelper.addCultureToUrl('/Friends/RemoveFriendJson');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: new URLSearchParams({ 
                    friendId: friendId,
                    __RequestVerificationToken: getAntiForgeryToken()
                })
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

            if (!confirm(window.I18n?.translations.friends.confirmBlock || 'Are you sure you want to block this user? They will not be able to send you friend requests or interact with your content.')) {
                return;
            }

            const form = e.target;
            const button = form.querySelector('button[type="submit"]');
            const friendId = form.querySelector('input[name="friendId"]').value;
            const card = form.closest('.col-md-6, .col-lg-4');

            // Disable button and show loading
            button.disabled = true;
            const originalHtml = button.innerHTML;
            button.innerHTML = `<i class="fas fa-spinner fa-spin me-2"></i> ${window.I18n?.translations.friends.blocking || 'Blocking...'}`;

            const url = CultureHelper.addCultureToUrl('/Friends/BlockUserJson');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: new URLSearchParams({ 
                    friendId: friendId,
                    __RequestVerificationToken: getAntiForgeryToken()
                })
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
            button.innerHTML = `<i class="fas fa-spinner fa-spin me-2"></i> ${window.I18n?.translations.friends.unblocking || 'Unblocking...'}`;

            const url = CultureHelper.addCultureToUrl('/Friends/UnblockUserJson');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: new URLSearchParams({ 
                    friendId: friendId,
                    __RequestVerificationToken: getAntiForgeryToken()
                })
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

    // Live Search Functionality for Friends Search Page
    const searchInput = document.querySelector('.friends-search input[name="query"]');
    if (searchInput) {
        let searchTimeout;
        
        searchInput.addEventListener('input', function(e) {
            const query = e.target.value.trim();
            
            // Clear previous timeout
            clearTimeout(searchTimeout);
            
            // If query is empty, redirect to search page without query
            if (query.length === 0) {
                searchTimeout = setTimeout(() => {
                    const url = CultureHelper.addCultureToUrl('/Friends/Search');
                    window.location.href = url;
                }, 500);
                return;
            }
            
            // Wait for user to stop typing (debounce)
            searchTimeout = setTimeout(() => {
                if (query.length >= 2) {
                    // Redirect to search with query parameter
                    const url = CultureHelper.addCultureToUrl(`/Friends/Search?query=${encodeURIComponent(query)}`);
                    window.location.href = url;
                }
            }, 500); // Wait 500ms after user stops typing
        });
        
        // Also handle form submission
        const searchForm = document.querySelector('.friends-search form');
        if (searchForm) {
            searchForm.addEventListener('submit', function(e) {
                e.preventDefault();
                const query = searchInput.value.trim();
                if (query.length >= 2) {
                    const url = CultureHelper.addCultureToUrl(`/Friends/Search?query=${encodeURIComponent(query)}`);
                    window.location.href = url;
                }
            });
        }
    }

})();
