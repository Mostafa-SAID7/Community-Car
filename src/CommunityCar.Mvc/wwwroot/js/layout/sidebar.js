/**
 * Sidebar Component
 * Handles toggle functionality for mobile responsiveness
 */

window.Sidebar = (function () {
    const SELECTORS = {
        TOGGLE_BTN: '#sidebarToggle',
        SIDEBAR: '.sidebar-left',
        BACKDROP: '.sidebar-backdrop',
        BODY: 'body'
    };

    const CLASSES = {
        OPEN: 'sidebar-open',
        SHOW: 'show'
    };

    function init() {
        const toggleBtn = document.querySelector(SELECTORS.TOGGLE_BTN);
        if (toggleBtn) {
            toggleBtn.addEventListener('click', toggleSidebar);
        }

        // Close sidebar when clicking backdrop
        const backdrop = document.querySelector(SELECTORS.BACKDROP);
        if (backdrop) {
            backdrop.addEventListener('click', closeSidebar);
        }
    }

    function toggleSidebar(e) {
        if (e) e.preventDefault();
        document.body.classList.toggle(CLASSES.OPEN);
    }

    function closeSidebar() {
        document.body.classList.remove(CLASSES.OPEN);
    }

    // Expose public API
    return {
        init: init,
        toggle: toggleSidebar,
        close: closeSidebar
    };
})();
