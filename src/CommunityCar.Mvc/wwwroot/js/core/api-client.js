/**
 * API Client - Centralized HTTP request handler
 * Eliminates duplicate AJAX code across the application
 */

class ApiClient {
    constructor(options = {}) {
        this.baseUrl = options.baseUrl || '';
        this.defaultHeaders = {
            'Content-Type': 'application/json',
            ...options.headers
        };
        this.timeout = options.timeout || 30000;
        this.interceptors = {
            request: [],
            response: [],
            error: []
        };
    }

    /**
     * Add request interceptor
     */
    addRequestInterceptor(fn) {
        this.interceptors.request.push(fn);
    }

    /**
     * Add response interceptor
     */
    addResponseInterceptor(fn) {
        this.interceptors.response.push(fn);
    }

    /**
     * Add error interceptor
     */
    addErrorInterceptor(fn) {
        this.interceptors.error.push(fn);
    }

    /**
     * Get anti-forgery token from page
     */
    getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : null;
    }

    /**
     * Build full URL
     */
    buildUrl(endpoint) {
        if (endpoint.startsWith('http')) {
            return endpoint;
        }
        return `${this.baseUrl}${endpoint}`;
    }

    /**
     * Execute request with interceptors
     */
    async request(method, endpoint, options = {}) {
        let config = {
            method,
            headers: { ...this.defaultHeaders, ...options.headers },
            ...options
        };

        // Add anti-forgery token for POST, PUT, DELETE
        if (['POST', 'PUT', 'DELETE'].includes(method.toUpperCase())) {
            const token = this.getAntiForgeryToken();
            if (token) {
                config.headers['RequestVerificationToken'] = token;
            }
        }

        // Run request interceptors
        for (const interceptor of this.interceptors.request) {
            config = await interceptor(config);
        }

        const url = this.buildUrl(endpoint);

        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), this.timeout);

            const response = await fetch(url, {
                ...config,
                signal: controller.signal
            });

            clearTimeout(timeoutId);

            // Run response interceptors
            let result = response;
            for (const interceptor of this.interceptors.response) {
                result = await interceptor(result);
            }

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            // Parse response based on content type
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }
            return await response.text();

        } catch (error) {
            // Run error interceptors
            for (const interceptor of this.interceptors.error) {
                await interceptor(error, config);
            }
            throw error;
        }
    }

    /**
     * GET request
     */
    async get(endpoint, params = {}, options = {}) {
        const queryString = new URLSearchParams(params).toString();
        const url = queryString ? `${endpoint}?${queryString}` : endpoint;
        return this.request('GET', url, options);
    }

    /**
     * POST request
     */
    async post(endpoint, data = {}, options = {}) {
        return this.request('POST', endpoint, {
            ...options,
            body: JSON.stringify(data)
        });
    }

    /**
     * PUT request
     */
    async put(endpoint, data = {}, options = {}) {
        return this.request('PUT', endpoint, {
            ...options,
            body: JSON.stringify(data)
        });
    }

    /**
     * DELETE request
     */
    async delete(endpoint, options = {}) {
        return this.request('DELETE', endpoint, options);
    }

    /**
     * PATCH request
     */
    async patch(endpoint, data = {}, options = {}) {
        return this.request('PATCH', endpoint, {
            ...options,
            body: JSON.stringify(data)
        });
    }

    /**
     * Upload file(s)
     */
    async upload(endpoint, formData, options = {}) {
        const headers = { ...options.headers };
        delete headers['Content-Type']; // Let browser set it with boundary

        return this.request('POST', endpoint, {
            ...options,
            headers,
            body: formData
        });
    }
}

// Create global instance
window.api = new ApiClient();

// Add default error interceptor
window.api.addErrorInterceptor((error, config) => {
    console.error('API Error:', error.message, config);
    
    // Show toast notification if available
    if (window.Toast) {
        window.Toast.show(error.message, 'error');
    }
});

// Add default response interceptor for logging
if (window.location.search.includes('debug=true')) {
    window.api.addResponseInterceptor((response) => {
        console.log('API Response:', response);
        return response;
    });
}
