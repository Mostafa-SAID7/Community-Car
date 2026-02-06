/**
 * AJAX Wrapper Utility
 */
const Ajax = {
    get: async (url) => {
        const response = await fetch(url);
        return await response.json();
    },
    post: async (url, data) => {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        return await response.json();
    }
};

window.Ajax = Ajax;
