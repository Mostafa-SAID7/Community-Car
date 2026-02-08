"use strict";

$(document).ready(function () {

    // Get current culture from URL path
    function getCurrentCulture() {
        var pathParts = window.location.pathname.split('/').filter(p => p);
        // Check if first part is a 2-letter culture code
        if (pathParts.length > 0 && pathParts[0].length === 2 && /^[a-z]{2}$/i.test(pathParts[0])) {
            return pathParts[0];
        }
        return 'en'; // default
    }

    // Helper to handle AJAX form submissions
    function handleAjaxForm(form, options) {
        var btn = form.find('button[type="submit"]');
        var originalBtnContent = btn.html();

        // Get the action URL from the form
        var url = form.attr('action');
        
        // If URL doesn't already end with 'Json', append it
        if (!url.endsWith('Json')) {
            // Check if URL already has culture prefix
            var culture = getCurrentCulture();
            var hasCulturePrefix = url.startsWith('/' + culture + '/');
            
            if (!hasCulturePrefix && !url.startsWith('http')) {
                // Add culture prefix if missing
                url = '/' + culture + url;
            }
            
            // Append 'Json' to the action name
            url += 'Json';
        }

        if (options.confirmMsg && !confirm(options.confirmMsg)) {
            return;
        }

        // Disable button and show spinner
        btn.prop('disabled', true);
        btn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>');

        $.ajax({
            url: url,
            type: 'POST',
            data: form.serialize(),
            success: function (response) {
                if (response.success) {
                    // Show success toast
                    if (window.Toast) {
                        window.Toast.show(response.message, 'success');
                    }

                    // Execute success callback
                    if (options.onSuccess) {
                        options.onSuccess(form, btn, response);
                    }
                } else {
                    // Revert button state
                    btn.prop('disabled', false);
                    btn.html(originalBtnContent);

                    // Show error toast
                    if (window.Toast) {
                        window.Toast.show(response.message, 'error');
                    } else {
                        alert(response.message);
                    }
                }
            },
            error: function (xhr) {
                // Revert button state
                btn.prop('disabled', false);
                btn.html(originalBtnContent);

                var msg = 'An error occurred. Please try again.';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    msg = xhr.responseJSON.message;
                }

                if (window.Toast) {
                    window.Toast.show(msg, 'error');
                } else {
                    alert(msg);
                }
            }
        });
    }

    // 1. Add Friend
    $(document).on('submit', '.ajax-add-friend-form', function (e) {
        e.preventDefault();
        handleAjaxForm($(this), {
            onSuccess: function (form, btn) {
                btn.removeClass('btn-primary-premium').addClass('btn-secondary');
                btn.html('<i class="fas fa-clock me-1"></i> Pending');
                btn.prop('disabled', true);
            }
        });
    });

    // 2. Accept Request
    $(document).on('submit', '.ajax-accept-form', function (e) {
        e.preventDefault();
        var form = $(this);
        var notificationId = form.data('notification-id');

        handleAjaxForm(form, {
            onSuccess: function (form) {
                // 1. Handle Notification List if applicable
                if (notificationId) {
                    if (window.markAsRead) window.markAsRead(notificationId);
                    var item = form.closest('.notification-item');
                    item.fadeOut(300, function () {
                        $(this).remove();
                        if (window.updateUnreadCount) window.updateUnreadCount();
                    });
                    return;
                }

                // 2. Handle standard friend card
                var cardCol = form.closest('.col-md-6, .col-lg-4');
                cardCol.fadeOut(300, function () {
                    $(this).remove();
                    checkEmptyState('.friends-empty-placeholder', 'No Friend Requests', 'You re all caught up!');
                });
            }
        });
    });

    // 3. Reject Request
    $(document).on('submit', '.ajax-reject-form', function (e) {
        e.preventDefault();
        var form = $(this);
        var notificationId = form.data('notification-id');

        handleAjaxForm(form, {
            confirmMsg: 'Are you sure you want to decline this request?',
            onSuccess: function (form) {
                // 1. Handle Notification List if applicable
                if (notificationId) {
                    if (window.markAsRead) window.markAsRead(notificationId);
                    var item = form.closest('.notification-item');
                    item.fadeOut(300, function () {
                        $(this).remove();
                        if (window.updateUnreadCount) window.updateUnreadCount();
                    });
                    return;
                }

                // 2. Handle standard friend card
                var cardCol = form.closest('.col-md-6, .col-lg-4');
                cardCol.fadeOut(300, function () {
                    $(this).remove();
                    checkEmptyState('.friends-empty-placeholder');
                });
            }
        });
    });

    // 4. Remove Friend
    $(document).on('submit', '.ajax-remove-form', function (e) {
        e.preventDefault();
        handleAjaxForm($(this), {
            confirmMsg: 'Are you sure you want to remove this friend?',
            onSuccess: function (form) {
                var cardCol = form.closest('.col-md-6, .col-lg-4');
                cardCol.fadeOut(300, function () {
                    $(this).remove();
                    checkEmptyState();
                });
            }
        });
    });

    // 5. Block User
    $(document).on('submit', '.ajax-block-form', function (e) {
        e.preventDefault();
        handleAjaxForm($(this), {
            confirmMsg: 'Are you sure you want to block this user?',
            onSuccess: function (form) {
                var cardCol = form.closest('.col-md-6, .col-lg-4');
                cardCol.fadeOut(300, function () {
                    $(this).remove();
                    checkEmptyState();
                });
            }
        });
    });

    // 6. Unblock User
    $(document).on('submit', '.ajax-unblock-form', function (e) {
        e.preventDefault();
        handleAjaxForm($(this), {
            confirmMsg: 'Are you sure you want to unblock this user?',
            onSuccess: function (form) {
                var cardCol = form.closest('.col-md-6, .col-lg-4');
                cardCol.fadeOut(300, function () {
                    $(this).remove();
                    checkEmptyState();
                });
            }
        });
    });

    function checkEmptyState(placeholderSelector = '.friends-empty', title = 'Nothing here', msg = 'List is empty') {
        // If no more columns, show refresh or empty state
        if ($('.col-md-6:visible, .col-lg-4:visible').length === 0) {
            location.reload(); // Simplest way to show correct empty state for now
        }
    }
});
