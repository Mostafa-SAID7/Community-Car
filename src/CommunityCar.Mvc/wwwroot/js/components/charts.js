/**
 * Charting Component Logic using Chart.js
 */
const Charts = {
    create: (canvasId, type, data, options = {}) => {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return null;

        return new Chart(ctx, {
            type: type,
            data: data,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                ...options
            }
        });
    }
};

window.Charts = Charts;
