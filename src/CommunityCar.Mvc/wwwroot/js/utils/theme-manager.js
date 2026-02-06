/**
 * Theme Manager
 * Handles light/dark mode switching, icon toggling, and persistence
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

        // Update Icons
        this.updateIcons(theme === this.THEME_DARK);

        // Dispatch custom event for other components
        window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme } }));
    }

    updateIcons(isDark) {
        const moonIcons = document.querySelectorAll('.theme-icon-moon');
        const sunIcons = document.querySelectorAll('.theme-icon-sun');

        moonIcons.forEach(icon => {
            if (isDark) icon.classList.add('d-none');
            else icon.classList.remove('d-none');
        });

        sunIcons.forEach(icon => {
            if (isDark) icon.classList.remove('d-none');
            else icon.classList.add('d-none');
        });
    }

    toggleTheme() {
        const currentTheme = this.getCurrentTheme();
        const newTheme = currentTheme === this.THEME_LIGHT ? this.THEME_DARK : this.THEME_LIGHT;
        this.setTheme(newTheme);
    }

    getCurrentTheme() {
        return document.documentElement.getAttribute('data-theme') || this.THEME_LIGHT;
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

        // Setup toggle buttons using delegation for better reliability
        document.addEventListener('click', (e) => {
            const toggleBtn = e.target.closest('.theme-toggle') || e.target.closest('.theme-toggle-icon');
            if (toggleBtn) {
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
