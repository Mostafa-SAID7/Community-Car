/**
 * DOM Utilities - Shared DOM manipulation functions
 * Eliminates duplicate DOM code across the application
 */

const DomUtils = {
    /**
     * Query selector with optional parent
     */
    $(selector, parent = document) {
        return parent.querySelector(selector);
    },

    /**
     * Query selector all with optional parent
     */
    $$$(selector, parent = document) {
        return Array.from(parent.querySelectorAll(selector));
    },

    /**
     * Create element with attributes and children
     */
    createElement(tag, attributes = {}, children = []) {
        const element = document.createElement(tag);
        
        Object.entries(attributes).forEach(([key, value]) => {
            if (key === 'className') {
                element.className = value;
            } else if (key === 'dataset') {
                Object.entries(value).forEach(([dataKey, dataValue]) => {
                    element.dataset[dataKey] = dataValue;
                });
            } else if (key.startsWith('on')) {
                const event = key.substring(2).toLowerCase();
                element.addEventListener(event, value);
            } else {
                element.setAttribute(key, value);
            }
        });

        children.forEach(child => {
            if (typeof child === 'string') {
                element.appendChild(document.createTextNode(child));
            } else if (child instanceof Node) {
                element.appendChild(child);
            }
        });

        return element;
    },

    /**
     * Add class(es) to element
     */
    addClass(element, ...classes) {
        if (!element) return;
        element.classList.add(...classes);
    },

    /**
     * Remove class(es) from element
     */
    removeClass(element, ...classes) {
        if (!element) return;
        element.classList.remove(...classes);
    },

    /**
     * Toggle class on element
     */
    toggleClass(element, className) {
        if (!element) return;
        element.classList.toggle(className);
    },

    /**
     * Check if element has class
     */
    hasClass(element, className) {
        if (!element) return false;
        return element.classList.contains(className);
    },

    /**
     * Show element
     */
    show(element, display = 'block') {
        if (!element) return;
        element.style.display = display;
    },

    /**
     * Hide element
     */
    hide(element) {
        if (!element) return;
        element.style.display = 'none';
    },

    /**
     * Toggle element visibility
     */
    toggle(element, display = 'block') {
        if (!element) return;
        element.style.display = element.style.display === 'none' ? display : 'none';
    },

    /**
     * Remove element from DOM
     */
    remove(element) {
        if (!element) return;
        element.remove();
    },

    /**
     * Empty element (remove all children)
     */
    empty(element) {
        if (!element) return;
        while (element.firstChild) {
            element.removeChild(element.firstChild);
        }
    },

    /**
     * Get/Set element attribute
     */
    attr(element, name, value) {
        if (!element) return null;
        if (value === undefined) {
            return element.getAttribute(name);
        }
        element.setAttribute(name, value);
    },

    /**
     * Remove attribute
     */
    removeAttr(element, name) {
        if (!element) return;
        element.removeAttribute(name);
    },

    /**
     * Get/Set element data attribute
     */
    data(element, key, value) {
        if (!element) return null;
        if (value === undefined) {
            return element.dataset[key];
        }
        element.dataset[key] = value;
    },

    /**
     * Get/Set element HTML
     */
    html(element, content) {
        if (!element) return null;
        if (content === undefined) {
            return element.innerHTML;
        }
        element.innerHTML = content;
    },

    /**
     * Get/Set element text
     */
    text(element, content) {
        if (!element) return null;
        if (content === undefined) {
            return element.textContent;
        }
        element.textContent = content;
    },

    /**
     * Get/Set element value
     */
    val(element, value) {
        if (!element) return null;
        if (value === undefined) {
            return element.value;
        }
        element.value = value;
    },

    /**
     * Add event listener
     */
    on(element, event, handler, options = {}) {
        if (!element) return;
        element.addEventListener(event, handler, options);
    },

    /**
     * Remove event listener
     */
    off(element, event, handler) {
        if (!element) return;
        element.removeEventListener(event, handler);
    },

    /**
     * Trigger custom event
     */
    trigger(element, eventName, detail = {}) {
        if (!element) return;
        const event = new CustomEvent(eventName, { detail, bubbles: true });
        element.dispatchEvent(event);
    },

    /**
     * Delegate event listener
     */
    delegate(parent, selector, event, handler) {
        if (!parent) return;
        parent.addEventListener(event, (e) => {
            const target = e.target.closest(selector);
            if (target) {
                handler.call(target, e);
            }
        });
    },

    /**
     * Get element offset
     */
    offset(element) {
        if (!element) return { top: 0, left: 0 };
        const rect = element.getBoundingClientRect();
        return {
            top: rect.top + window.pageYOffset,
            left: rect.left + window.pageXOffset
        };
    },

    /**
     * Get element position
     */
    position(element) {
        if (!element) return { top: 0, left: 0 };
        return {
            top: element.offsetTop,
            left: element.offsetLeft
        };
    },

    /**
     * Get element dimensions
     */
    dimensions(element) {
        if (!element) return { width: 0, height: 0 };
        return {
            width: element.offsetWidth,
            height: element.offsetHeight
        };
    },

    /**
     * Scroll to element
     */
    scrollTo(element, options = {}) {
        if (!element) return;
        element.scrollIntoView({
            behavior: options.smooth ? 'smooth' : 'auto',
            block: options.block || 'start',
            inline: options.inline || 'nearest'
        });
    },

    /**
     * Check if element is in viewport
     */
    isInViewport(element) {
        if (!element) return false;
        const rect = element.getBoundingClientRect();
        return (
            rect.top >= 0 &&
            rect.left >= 0 &&
            rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
            rect.right <= (window.innerWidth || document.documentElement.clientWidth)
        );
    },

    /**
     * Get closest parent matching selector
     */
    closest(element, selector) {
        if (!element) return null;
        return element.closest(selector);
    },

    /**
     * Get siblings
     */
    siblings(element) {
        if (!element || !element.parentNode) return [];
        return Array.from(element.parentNode.children).filter(child => child !== element);
    },

    /**
     * Get next sibling
     */
    next(element) {
        if (!element) return null;
        return element.nextElementSibling;
    },

    /**
     * Get previous sibling
     */
    prev(element) {
        if (!element) return null;
        return element.previousElementSibling;
    },

    /**
     * Get parent
     */
    parent(element) {
        if (!element) return null;
        return element.parentElement;
    },

    /**
     * Get children
     */
    children(element) {
        if (!element) return [];
        return Array.from(element.children);
    },

    /**
     * Append child
     */
    append(parent, child) {
        if (!parent || !child) return;
        if (typeof child === 'string') {
            parent.insertAdjacentHTML('beforeend', child);
        } else {
            parent.appendChild(child);
        }
    },

    /**
     * Prepend child
     */
    prepend(parent, child) {
        if (!parent || !child) return;
        if (typeof child === 'string') {
            parent.insertAdjacentHTML('afterbegin', child);
        } else {
            parent.insertBefore(child, parent.firstChild);
        }
    },

    /**
     * Insert before
     */
    before(element, newElement) {
        if (!element || !newElement || !element.parentNode) return;
        element.parentNode.insertBefore(newElement, element);
    },

    /**
     * Insert after
     */
    after(element, newElement) {
        if (!element || !newElement || !element.parentNode) return;
        element.parentNode.insertBefore(newElement, element.nextSibling);
    },

    /**
     * Replace element
     */
    replace(oldElement, newElement) {
        if (!oldElement || !newElement || !oldElement.parentNode) return;
        oldElement.parentNode.replaceChild(newElement, oldElement);
    },

    /**
     * Clone element
     */
    clone(element, deep = true) {
        if (!element) return null;
        return element.cloneNode(deep);
    },

    /**
     * Serialize form data
     */
    serializeForm(form) {
        if (!form) return {};
        const formData = new FormData(form);
        const data = {};
        for (const [key, value] of formData.entries()) {
            if (data[key]) {
                if (!Array.isArray(data[key])) {
                    data[key] = [data[key]];
                }
                data[key].push(value);
            } else {
                data[key] = value;
            }
        }
        return data;
    },

    /**
     * Ready - Execute when DOM is ready
     */
    ready(fn) {
        if (document.readyState !== 'loading') {
            fn();
        } else {
            document.addEventListener('DOMContentLoaded', fn);
        }
    }
};

// Export to window
window.DomUtils = DomUtils;
window.dom = DomUtils; // Shorter alias
