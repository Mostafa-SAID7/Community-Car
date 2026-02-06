/**
 * Dashboard Page Logic
 */
const DashboardPage = {
    init: () => {
        console.log("Dashboard page initialized");
    }
};

if (document.body.classList.contains('dashboard-page')) {
    document.addEventListener('DOMContentLoaded', DashboardPage.init);
}
