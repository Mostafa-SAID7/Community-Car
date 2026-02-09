/**
 * Community Car - Main JavaScript Entry Point
 * This file orchestrates the initialization of global components.
 */

(function () {
    'use strict';

    function init() {
        console.log("Community Car Web application initialized");

        // Initialize global layout components if needed
        if (window.Header) window.Header.init();
        if (window.Navigation) window.Navigation.init();
        if (window.Sidebar) window.Sidebar.init();
    }

    document.addEventListener('DOMContentLoaded', init);
})();
