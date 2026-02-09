/**
 * Login Page Logic
 */
const LoginPage = {
    init: () => {
        console.log("Login page initialized");
    }
};

if (document.body.classList.contains('login-page')) {
    document.addEventListener('DOMContentLoaded', LoginPage.init);
}
