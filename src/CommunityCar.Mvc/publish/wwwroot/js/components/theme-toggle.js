/**
 * Theme Toggle Component
 * Handles light/dark mode switching with localStorage persistence
 */

(function () {
    'use strict';

    const ThemeToggle = {
        STORAGE_KEY: 'community-car-theme',
        THEME_LIGHT: 'light',
        THEME_DARK: 'dark',

        /**
         * Initialize theme toggle
         */
        init: function () {
            this.loadTheme();
            this.attachEventListeners();
        },

        /**
         * Load theme from localStorage or system preference
         */
        loadTheme: function () {
            const savedTheme = localStorage.getItem(this.STORAGE_KEY);

            if (savedTheme) {
                this.setTheme(savedTheme);
            } else {
                // Check system preference
                const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
                this.setTheme(prefersDark ? this.THEME_DARK : this.THEME_LIGHT);
            }
        },

        /**
         * Set theme
         * @param {string} theme - 'light' or 'dark'
         */
        setTheme: function (theme) {
            if (theme === this.THEME_DARK) {
                document.documentElement.setAttribute('data-theme', 'dark');
                this.updateIcons(true);
            } else {
                document.documentElement.removeAttribute('data-theme');
                this.updateIcons(false);
            }
            localStorage.setItem(this.STORAGE_KEY, theme);
        },

        /**
         * Update icons based on theme
         */
        updateIcons: function (isDark) {
            // Visibility is now handled purely by CSS using [data-theme] attribute
        },

        /**
         * Get current theme
         * @returns {string} Current theme
         */
        getCurrentTheme: function () {
            return document.documentElement.getAttribute('data-theme') === 'dark'
                ? this.THEME_DARK
                : this.THEME_LIGHT;
        },

        /**
         * Toggle between light and dark theme
         */
        toggleTheme: function () {
            const currentTheme = this.getCurrentTheme();
            const newTheme = currentTheme === this.THEME_LIGHT ? this.THEME_DARK : this.THEME_LIGHT;
            this.setTheme(newTheme);
        },

        /**
         * Attach event listeners
         */
        attachEventListeners: function () {
            // Find all theme toggle buttons
            const toggleButtons = document.querySelectorAll('.theme-toggle-icon');

            toggleButtons.forEach(btn => {
                // Remove existing listeners to avoid duplicates if re-initialized
                const newBtn = btn.cloneNode(true);
                btn.parentNode.replaceChild(newBtn, btn);

                newBtn.addEventListener('click', () => {
                    this.toggleTheme();
                });
            });

            // Listen for system theme changes
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
                if (!localStorage.getItem(this.STORAGE_KEY)) {
                    this.setTheme(e.matches ? this.THEME_DARK : this.THEME_LIGHT);
                }
            });
        }
    };

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => ThemeToggle.init());
    } else {
        ThemeToggle.init();
    }

    // Expose to global scope if needed
    window.ThemeToggle = ThemeToggle;

})();
