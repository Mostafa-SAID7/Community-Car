/**
 * Developer Tools Panel
 * Advanced debugging panel with error monitoring, network inspection, and performance metrics
 */

class DevTools {
    constructor() {
        this.isVisible = false;
        this.activeTab = 'errors';
        this.networkRequests = [];
        this.performanceMetrics = [];
        
        this.init();
    }

    init() {
        this.captureNetworkRequests();
        this.capturePerformanceMetrics();
        this.setupKeyboardShortcuts();
    }

    captureNetworkRequests() {
        const self = this;
        
        // Intercept XMLHttpRequest
        const originalXHR = window.XMLHttpRequest;
        window.XMLHttpRequest = function() {
            const xhr = new originalXHR();
            const originalOpen = xhr.open;
            const originalSend = xhr.send;
            let requestData = {};

            xhr.open = function(method, url, ...args) {
                requestData = {
                    method,
                    url,
                    startTime: Date.now(),
                    type: 'XHR'
                };
                return originalOpen.apply(this, [method, url, ...args]);
            };

            xhr.send = function(body) {
                requestData.body = body;
                
                xhr.addEventListener('load', function() {
                    requestData.endTime = Date.now();
                    requestData.duration = requestData.endTime - requestData.startTime;
                    requestData.status = xhr.status;
                    requestData.statusText = xhr.statusText;
                    requestData.response = xhr.responseText?.substring(0, 1000);
                    
                    self.networkRequests.push(requestData);
                    if (self.networkRequests.length > 50) {
                        self.networkRequests.shift();
                    }
                });

                return originalSend.apply(this, arguments);
            };

            return xhr;
        };

        // Intercept Fetch
        const originalFetch = window.fetch;
        window.fetch = function(...args) {
            const startTime = Date.now();
            const url = typeof args[0] === 'string' ? args[0] : args[0].url;
            const method = args[1]?.method || 'GET';

            return originalFetch.apply(this, args)
                .then(response => {
                    const endTime = Date.now();
                    self.networkRequests.push({
                        method,
                        url,
                        startTime,
                        endTime,
                        duration: endTime - startTime,
                        status: response.status,
                        statusText: response.statusText,
                        type: 'Fetch'
                    });
                    
                    if (self.networkRequests.length > 50) {
                        self.networkRequests.shift();
                    }
                    
                    return response;
                });
        };
    }

    capturePerformanceMetrics() {
        if (window.performance) {
            // Use modern Navigation Timing API Level 2
            const perfData = window.performance.getEntriesByType('navigation')[0];
            if (perfData) {
                this.performanceMetrics = {
                    pageLoadTime: Math.round(perfData.loadEventEnd - perfData.fetchStart),
                    domReadyTime: Math.round(perfData.domContentLoadedEventEnd - perfData.fetchStart),
                    responseTime: Math.round(perfData.responseEnd - perfData.requestStart),
                    renderTime: Math.round(perfData.domComplete - perfData.domLoading)
                };
            } else {
                // Fallback to basic timing
                this.performanceMetrics = {
                    pageLoadTime: 0,
                    domReadyTime: 0,
                    responseTime: 0,
                    renderTime: 0
                };
            }
        }
    }

    setupKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Ctrl+Shift+D to toggle dev tools
            if (e.ctrlKey && e.shiftKey && e.key === 'D') {
                e.preventDefault();
                this.toggle();
            }
        });
    }

    toggle() {
        if (this.isVisible) {
            this.hide();
        } else {
            this.show();
        }
    }

    show() {
        this.isVisible = true;
        this.render();
    }

    hide() {
        this.isVisible = false;
        const panel = document.getElementById('dev-tools-panel');
        if (panel) {
            panel.remove();
        }
    }

    switchTab(tab) {
        this.activeTab = tab;
        this.render();
    }

    render() {
        // Remove existing panel
        const existing = document.getElementById('dev-tools-panel');
        if (existing) {
            existing.remove();
        }

        const panel = document.createElement('div');
        panel.id = 'dev-tools-panel';
        panel.innerHTML = this.getHTML();
        document.body.appendChild(panel);

        // Attach event listeners
        this.attachEventListeners();
    }

    getHTML() {
        return `
            <style>
                #dev-tools-panel {
                    position: fixed;
                    bottom: 0;
                    left: 0;
                    right: 0;
                    height: 400px;
                    background: #1e1e1e;
                    color: #d4d4d4;
                    font-family: 'Consolas', 'Monaco', monospace;
                    font-size: 12px;
                    z-index: 999999;
                    display: flex;
                    flex-direction: column;
                    box-shadow: 0 -2px 10px rgba(0,0,0,0.5);
                }
                .dev-tools-header {
                    background: #2d2d30;
                    padding: 8px 12px;
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    border-bottom: 1px solid #3e3e42;
                }
                .dev-tools-tabs {
                    display: flex;
                    gap: 4px;
                }
                .dev-tools-tab {
                    padding: 6px 12px;
                    background: transparent;
                    border: none;
                    color: #cccccc;
                    cursor: pointer;
                    border-radius: 4px;
                    transition: background 0.2s;
                }
                .dev-tools-tab:hover {
                    background: #3e3e42;
                }
                .dev-tools-tab.active {
                    background: #007acc;
                    color: white;
                }
                .dev-tools-content {
                    flex: 1;
                    overflow-y: auto;
                    padding: 12px;
                }
                .dev-tools-close {
                    background: #dc3545;
                    color: white;
                    border: none;
                    padding: 4px 12px;
                    border-radius: 4px;
                    cursor: pointer;
                }
                .error-item, .network-item {
                    background: #252526;
                    padding: 10px;
                    margin-bottom: 8px;
                    border-left: 3px solid #007acc;
                    border-radius: 4px;
                }
                .error-item.error {
                    border-left-color: #dc3545;
                }
                .error-item.warning {
                    border-left-color: #ffc107;
                }
                .error-type {
                    color: #4ec9b0;
                    font-weight: bold;
                    margin-bottom: 4px;
                }
                .error-message {
                    color: #ce9178;
                    margin-bottom: 4px;
                }
                .error-meta {
                    color: #858585;
                    font-size: 11px;
                }
                .network-method {
                    display: inline-block;
                    padding: 2px 6px;
                    border-radius: 3px;
                    font-weight: bold;
                    margin-right: 8px;
                }
                .network-method.GET { background: #28a745; color: white; }
                .network-method.POST { background: #007bff; color: white; }
                .network-method.PUT { background: #ffc107; color: black; }
                .network-method.DELETE { background: #dc3545; color: white; }
                .status-code {
                    display: inline-block;
                    padding: 2px 6px;
                    border-radius: 3px;
                    font-weight: bold;
                }
                .status-code.success { background: #28a745; color: white; }
                .status-code.error { background: #dc3545; color: white; }
                .metric-grid {
                    display: grid;
                    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
                    gap: 12px;
                }
                .metric-card {
                    background: #252526;
                    padding: 12px;
                    border-radius: 4px;
                    border-left: 3px solid #007acc;
                }
                .metric-label {
                    color: #858585;
                    font-size: 11px;
                    margin-bottom: 4px;
                }
                .metric-value {
                    color: #4ec9b0;
                    font-size: 20px;
                    font-weight: bold;
                }
            </style>
            <div class="dev-tools-header">
                <div class="dev-tools-tabs">
                    <button class="dev-tools-tab ${this.activeTab === 'errors' ? 'active' : ''}" onclick="devTools.switchTab('errors')">
                        🔴 Errors (${window.errorMonitor?.errors.length || 0})
                    </button>
                    <button class="dev-tools-tab ${this.activeTab === 'network' ? 'active' : ''}" onclick="devTools.switchTab('network')">
                        🌐 Network (${this.networkRequests.length})
                    </button>
                    <button class="dev-tools-tab ${this.activeTab === 'performance' ? 'active' : ''}" onclick="devTools.switchTab('performance')">
                        ⚡ Performance
                    </button>
                    <button class="dev-tools-tab ${this.activeTab === 'console' ? 'active' : ''}" onclick="devTools.switchTab('console')">
                        💻 Console
                    </button>
                </div>
                <div>
                    <button class="dev-tools-close" onclick="devTools.hide()">✕ Close</button>
                </div>
            </div>
            <div class="dev-tools-content">
                ${this.getTabContent()}
            </div>
        `;
    }

    getTabContent() {
        switch (this.activeTab) {
            case 'errors':
                return this.getErrorsContent();
            case 'network':
                return this.getNetworkContent();
            case 'performance':
                return this.getPerformanceContent();
            case 'console':
                return this.getConsoleContent();
            default:
                return '<div>Unknown tab</div>';
        }
    }

    getErrorsContent() {
        const errors = window.errorMonitor?.errors || [];
        
        if (errors.length === 0) {
            return '<div style="text-align: center; padding: 40px; color: #858585;">✅ No errors logged</div>';
        }

        return `
            <div style="margin-bottom: 12px;">
                <button onclick="window.errorMonitor.clearErrors(); devTools.render();" style="padding: 6px 12px; background: #dc3545; color: white; border: none; border-radius: 4px; cursor: pointer;">Clear All</button>
                <button onclick="window.errorMonitor.exportErrors();" style="padding: 6px 12px; background: #007acc; color: white; border: none; border-radius: 4px; cursor: pointer; margin-left: 8px;">Export JSON</button>
            </div>
            ${errors.slice().reverse().map(error => `
                <div class="error-item ${error.type?.toLowerCase().includes('error') ? 'error' : 'warning'}">
                    <div class="error-type">${error.type}</div>
                    <div class="error-message">${error.message}</div>
                    <div class="error-meta">
                        ${error.filename ? `📄 ${error.filename}:${error.line}:${error.column}` : ''}
                        ${error.url ? `🔗 ${error.url}` : ''}
                        <br>🕐 ${error.timestamp}
                    </div>
                    ${error.stack ? `<details style="margin-top: 8px;"><summary style="cursor: pointer; color: #858585;">Stack Trace</summary><pre style="margin-top: 8px; color: #ce9178; font-size: 10px;">${error.stack}</pre></details>` : ''}
                </div>
            `).join('')}
        `;
    }

    getNetworkContent() {
        if (this.networkRequests.length === 0) {
            return '<div style="text-align: center; padding: 40px; color: #858585;">No network requests captured yet</div>';
        }

        return `
            <div style="margin-bottom: 12px;">
                <button onclick="devTools.networkRequests = []; devTools.render();" style="padding: 6px 12px; background: #dc3545; color: white; border: none; border-radius: 4px; cursor: pointer;">Clear</button>
            </div>
            ${this.networkRequests.slice().reverse().map(req => `
                <div class="network-item">
                    <div>
                        <span class="network-method ${req.method}">${req.method}</span>
                        <span class="status-code ${req.status < 400 ? 'success' : 'error'}">${req.status}</span>
                        <span style="color: #d4d4d4; margin-left: 8px;">${req.url}</span>
                    </div>
                    <div class="error-meta" style="margin-top: 4px;">
                        ⏱️ ${req.duration}ms | 📦 ${req.type}
                    </div>
                </div>
            `).join('')}
        `;
    }

    getPerformanceContent() {
        return `
            <div class="metric-grid">
                <div class="metric-card">
                    <div class="metric-label">Page Load Time</div>
                    <div class="metric-value">${this.performanceMetrics.pageLoadTime || 0}ms</div>
                </div>
                <div class="metric-card">
                    <div class="metric-label">DOM Ready Time</div>
                    <div class="metric-value">${this.performanceMetrics.domReadyTime || 0}ms</div>
                </div>
                <div class="metric-card">
                    <div class="metric-label">Response Time</div>
                    <div class="metric-value">${this.performanceMetrics.responseTime || 0}ms</div>
                </div>
                <div class="metric-card">
                    <div class="metric-label">Render Time</div>
                    <div class="metric-value">${this.performanceMetrics.renderTime || 0}ms</div>
                </div>
            </div>
            <div style="margin-top: 20px;">
                <h3 style="color: #4ec9b0; margin-bottom: 12px;">Memory Usage</h3>
                <div class="metric-card">
                    <div class="metric-label">JS Heap Size</div>
                    <div class="metric-value">${window.performance.memory ? (window.performance.memory.usedJSHeapSize / 1048576).toFixed(2) + ' MB' : 'N/A'}</div>
                </div>
            </div>
        `;
    }

    getConsoleContent() {
        return `
            <div style="padding: 20px; text-align: center; color: #858585;">
                <p>Open browser DevTools (F12) for full console access</p>
                <p style="margin-top: 12px;">Keyboard Shortcuts:</p>
                <ul style="list-style: none; padding: 0; margin-top: 12px;">
                    <li>Ctrl+Shift+D - Toggle Dev Tools Panel</li>
                    <li>Ctrl+Shift+E - Show Error Monitor</li>
                    <li>F12 - Open Browser DevTools</li>
                </ul>
            </div>
        `;
    }

    attachEventListeners() {
        // Event listeners are attached via onclick in HTML
    }
}

// Initialize DevTools
window.devTools = new DevTools();

console.log('🛠️ Dev Tools initialized. Press Ctrl+Shift+D to open panel.');
