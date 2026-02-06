// Security Alerts JavaScript
// Handles interactive features for security alerts

(function () {
    'use strict';

    // Auto-refresh alerts every 30 seconds
    let autoRefreshInterval = null;

    function initAutoRefresh() {
        const refreshEnabled = localStorage.getItem('security-auto-refresh') === 'true';
        if (refreshEnabled) {
            startAutoRefresh();
        }
    }

    function startAutoRefresh() {
        if (autoRefreshInterval) return;
        
        autoRefreshInterval = setInterval(() => {
            refreshAlertsList();
        }, 30000); // 30 seconds
        
        console.log('Auto-refresh enabled for security alerts');
    }

    function stopAutoRefresh() {
        if (autoRefreshInterval) {
            clearInterval(autoRefreshInterval);
            autoRefreshInterval = null;
            console.log('Auto-refresh disabled for security alerts');
        }
    }

    function refreshAlertsList() {
        const currentUrl = window.location.href;
        
        fetch(currentUrl, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(response => response.text())
        .then(html => {
            const alertsList = document.getElementById('alertsList');
            if (alertsList) {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const newAlertsList = doc.getElementById('alertsList');
                if (newAlertsList) {
                    alertsList.innerHTML = newAlertsList.innerHTML;
                    console.log('Alerts list refreshed');
                }
            }
        })
        .catch(error => {
            console.error('Error refreshing alerts:', error);
        });
    }

    // Filter form auto-submit on change
    function initFilterAutoSubmit() {
        const filterForm = document.getElementById('filterForm');
        if (!filterForm) return;

        const selects = filterForm.querySelectorAll('select');
        selects.forEach(select => {
            select.addEventListener('change', () => {
                filterForm.submit();
            });
        });
    }

    // Keyboard shortcuts
    function initKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Ctrl/Cmd + K: Focus search
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                const searchInput = document.querySelector('input[name="searchTerm"]');
                if (searchInput) {
                    searchInput.focus();
                    searchInput.select();
                }
            }

            // Ctrl/Cmd + N: Create new alert
            if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
                e.preventDefault();
                const createBtn = document.querySelector('a[href*="Create"]');
                if (createBtn) {
                    window.location.href = createBtn.href;
                }
            }

            // R: Refresh alerts
            if (e.key === 'r' && !e.ctrlKey && !e.metaKey && !e.altKey) {
                const activeElement = document.activeElement;
                if (activeElement.tagName !== 'INPUT' && activeElement.tagName !== 'TEXTAREA') {
                    e.preventDefault();
                    refreshAlertsList();
                }
            }
        });
    }

    // Animate stat cards on scroll
    function initScrollAnimations() {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry, index) => {
                if (entry.isIntersecting) {
                    setTimeout(() => {
                        entry.target.style.animationDelay = `${index * 0.1}s`;
                        entry.target.classList.add('animate-in');
                    }, index * 100);
                    observer.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.1
        });

        document.querySelectorAll('.stat-card, .alert-card').forEach(card => {
            observer.observe(card);
        });
    }

    // Confirm delete with better UX
    window.confirmDeleteAlert = function(alertId, alertTitle) {
        return confirm(`Are you sure you want to delete the alert "${alertTitle}"?\n\nThis action cannot be undone.`);
    };

    // Toast notifications
    function showToast(message, type = 'info') {
        // Check if Bootstrap toast is available
        if (typeof bootstrap !== 'undefined' && bootstrap.Toast) {
            const toastHtml = `
                <div class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                    <div class="d-flex">
                        <div class="toast-body">
                            ${message}
                        </div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                    </div>
                </div>
            `;
            
            let toastContainer = document.querySelector('.toast-container');
            if (!toastContainer) {
                toastContainer = document.createElement('div');
                toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
                document.body.appendChild(toastContainer);
            }
            
            toastContainer.insertAdjacentHTML('beforeend', toastHtml);
            const toastElement = toastContainer.lastElementChild;
            const toast = new bootstrap.Toast(toastElement);
            toast.show();
            
            toastElement.addEventListener('hidden.bs.toast', () => {
                toastElement.remove();
            });
        } else {
            // Fallback to alert
            alert(message);
        }
    }

    // Export functions for global use
    window.SecurityAlerts = {
        refresh: refreshAlertsList,
        showToast: showToast,
        startAutoRefresh: startAutoRefresh,
        stopAutoRefresh: stopAutoRefresh
    };

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            initAutoRefresh();
            initFilterAutoSubmit();
            initKeyboardShortcuts();
            initScrollAnimations();
        });
    } else {
        initAutoRefresh();
        initFilterAutoSubmit();
        initKeyboardShortcuts();
        initScrollAnimations();
    }

    // Cleanup on page unload
    window.addEventListener('beforeunload', () => {
        stopAutoRefresh();
    });

})();
