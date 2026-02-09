/**
 * Application Core - Main initialization and module management
 * Clean architecture entry point
 */

class App {
    constructor() {
        this.modules = new Map();
        this.initialized = false;
        this.config = {
            debug: window.location.search.includes('debug=true'),
            apiBaseUrl: '',
            culture: document.documentElement.lang || 'en'
        };
    }

    /**
     * Register a module
     */
    register(name, module) {
        if (this.modules.has(name)) {
            console.warn(`Module "${name}" is already registered`);
            return;
        }
        this.modules.set(name, module);
        if (this.config.debug) {
            console.log(`✅ Module registered: ${name}`);
        }
    }

    /**
     * Get a module
     */
    get(name) {
        return this.modules.get(name);
    }

    /**
     * Initialize application
     */
    async init() {
        if (this.initialized) {
            console.warn('App already initialized');
            return;
        }

        console.log('🚀 Initializing Community Car Application...');

        try {
            // Initialize core modules
            await this.initCoreModules();

            // Initialize page-specific modules
            await this.initPageModules();

            // Initialize components
            await this.initComponents();

            this.initialized = true;
            console.log('✅ Application initialized successfully');

            // Emit app ready event
            if (window.eventBus) {
                window.eventBus.emit('app:ready');
            }

        } catch (error) {
            console.error('❌ Application initialization failed:', error);
            throw error;
        }
    }

    /**
     * Initialize core modules
     */
    async initCoreModules() {
        // API Client
        if (window.api) {
            this.register('api', window.api);
        }

        // Event Bus
        if (window.eventBus) {
            this.register('eventBus', window.eventBus);
        }

        // Storage Manager
        if (window.StorageManager) {
            this.register('storage', window.StorageManager);
        }

        // Theme Manager
        if (window.ThemeManager) {
            const themeManager = new window.ThemeManager();
            themeManager.init();
            this.register('theme', themeManager);
        }
    }

    /**
     * Initialize page-specific modules
     */
    async initPageModules() {
        const body = document.body;
        const pageType = body.dataset.page;

        if (!pageType) return;

        // Map page types to module names
        const pageModules = {
            'home': 'HomePage',
            'dashboard': 'DashboardPage',
            'friends': 'FriendsPage',
            'login': 'LoginPage'
        };

        const moduleName = pageModules[pageType];
        if (moduleName && window[moduleName]) {
            const module = new window[moduleName]();
            if (module.init) {
                await module.init();
            }
            this.register(`page:${pageType}`, module);
            
            if (this.config.debug) {
                console.log(`✅ Page module initialized: ${moduleName}`);
            }
        }
    }

    /**
     * Initialize components
     */
    async initComponents() {
        // Header
        if (window.Header && window.Header.init) {
            window.Header.init();
            this.register('header', window.Header);
        }

        // Navigation
        if (window.Navigation && window.Navigation.init) {
            window.Navigation.init();
            this.register('navigation', window.Navigation);
        }

        // Sidebar
        if (window.Sidebar && window.Sidebar.init) {
            window.Sidebar.init();
            this.register('sidebar', window.Sidebar);
        }

        // Toast
        if (window.Toast) {
            this.register('toast', window.Toast);
        }

        // Modal
        if (window.Modal) {
            this.register('modal', window.Modal);
        }

        // Notification
        if (window.Notification) {
            this.register('notification', window.Notification);
        }
    }

    /**
     * Get configuration value
     */
    getConfig(key) {
        return this.config[key];
    }

    /**
     * Set configuration value
     */
    setConfig(key, value) {
        this.config[key] = value;
    }

    /**
     * Check if debug mode is enabled
     */
    isDebug() {
        return this.config.debug;
    }

    /**
     * Log debug message
     */
    debug(...args) {
        if (this.config.debug) {
            console.log('[DEBUG]', ...args);
        }
    }
}

// Create global app instance
window.app = new App();

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.app.init();
    });
} else {
    window.app.init();
}
