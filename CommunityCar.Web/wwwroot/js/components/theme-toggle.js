/**
 * Theme Toggle Component
 * Handles light/dark mode switching with localStorage persistence
 */

(function() {
    'use strict';

    const ThemeToggle = {
        STORAGE_KEY: 'community-car-theme',
        THEME_LIGHT: 'light',
        THEME_DARK: 'dark',

        /**
         * Initialize theme toggle
         */
        init: function() {
            this.loadTheme();
            this.createToggleButton();
            this.attachEventListeners();
        },

        /**
         * Load theme from localStorage or system preference
         */
        loadTheme: function() {
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
        setTheme: function(theme) {
            if (theme === this.THEME_DARK) {
                document.documentElement.setAttribute('data-theme', 'dark');
            } else {
                document.documentElement.removeAttribute('data-theme');
            }
            localStorage.setItem(this.STORAGE_KEY, theme);
        },

        /**
         * Get current theme
         * @returns {string} Current theme
         */
        getCurrentTheme: function() {
            return document.documentElement.getAttribute('data-theme') === 'dark' 
                ? this.THEME_DARK 
                : this.THEME_LIGHT;
        },

        /**
         * Toggle between light and dark theme
         */
        toggleTheme: function() {
            const currentTheme = this.getCurrentTheme();
            const newTheme = currentTheme === this.THEME_LIGHT ? this.THEME_DARK : this.THEME_LIGHT;
            this.setTheme(newTheme);
        },

        /**
         * Create toggle button HTML
         */
        createToggleButton: function() {
            const toggleButton = document.createElement('button');
            toggleButton.className = 'theme-toggle';
            toggleButton.setAttribute('aria-label', 'Toggle theme');
            toggleButton.innerHTML = `
                <span class="theme-toggle-icon">
                    <svg class="theme-icon-sun" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <circle cx="12" cy="12" r="5"></circle>
                        <line x1="12" y1="1" x2="12" y2="3"></line>
                        <line x1="12" y1="21" x2="12" y2="23"></line>
                        <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line>
                        <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line>
                        <line x1="1" y1="12" x2="3" y2="12"></line>
                        <line x1="21" y1="12" x2="23" y2="12"></line>
                        <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line>
                        <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line>
                    </svg>
                    <svg class="theme-icon-moon" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path>
                    </svg>
                </span>
                <span class="theme-toggle-text">Theme</span>
            `;
            
            document.body.appendChild(toggleButton);
            this.toggleButton = toggleButton;
        },

        /**
         * Attach event listeners
         */
        attachEventListeners: function() {
            if (this.toggleButton) {
                this.toggleButton.addEventListener('click', () => {
                    this.toggleTheme();
                });
            }

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
