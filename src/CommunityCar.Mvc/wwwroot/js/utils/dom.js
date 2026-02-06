/**
 * DOM Utilities
 */
const DomUtils = {
    select: (selector) => document.querySelector(selector),
    selectAll: (selector) => document.querySelectorAll(selector),
    on: (element, event, handler) => {
        if (element) element.addEventListener(event, handler);
    }
};

window.DomUtils = DomUtils;
