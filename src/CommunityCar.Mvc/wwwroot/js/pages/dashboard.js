/**
 * Dashboard Page Logic
 */
const DashboardPage = {
    activityChart: null,
    currentPeriod: 'week',

    init: () => {
        console.log("Dashboard page initialized");
        DashboardPage.initActivityPeriodButtons();
    },

    initActivityPeriodButtons: () => {
        const periodButtons = document.querySelectorAll('.dashboard-btn-group-item[data-period]');
        
        periodButtons.forEach(button => {
            button.addEventListener('click', async (e) => {
                e.preventDefault();
                const period = button.getAttribute('data-period');
                
                // Update active state
                periodButtons.forEach(btn => btn.classList.remove('active'));
                button.classList.add('active');
                
                // Fetch and update chart data
                await DashboardPage.updateActivityChart(period);
            });
        });
    },

    updateActivityChart: async (period) => {
        try {
            // Show loading state
            const chartCanvas = document.getElementById('activityChart');
            if (!chartCanvas) {
                console.error('Activity chart canvas not found');
                return;
            }

            // Get current culture from URL
            const pathParts = window.location.pathname.split('/');
            const culture = pathParts[1] || 'en';

            // Fetch new data
            const response = await fetch(`/${culture}/Dashboard/GetActivityData?period=${period}`);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const result = await response.json();
            
            if (!result.success) {
                throw new Error(result.message || 'Failed to fetch activity data');
            }

            // Update chart
            if (DashboardPage.activityChart) {
                DashboardPage.activityChart.data.labels = result.labels;
                DashboardPage.activityChart.data.datasets[0].data = result.data;
                DashboardPage.activityChart.update('active');
            } else {
                console.warn('Activity chart instance not found, chart may not be initialized yet');
            }

            DashboardPage.currentPeriod = period;
            console.log(`Activity chart updated for period: ${period}`);
            
        } catch (error) {
            console.error('Error updating activity chart:', error);
            
            // Show error notification if available
            if (typeof showNotification === 'function') {
                showNotification('Error loading activity data', 'error');
            }
        }
    },

    setActivityChartInstance: (chartInstance) => {
        DashboardPage.activityChart = chartInstance;
    }
};

if (document.body.classList.contains('dashboard-page')) {
    document.addEventListener('DOMContentLoaded', DashboardPage.init);
}
