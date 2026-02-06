/**
 * Theme Manager
 * Handles light/dark mode switching and persistence
 */

class ThemeManager {
    constructor() {
        this.THEME_KEY = 'community-car-theme';
        this.THEME_LIGHT = 'light';
        this.THEME_DARK = 'dark';
        
        this.init();
    }

    init() {
        // Load saved theme or detect system preference
        const savedTheme = this.getSavedTheme();
        const systemTheme = this.getSystemTheme();
        const theme = savedTheme || systemTheme;
        
        this.setTheme(theme, false);
        this.setupListeners();
    }

    getSavedTheme() {
        return localStorage.getItem(this.THEME_KEY);
    }

    getSystemTheme() {
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return this.THEME_DARK;
        }
        return this.THEME_LIGHT;
    }

    setTheme(theme, save = true) {
        document.documentElement.setAttribute('data-theme', theme);
        
        if (save) {
            localStorage.setItem(this.THEME_KEY, theme);
        }

        // Dispatch custom event for other components
        window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme } }));
    }

    toggleTheme() {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        const newTheme = currentTheme === this.THEME_LIGHT ? this.THEME_DARK : this.THEME_LIGHT;
        this.setTheme(newTheme);
    }

    getCurrentTheme() {
        return document.documentElement.getAttribute('data-theme');
    }

    setupListeners() {
        // Listen for system theme changes
        if (window.matchMedia) {
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
                if (!this.getSavedTheme()) {
                    this.setTheme(e.matches ? this.THEME_DARK : this.THEME_LIGHT, false);
                }
            });
        }

        // Setup toggle buttons
        document.addEventListener('click', (e) => {
            if (e.target.closest('.theme-toggle') || e.target.closest('.theme-toggle-icon')) {
                this.toggleTheme();
            }
        });
    }
}

// Initialize theme manager when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.themeManager = new ThemeManager();
    });
} else {
    window.themeManager = new ThemeManager();
}
