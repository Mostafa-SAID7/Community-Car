// Security Alerts JavaScript
// Handles interactions and real-time updates for security alerts

(function () {
    'use strict';

    // Initialize on page load
    document.addEventListener('DOMContentLoaded', function () {
        initializeSecurityAlerts();
    });

    function initializeSecurityAlerts() {
        // Initialize tooltips
        initializeTooltips();

        // Initialize confirm dialogs
        initializeConfirmDialogs();

        // Initialize auto-refresh for unresolved alerts
        initializeAutoRefresh();

        // Initialize filter persistence
        initializeFilterPersistence();
    }

    function initializeTooltips() {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    function initializeConfirmDialogs() {
        // Confirm delete actions
        const deleteButtons = document.querySelectorAll('form[action*="Delete"] button[type="submit"]');
        deleteButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Are you sure you want to delete this security alert? This action cannot be undone.')) {
                    e.preventDefault();
                    return false;
                }
            });
        });

        // Confirm reopen actions
        const reopenButtons = document.querySelectorAll('form[action*="Reopen"] button[type="submit"]');
        reopenButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Are you sure you want to reopen this security alert?')) {
                    e.preventDefault();
                    return false;
                }
            });
        });
    }

    function initializeAutoRefresh() {
        // Auto-refresh unresolved alerts every 30 seconds if on index page
        const alertsList = document.querySelector('.alerts-list');
        if (alertsList && window.location.pathname.includes('/Security/Index')) {
            // Only refresh if viewing unresolved alerts
            const urlParams = new URLSearchParams(window.location.search);
            const isResolved = urlParams.get('isResolved');
            
            if (isResolved === 'false' || isResolved === null) {
                setInterval(function () {
                    checkForNewAlerts();
                }, 30000); // 30 seconds
            }
        }
    }

    function checkForNewAlerts() {
        // This would typically make an AJAX call to check for new alerts
        // For now, we'll just add a visual indicator
        const header = document.querySelector('.security-header');
        if (header) {
            const badge = document.createElement('span');
            badge.className = 'badge bg-warning position-absolute top-0 end-0 m-3';
            badge.textContent = 'Checking for updates...';
            badge.style.opacity = '0';
            badge.style.transition = 'opacity 0.3s ease';
            
            header.style.position = 'relative';
            header.appendChild(badge);
            
            setTimeout(() => {
                badge.style.opacity = '1';
            }, 10);
            
            setTimeout(() => {
                badge.style.opacity = '0';
                setTimeout(() => {
                    badge.remove();
                }, 300);
            }, 2000);
        }
    }

    function initializeFilterPersistence() {
        // Save filter state to localStorage
        const filterForm = document.querySelector('.security-search-section form');
        if (filterForm) {
            filterForm.addEventListener('submit', function () {
                const formData = new FormData(filterForm);
                const filters = {};
                for (let [key, value] of formData.entries()) {
                    if (value) {
                        filters[key] = value;
                    }
                }
                localStorage.setItem('securityFilters', JSON.stringify(filters));
            });
        }

        // Restore filters from localStorage if no query params
        const urlParams = new URLSearchParams(window.location.search);
        if (urlParams.toString() === '' && filterForm) {
            const savedFilters = localStorage.getItem('securityFilters');
            if (savedFilters) {
                const filters = JSON.parse(savedFilters);
                Object.keys(filters).forEach(key => {
                    const input = filterForm.querySelector(`[name="${key}"]`);
                    if (input) {
                        input.value = filters[key];
                    }
                });
            }
        }
    }

    // Export functions for use in other scripts
    window.SecurityAlerts = {
        refresh: checkForNewAlerts
    };

    // Add animation classes to cards as they appear
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.animationDelay = '0s';
                entry.target.classList.add('animate-in');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    // Observe all alert cards
    document.querySelectorAll('.alert-card').forEach(card => {
        observer.observe(card);
    });

    // Severity badge color animation on hover
    document.querySelectorAll('.severity-badge').forEach(badge => {
        badge.addEventListener('mouseenter', function () {
            this.style.transform = 'scale(1.1) rotate(5deg)';
        });
        badge.addEventListener('mouseleave', function () {
            this.style.transform = 'scale(1) rotate(0deg)';
        });
    });

    // Add smooth scroll to top button
    const scrollButton = document.createElement('button');
    scrollButton.innerHTML = '<i class="fas fa-arrow-up"></i>';
    scrollButton.className = 'btn btn-primary rounded-circle position-fixed bottom-0 end-0 m-4';
    scrollButton.style.width = '50px';
    scrollButton.style.height = '50px';
    scrollButton.style.display = 'none';
    scrollButton.style.zIndex = '1000';
    scrollButton.style.boxShadow = '0 4px 15px rgba(0, 0, 0, 0.2)';
    scrollButton.onclick = function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    document.body.appendChild(scrollButton);

    window.addEventListener('scroll', function () {
        if (window.pageYOffset > 300) {
            scrollButton.style.display = 'block';
        } else {
            scrollButton.style.display = 'none';
        }
    });

})();
