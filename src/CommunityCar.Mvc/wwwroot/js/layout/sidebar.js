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
        console.log("Sidebar initialized");
        const toggleBtns = document.querySelectorAll(SELECTORS.TOGGLE_BTN);
        if (toggleBtns.length > 0) {
            toggleBtns.forEach(btn => {
                btn.addEventListener('click', (e) => {
                    e.preventDefault();
                    toggleSidebar();
                });
            });
        }

        // Close sidebar when clicking backdrop
        const backdrop = document.querySelector(SELECTORS.BACKDROP);
        if (backdrop) {
            backdrop.addEventListener('click', closeSidebar);
        }

        // Close sidebar when clicking a nav link on mobile
        const sidebarLinks = document.querySelectorAll(`${SELECTORS.SIDEBAR} a`);
        sidebarLinks.forEach(link => {
            link.addEventListener('click', () => {
                if (window.innerWidth < 768) {
                    closeSidebar();
                }
            });
        });
    }

    function toggleSidebar() {
        document.body.classList.toggle(CLASSES.OPEN);
        console.log("Sidebar toggled", document.body.classList.contains(CLASSES.OPEN));
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
