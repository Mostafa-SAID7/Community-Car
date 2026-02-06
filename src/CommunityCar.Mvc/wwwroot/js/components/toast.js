/**
 * Toast Component Logic using Toastr.js
 */
const Toast = {
    show: (message, type = 'info', title = '') => {
        toastr.options = {
            "closeButton": true,
            "progressBar": true,
            "positionClass": "toast-top-right",
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "5000",
            "extendedTimeOut": "1000"
        };

        switch (type.toLowerCase()) {
            case 'success':
                toastr.success(message, title);
                break;
            case 'error':
                toastr.error(message, title);
                break;
            case 'warning':
                toastr.warning(message, title);
                break;
            default:
                toastr.info(message, title);
        }
    }
};

window.Toast = Toast;
