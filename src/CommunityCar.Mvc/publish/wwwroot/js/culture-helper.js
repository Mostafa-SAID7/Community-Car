/**
 * Culture Helper - Utility functions for handling culture-aware URLs
 */
(function(window) {
    'use strict';

    /**
     * Get the current culture code from the URL path
     * @returns {string} The culture code (e.g., 'en', 'ar') or 'en' as default
     */
    function getCurrentCulture() {
        const pathParts = window.location.pathname.split('/').filter(p => p);
        // Check if first part is a 2-letter culture code
        if (pathParts.length > 0 && pathParts[0].length === 2 && /^[a-z]{2}$/i.test(pathParts[0])) {
            return pathParts[0];
        }
        return 'en'; // default culture
    }

    /**
     * Build a culture-aware URL
     * @param {string} path - The path without culture prefix (e.g., '/News/AddComment')
     * @returns {string} The full URL with culture prefix (e.g., '/en/News/AddComment')
     */
    function buildUrl(path) {
        const culture = getCurrentCulture();
        
        // Remove leading slash if present
        path = path.replace(/^\//, '');
        
        // Check if path already has culture prefix
        if (path.startsWith(culture + '/')) {
            return '/' + path;
        }
        
        return '/' + culture + '/' + path;
    }

    /**
     * Wrapper for jQuery $.post that automatically adds culture prefix
     * @param {string} url - The URL without culture prefix
     * @param {object|function} data - Data to send or success callback
     * @param {function} success - Success callback (if data is provided)
     * @returns {jqXHR} jQuery XHR object
     */
    function culturePost(url, data, success) {
        const fullUrl = buildUrl(url);
        return $.post(fullUrl, data, success);
    }

    /**
     * Wrapper for jQuery $.ajax that automatically adds culture prefix to URL
     * @param {object} settings - jQuery AJAX settings object
     * @returns {jqXHR} jQuery XHR object
     */
    function cultureAjax(settings) {
        if (settings.url) {
            settings.url = buildUrl(settings.url);
        }
        return $.ajax(settings);
    }

    // Expose functions to global scope
    window.CultureHelper = {
        getCurrentCulture: getCurrentCulture,
        buildUrl: buildUrl,
        addCultureToUrl: buildUrl, // Alias for consistency
        post: culturePost,
        ajax: cultureAjax
    };

})(window);
