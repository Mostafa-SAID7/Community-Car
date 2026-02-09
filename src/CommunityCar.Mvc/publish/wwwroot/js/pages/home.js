/**
 * Home Page Logic
 */
const HomePage = {
    init: () => {
        console.log("Home page initialized");
    }
};

if (document.body.classList.contains('home-page')) {
    document.addEventListener('DOMContentLoaded', HomePage.init);
}
