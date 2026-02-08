/**
 * Storage Manager - Comprehensive browser storage utilities
 * Supports: Session Storage, Cookies, Cache Storage, Local Storage
 * Note: Extension Storage, Shared Storage, Storage Buckets require specific browser contexts
 */
(function(window) {
    'use strict';

    // ============================================================================
    // SESSION STORAGE
    // ============================================================================
    const SessionStorage = {
        /**
         * Set item in session storage
         * @param {string} key - Storage key
         * @param {any} value - Value to store (will be JSON stringified)
         */
        set(key, value) {
            try {
                const serialized = JSON.stringify(value);
                sessionStorage.setItem(key, serialized);
                return true;
            } catch (error) {
                console.error('SessionStorage.set error:', error);
                return false;
            }
        },

        /**
         * Get item from session storage
         * @param {string} key - Storage key
         * @param {any} defaultValue - Default value if key doesn't exist
         * @returns {any} Parsed value or default
         */
        get(key, defaultValue = null) {
            try {
                const item = sessionStorage.getItem(key);
                return item ? JSON.parse(item) : defaultValue;
            } catch (error) {
                console.error('SessionStorage.get error:', error);
                return defaultValue;
            }
        },

        /**
         * Remove item from session storage
         * @param {string} key - Storage key
         */
        remove(key) {
            try {
                sessionStorage.removeItem(key);
                return true;
            } catch (error) {
                console.error('SessionStorage.remove error:', error);
                return false;
            }
        },

        /**
         * Clear all session storage
         */
        clear() {
            try {
                sessionStorage.clear();
                return true;
            } catch (error) {
                console.error('SessionStorage.clear error:', error);
                return false;
            }
        },

        /**
         * Check if key exists
         * @param {string} key - Storage key
         * @returns {boolean}
         */
        has(key) {
            return sessionStorage.getItem(key) !== null;
        },

        /**
         * Get all keys
         * @returns {string[]}
         */
        keys() {
            return Object.keys(sessionStorage);
        }
    };

    // ============================================================================
    // LOCAL STORAGE (Persistent)
    // ============================================================================
    const LocalStorage = {
        /**
         * Set item in local storage with optional expiry
         * @param {string} key - Storage key
         * @param {any} value - Value to store
         * @param {number} ttl - Time to live in milliseconds (optional)
         */
        set(key, value, ttl = null) {
            try {
                const item = {
                    value: value,
                    timestamp: Date.now(),
                    expiry: ttl ? Date.now() + ttl : null
                };
                localStorage.setItem(key, JSON.stringify(item));
                return true;
            } catch (error) {
                console.error('LocalStorage.set error:', error);
                return false;
            }
        },

        /**
         * Get item from local storage (checks expiry)
         * @param {string} key - Storage key
         * @param {any} defaultValue - Default value if key doesn't exist or expired
         * @returns {any}
         */
        get(key, defaultValue = null) {
            try {
                const itemStr = localStorage.getItem(key);
                if (!itemStr) return defaultValue;

                const item = JSON.parse(itemStr);
                
                // Check expiry
                if (item.expiry && Date.now() > item.expiry) {
                    localStorage.removeItem(key);
                    return defaultValue;
                }

                return item.value;
            } catch (error) {
                console.error('LocalStorage.get error:', error);
                return defaultValue;
            }
        },

        /**
         * Remove item from local storage
         * @param {string} key - Storage key
         */
        remove(key) {
            try {
                localStorage.removeItem(key);
                return true;
            } catch (error) {
                console.error('LocalStorage.remove error:', error);
                return false;
            }
        },

        /**
         * Clear all local storage
         */
        clear() {
            try {
                localStorage.clear();
                return true;
            } catch (error) {
                console.error('LocalStorage.clear error:', error);
                return false;
            }
        },

        /**
         * Check if key exists and not expired
         * @param {string} key - Storage key
         * @returns {boolean}
         */
        has(key) {
            const value = this.get(key, undefined);
            return value !== undefined;
        },

        /**
         * Get all keys
         * @returns {string[]}
         */
        keys() {
            return Object.keys(localStorage);
        }
    };

    // ============================================================================
    // COOKIES
    // ============================================================================
    const Cookies = {
        /**
         * Set cookie
         * @param {string} name - Cookie name
         * @param {string} value - Cookie value
         * @param {object} options - Cookie options (expires, path, domain, secure, sameSite)
         */
        set(name, value, options = {}) {
            try {
                let cookieString = `${encodeURIComponent(name)}=${encodeURIComponent(value)}`;

                if (options.expires) {
                    if (typeof options.expires === 'number') {
                        const date = new Date();
                        date.setTime(date.getTime() + options.expires * 24 * 60 * 60 * 1000);
                        cookieString += `; expires=${date.toUTCString()}`;
                    } else if (options.expires instanceof Date) {
                        cookieString += `; expires=${options.expires.toUTCString()}`;
                    }
                }

                if (options.path) {
                    cookieString += `; path=${options.path}`;
                } else {
                    cookieString += '; path=/';
                }

                if (options.domain) {
                    cookieString += `; domain=${options.domain}`;
                }

                if (options.secure) {
                    cookieString += '; secure';
                }

                if (options.sameSite) {
                    cookieString += `; samesite=${options.sameSite}`;
                }

                document.cookie = cookieString;
                return true;
            } catch (error) {
                console.error('Cookies.set error:', error);
                return false;
            }
        },

        /**
         * Get cookie value
         * @param {string} name - Cookie name
         * @returns {string|null}
         */
        get(name) {
            try {
                const nameEQ = encodeURIComponent(name) + '=';
                const cookies = document.cookie.split(';');
                
                for (let i = 0; i < cookies.length; i++) {
                    let cookie = cookies[i].trim();
                    if (cookie.indexOf(nameEQ) === 0) {
                        return decodeURIComponent(cookie.substring(nameEQ.length));
                    }
                }
                return null;
            } catch (error) {
                console.error('Cookies.get error:', error);
                return null;
            }
        },

        /**
         * Remove cookie
         * @param {string} name - Cookie name
         * @param {object} options - Cookie options (path, domain)
         */
        remove(name, options = {}) {
            this.set(name, '', { ...options, expires: -1 });
        },

        /**
         * Check if cookie exists
         * @param {string} name - Cookie name
         * @returns {boolean}
         */
        has(name) {
            return this.get(name) !== null;
        },

        /**
         * Get all cookies as object
         * @returns {object}
         */
        getAll() {
            const cookies = {};
            const cookieArray = document.cookie.split(';');
            
            cookieArray.forEach(cookie => {
                const [name, value] = cookie.trim().split('=');
                if (name) {
                    cookies[decodeURIComponent(name)] = decodeURIComponent(value || '');
                }
            });
            
            return cookies;
        }
    };

    // ============================================================================
    // CACHE STORAGE (Service Worker Cache API)
    // ============================================================================
    const CacheStorage = {
        /**
         * Open or create a cache
         * @param {string} cacheName - Cache name
         * @returns {Promise<Cache>}
         */
        async open(cacheName) {
            if (!('caches' in window)) {
                throw new Error('Cache API not supported');
            }
            return await caches.open(cacheName);
        },

        /**
         * Add item to cache
         * @param {string} cacheName - Cache name
         * @param {string|Request} request - Request URL or Request object
         * @param {Response} response - Response to cache
         */
        async add(cacheName, request, response) {
            try {
                const cache = await this.open(cacheName);
                await cache.put(request, response);
                return true;
            } catch (error) {
                console.error('CacheStorage.add error:', error);
                return false;
            }
        },

        /**
         * Get item from cache
         * @param {string} cacheName - Cache name
         * @param {string|Request} request - Request URL or Request object
         * @returns {Promise<Response|undefined>}
         */
        async get(cacheName, request) {
            try {
                const cache = await this.open(cacheName);
                return await cache.match(request);
            } catch (error) {
                console.error('CacheStorage.get error:', error);
                return undefined;
            }
        },

        /**
         * Remove item from cache
         * @param {string} cacheName - Cache name
         * @param {string|Request} request - Request URL or Request object
         */
        async remove(cacheName, request) {
            try {
                const cache = await this.open(cacheName);
                return await cache.delete(request);
            } catch (error) {
                console.error('CacheStorage.remove error:', error);
                return false;
            }
        },

        /**
         * Delete entire cache
         * @param {string} cacheName - Cache name
         */
        async delete(cacheName) {
            try {
                return await caches.delete(cacheName);
            } catch (error) {
                console.error('CacheStorage.delete error:', error);
                return false;
            }
        },

        /**
         * Get all cache names
         * @returns {Promise<string[]>}
         */
        async keys() {
            try {
                return await caches.keys();
            } catch (error) {
                console.error('CacheStorage.keys error:', error);
                return [];
            }
        },

        /**
         * Check if cache exists
         * @param {string} cacheName - Cache name
         * @returns {Promise<boolean>}
         */
        async has(cacheName) {
            try {
                const cacheNames = await caches.keys();
                return cacheNames.includes(cacheName);
            } catch (error) {
                console.error('CacheStorage.has error:', error);
                return false;
            }
        }
    };

    // ============================================================================
    // INDEXED DB (For large structured data)
    // ============================================================================
    const IndexedDB = {
        /**
         * Open IndexedDB database
         * @param {string} dbName - Database name
         * @param {number} version - Database version
         * @param {function} upgradeCallback - Callback for database upgrade
         * @returns {Promise<IDBDatabase>}
         */
        async open(dbName, version = 1, upgradeCallback = null) {
            return new Promise((resolve, reject) => {
                const request = indexedDB.open(dbName, version);

                request.onerror = () => reject(request.error);
                request.onsuccess = () => resolve(request.result);
                
                if (upgradeCallback) {
                    request.onupgradeneeded = (event) => {
                        upgradeCallback(event.target.result, event);
                    };
                }
            });
        },

        /**
         * Add or update item in object store
         * @param {string} dbName - Database name
         * @param {string} storeName - Object store name
         * @param {any} data - Data to store
         * @param {any} key - Key (optional if keyPath is set)
         */
        async put(dbName, storeName, data, key = null) {
            try {
                const db = await this.open(dbName);
                const transaction = db.transaction([storeName], 'readwrite');
                const store = transaction.objectStore(storeName);
                
                const request = key ? store.put(data, key) : store.put(data);
                
                return new Promise((resolve, reject) => {
                    request.onsuccess = () => resolve(request.result);
                    request.onerror = () => reject(request.error);
                });
            } catch (error) {
                console.error('IndexedDB.put error:', error);
                throw error;
            }
        },

        /**
         * Get item from object store
         * @param {string} dbName - Database name
         * @param {string} storeName - Object store name
         * @param {any} key - Key to retrieve
         */
        async get(dbName, storeName, key) {
            try {
                const db = await this.open(dbName);
                const transaction = db.transaction([storeName], 'readonly');
                const store = transaction.objectStore(storeName);
                const request = store.get(key);
                
                return new Promise((resolve, reject) => {
                    request.onsuccess = () => resolve(request.result);
                    request.onerror = () => reject(request.error);
                });
            } catch (error) {
                console.error('IndexedDB.get error:', error);
                return undefined;
            }
        },

        /**
         * Delete item from object store
         * @param {string} dbName - Database name
         * @param {string} storeName - Object store name
         * @param {any} key - Key to delete
         */
        async delete(dbName, storeName, key) {
            try {
                const db = await this.open(dbName);
                const transaction = db.transaction([storeName], 'readwrite');
                const store = transaction.objectStore(storeName);
                const request = store.delete(key);
                
                return new Promise((resolve, reject) => {
                    request.onsuccess = () => resolve(true);
                    request.onerror = () => reject(request.error);
                });
            } catch (error) {
                console.error('IndexedDB.delete error:', error);
                return false;
            }
        },

        /**
         * Get all items from object store
         * @param {string} dbName - Database name
         * @param {string} storeName - Object store name
         */
        async getAll(dbName, storeName) {
            try {
                const db = await this.open(dbName);
                const transaction = db.transaction([storeName], 'readonly');
                const store = transaction.objectStore(storeName);
                const request = store.getAll();
                
                return new Promise((resolve, reject) => {
                    request.onsuccess = () => resolve(request.result);
                    request.onerror = () => reject(request.error);
                });
            } catch (error) {
                console.error('IndexedDB.getAll error:', error);
                return [];
            }
        }
    };

    // ============================================================================
    // BACK/FORWARD CACHE (BFCache) Utilities
    // ============================================================================
    const BFCache = {
        /**
         * Check if page was restored from BFCache
         * @returns {boolean}
         */
        isRestored() {
            return performance.getEntriesByType('navigation')[0]?.type === 'back_forward';
        },

        /**
         * Register callback for page show event (BFCache restore)
         * @param {function} callback - Callback function
         */
        onRestore(callback) {
            window.addEventListener('pageshow', (event) => {
                if (event.persisted) {
                    callback(event);
                }
            });
        },

        /**
         * Register callback for page hide event (BFCache store)
         * @param {function} callback - Callback function
         */
        onStore(callback) {
            window.addEventListener('pagehide', (event) => {
                if (event.persisted) {
                    callback(event);
                }
            });
        },

        /**
         * Prevent page from being cached in BFCache
         */
        preventCaching() {
            // Add unload event listener (prevents BFCache)
            window.addEventListener('unload', () => {});
        }
    };

    // ============================================================================
    // STORAGE MANAGER (Unified Interface)
    // ============================================================================
    const StorageManager = {
        session: SessionStorage,
        local: LocalStorage,
        cookies: Cookies,
        cache: CacheStorage,
        indexedDB: IndexedDB,
        bfcache: BFCache,

        /**
         * Get storage quota information
         * @returns {Promise<object>}
         */
        async getQuota() {
            if ('storage' in navigator && 'estimate' in navigator.storage) {
                const estimate = await navigator.storage.estimate();
                return {
                    usage: estimate.usage,
                    quota: estimate.quota,
                    usagePercent: ((estimate.usage / estimate.quota) * 100).toFixed(2),
                    available: estimate.quota - estimate.usage
                };
            }
            return null;
        },

        /**
         * Request persistent storage
         * @returns {Promise<boolean>}
         */
        async requestPersistent() {
            if ('storage' in navigator && 'persist' in navigator.storage) {
                return await navigator.storage.persist();
            }
            return false;
        },

        /**
         * Check if storage is persistent
         * @returns {Promise<boolean>}
         */
        async isPersistent() {
            if ('storage' in navigator && 'persisted' in navigator.storage) {
                return await navigator.storage.persisted();
            }
            return false;
        },

        /**
         * Clear all storage types
         */
        async clearAll() {
            try {
                // Clear session storage
                SessionStorage.clear();
                
                // Clear local storage
                LocalStorage.clear();
                
                // Clear all caches
                const cacheNames = await CacheStorage.keys();
                await Promise.all(cacheNames.map(name => CacheStorage.delete(name)));
                
                console.log('All storage cleared successfully');
                return true;
            } catch (error) {
                console.error('StorageManager.clearAll error:', error);
                return false;
            }
        }
    };

    // Expose to global scope
    window.StorageManager = StorageManager;

    // Also expose individual storage types for convenience
    window.SessionStorage = SessionStorage;
    window.LocalStorage = LocalStorage;
    window.Cookies = Cookies;
    window.CacheStorage = CacheStorage;
    window.IndexedDB = IndexedDB;
    window.BFCache = BFCache;

})(window);
