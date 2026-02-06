/**
 * Reusable Helper Functions
 */
const Helpers = {
    formatDate: (date) => new Date(date).toLocaleDateString(),
    debounce: (func, wait) => {
        let timeout;
        return function (...args) {
            clearTimeout(timeout);
            timeout = setTimeout(() => func.apply(this, args), wait);
        };
    }
};

window.Helpers = Helpers;
