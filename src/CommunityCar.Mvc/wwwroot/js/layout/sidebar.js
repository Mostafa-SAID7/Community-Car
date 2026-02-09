/**
 * Sidebar Component
 * Handles toggle functionality for mobile responsiveness using Bootstrap & jQuery
 */

window.Sidebar = (function ($) {
    'use strict';

    const SELECTORS = {
        TOGGLE_BTN: '.sidebar-toggle-btn',
        SIDEBAR: '.sidebar-left',
        BACKDROP: '.sidebar-backdrop',
        BODY: 'body'
    };

    const CLASSES = {
        OPEN: 'sidebar-open'
    };

    function init() {
        console.log("Sidebar initialized with Bootstrap & jQuery");
        
        // Toggle sidebar on button click
        $(document).on('click', SELECTORS.TOGGLE_BTN, function(e) {
            e.preventDefault();
            toggleSidebar();
        });

        // Close sidebar when clicking backdrop
        $(document).on('click', SELECTORS.BACKDROP, function() {
            closeSidebar();
        });

        // Close sidebar when clicking a nav link on mobile
        $(document).on('click', `${SELECTORS.SIDEBAR} a`, function() {
            if ($(window).width() < 768) {
                closeSidebar();
            }
        });
    }

    function toggleSidebar() {
        $('body').toggleClass(CLASSES.OPEN);
        console.log("Sidebar toggled", $('body').hasClass(CLASSES.OPEN));
    }

    function closeSidebar() {
        $('body').removeClass(CLASSES.OPEN);
    }

    function openSidebar() {
        $('body').addClass(CLASSES.OPEN);
    }

    // Expose public API
    return {
        init: init,
        toggle: toggleSidebar,
        close: closeSidebar,
        open: openSidebar
    };
})(jQuery);
