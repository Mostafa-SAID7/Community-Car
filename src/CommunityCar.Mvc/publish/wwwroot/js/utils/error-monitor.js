/**
 * Browser Error Monitor
 * Captures and logs all JavaScript errors, AJAX errors, and console messages
 */

class ErrorMonitor {
    constructor(options = {}) {
        this.options = {
            enableConsoleCapture: true,
            enableAjaxCapture: true,
            enableClickCapture: true,
            enableErrorCapture: true,
            logToServer: true,
            logToConsole: true,
            serverEndpoint: '/api/logs/client-error',
            ...options
        };

        this.errors = [];
        this.maxErrors = 100;
        this.originalConsole = {};

        this.init();
    }

    init() {
        if (this.options.enableErrorCapture) {
            this.captureGlobalErrors();
        }

        if (this.options.enableConsoleCapture) {
            this.captureConsoleMessages();
        }

        if (this.options.enableAjaxCapture) {
            this.captureAjaxErrors();
        }

        if (this.options.enableClickCapture) {
            this.captureClickEvents();
        }

        this.captureUnhandledPromises();
        this.captureResourceErrors();
    }

    // Capture global JavaScript errors
    captureGlobalErrors() {
        window.addEventListener('error', (event) => {
            const errorInfo = {
                type: 'JavaScript Error',
                message: event.message,
                filename: event.filename,
                line: event.lineno,
                column: event.colno,
                stack: event.error?.stack,
                timestamp: new Date().toISOString(),
                url: window.location.href,
                userAgent: navigator.userAgent
            };

            this.logError(errorInfo);
        });
    }

    // Capture unhandled promise rejections
    captureUnhandledPromises() {
        window.addEventListener('unhandledrejection', (event) => {
            const errorInfo = {
                type: 'Unhandled Promise Rejection',
                message: event.reason?.message || event.reason,
                stack: event.reason?.stack,
                timestamp: new Date().toISOString(),
                url: window.location.href,
                userAgent: navigator.userAgent
            };

            this.logError(errorInfo);
        });
    }

    // Capture resource loading errors
    captureResourceErrors() {
        window.addEventListener('error', (event) => {
            if (event.target !== window) {
                const errorInfo = {
                    type: 'Resource Load Error',
                    message: `Failed to load: ${event.target.src || event.target.href}`,
                    element: event.target.tagName,
                    timestamp: new Date().toISOString(),
                    url: window.location.href
                };

                this.logError(errorInfo);
            }
        }, true);
    }

    // Capture console messages
    captureConsoleMessages() {
        const methods = ['log', 'warn', 'error', 'info', 'debug'];

        methods.forEach(method => {
            this.originalConsole[method] = console[method];

            console[method] = (...args) => {
                // Call original console method
                this.originalConsole[method].apply(console, args);

                // Log to our system
                if (method === 'error' || method === 'warn') {
                    const errorInfo = {
                        type: `Console ${method.toUpperCase()}`,
                        message: args.map(arg =>
                            typeof arg === 'object' ? JSON.stringify(arg) : String(arg)
                        ).join(' '),
                        timestamp: new Date().toISOString(),
                        url: window.location.href,
                        stack: new Error().stack
                    };

                    this.logError(errorInfo);
                }
            };
        });
    }

    // Capture AJAX/Fetch errors
    captureAjaxErrors() {
        // Capture XMLHttpRequest errors
        const originalXHR = window.XMLHttpRequest;
        const self = this;

        window.XMLHttpRequest = function () {
            const xhr = new originalXHR();
            const originalOpen = xhr.open;
            const originalSend = xhr.send;
            let requestInfo = {};

            xhr.open = function (method, url, ...args) {
                requestInfo = { method, url };
                return originalOpen.apply(this, [method, url, ...args]);
            };

            xhr.send = function (...args) {
                xhr.addEventListener('error', function () {
                    const errorInfo = {
                        type: 'AJAX Error',
                        message: `${requestInfo.method} ${requestInfo.url} failed`,
                        status: xhr.status,
                        statusText: xhr.statusText,
                        timestamp: new Date().toISOString(),
                        url: window.location.href
                    };
                    self.logError(errorInfo);
                });

                xhr.addEventListener('load', function () {
                    if (xhr.status >= 400) {
                        const errorInfo = {
                            type: 'AJAX HTTP Error',
                            message: `${requestInfo.method} ${requestInfo.url} returned ${xhr.status}`,
                            status: xhr.status,
                            statusText: xhr.statusText,
                            response: xhr.responseText?.substring(0, 500),
                            timestamp: new Date().toISOString(),
                            url: window.location.href
                        };
                        self.logError(errorInfo);
                    }
                });

                return originalSend.apply(this, args);
            };

            return xhr;
        };

        // Capture Fetch errors
        const originalFetch = window.fetch;
        window.fetch = function (...args) {
            return originalFetch.apply(this, args)
                .then(response => {
                    if (!response.ok) {
                        const errorInfo = {
                            type: 'Fetch HTTP Error',
                            message: `Fetch ${args[0]} returned ${response.status}`,
                            status: response.status,
                            statusText: response.statusText,
                            timestamp: new Date().toISOString(),
                            url: window.location.href
                        };
                        self.logError(errorInfo);
                    }
                    return response;
                })
                .catch(error => {
                    const errorInfo = {
                        type: 'Fetch Error',
                        message: `Fetch ${args[0]} failed: ${error.message}`,
                        stack: error.stack,
                        timestamp: new Date().toISOString(),
                        url: window.location.href
                    };
                    self.logError(errorInfo);
                    throw error;
                });
        };
    }

    // Capture click events for debugging
    captureClickEvents() {
        document.addEventListener('click', (event) => {
            const target = event.target;
            const clickInfo = {
                type: 'Click Event',
                element: target.tagName,
                id: target.id,
                className: target.className,
                text: target.textContent?.substring(0, 50),
                href: target.href,
                timestamp: new Date().toISOString(),
                url: window.location.href
            };

            // Only log clicks on interactive elements
            if (target.tagName === 'A' || target.tagName === 'BUTTON' ||
                target.onclick || target.getAttribute('data-action')) {
                this.logInfo(clickInfo);
            }
        });
    }

    // Log error to storage and optionally to server
    logError(errorInfo) {
        // Ignore SignalR WebSocket closure errors (status 1006) - these are handled by SignalR's reconnection logic
        if (errorInfo.message && (
            errorInfo.message.includes('WebSocket closed with status code: 1006') ||
            errorInfo.message.includes('Connection closed with an error') ||
            errorInfo.message.includes('NotificationHub') ||
            errorInfo.message.includes('ChatHub') ||
            errorInfo.message.includes('FriendHub')
        )) {
            // Log to original console in debug mode only to avoid recursion
            if (window.location.search.includes('debug=true')) {
                this.originalConsole.debug?.('SignalR connection issue (expected):', errorInfo.message);
            }
            return; // Don't log these errors
        }

        this.errors.push(errorInfo);

        // Keep only last N errors
        if (this.errors.length > this.maxErrors) {
            this.errors.shift();
        }

        // Log to console if enabled using ORIGINAL methods to prevent infinite loop
        if (this.options.logToConsole) {
            this.originalConsole.group?.(`🔴 ${errorInfo.type}`);
            this.originalConsole.error?.('Message:', errorInfo.message);
            this.originalConsole.error?.('Details:', errorInfo);
            this.originalConsole.groupEnd?.();
        }

        // Send to server if enabled
        if (this.options.logToServer) {
            this.sendToServer(errorInfo);
        }

        // Store in localStorage for persistence
        this.saveToLocalStorage();
    }

    // Log info (non-error events)
    logInfo(info) {
        if (this.options.logToConsole && window.location.search.includes('debug=true')) {
            console.log('ℹ️', info.type, info);
        }
    }

    // Send error to server
    sendToServer(errorInfo) {
        try {
            const xhr = new XMLHttpRequest();
            xhr.open('POST', this.options.serverEndpoint, true);
            xhr.setRequestHeader('Content-Type', 'application/json');
            xhr.send(JSON.stringify(errorInfo));
        } catch (e) {
            // Silently fail if server logging fails
            this.originalConsole.error?.('Failed to send error to server:', e);
        }
    }

    // Save errors to localStorage
    saveToLocalStorage() {
        try {
            localStorage.setItem('errorMonitor_errors', JSON.stringify(this.errors));
        } catch (e) {
            // Ignore localStorage errors
        }
    }

    // Get all logged errors
    getErrors() {
        return this.errors;
    }

    // Get errors by type
    getErrorsByType(type) {
        return this.errors.filter(error => error.type === type);
    }

    // Clear all errors
    clearErrors() {
        this.errors = [];
        localStorage.removeItem('errorMonitor_errors');
    }

    // Export errors as JSON
    exportErrors() {
        const dataStr = JSON.stringify(this.errors, null, 2);
        const dataBlob = new Blob([dataStr], { type: 'application/json' });
        const url = URL.createObjectURL(dataBlob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `errors_${new Date().toISOString()}.json`;
        link.click();
    }

    // Display errors in a modal
    showErrorPanel() {
        const panel = document.createElement('div');
        panel.id = 'error-monitor-panel';
        panel.style.cssText = `
            position: fixed;
            top: 10px;
            right: 10px;
            width: 400px;
            max-height: 600px;
            background: white;
            border: 2px solid #dc3545;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.3);
            z-index: 999999;
            overflow: hidden;
            font-family: monospace;
            font-size: 12px;
        `;

        const header = `
            <div style="background: #dc3545; color: white; padding: 10px; display: flex; justify-content: space-between; align-items: center;">
                <strong>🔴 Error Monitor (${this.errors.length})</strong>
                <div>
                    <button onclick="errorMonitor.exportErrors()" style="margin-right: 5px; padding: 2px 8px; cursor: pointer;">Export</button>
                    <button onclick="errorMonitor.clearErrors(); errorMonitor.showErrorPanel();" style="margin-right: 5px; padding: 2px 8px; cursor: pointer;">Clear</button>
                    <button onclick="document.getElementById('error-monitor-panel').remove()" style="padding: 2px 8px; cursor: pointer;">✕</button>
                </div>
            </div>
        `;

        const errorList = this.errors.slice(-20).reverse().map(error => `
            <div style="padding: 10px; border-bottom: 1px solid #eee;">
                <div style="color: #dc3545; font-weight: bold;">${error.type}</div>
                <div style="margin: 5px 0;">${error.message}</div>
                <div style="color: #666; font-size: 10px;">${error.timestamp}</div>
            </div>
        `).join('');

        panel.innerHTML = header + `<div style="max-height: 500px; overflow-y: auto;">${errorList || '<div style="padding: 20px; text-align: center; color: #666;">No errors logged</div>'}</div>`;

        // Remove existing panel if any
        const existing = document.getElementById('error-monitor-panel');
        if (existing) existing.remove();

        document.body.appendChild(panel);
    }
}

// Initialize error monitor globally
window.errorMonitor = new ErrorMonitor({
    enableConsoleCapture: true,
    enableAjaxCapture: true,
    enableClickCapture: true,
    enableErrorCapture: true,
    logToServer: false, // Set to true when server endpoint is ready
    logToConsole: true
});

// Add keyboard shortcut to show error panel (Ctrl+Shift+E)
document.addEventListener('keydown', (e) => {
    if (e.ctrlKey && e.shiftKey && e.key === 'E') {
        window.errorMonitor.showErrorPanel();
    }
});

console.log('✅ Error Monitor initialized. Press Ctrl+Shift+E to view errors.');
