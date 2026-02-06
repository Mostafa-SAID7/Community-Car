/**
 * Modal and Alert Component Logic using SweetAlert2
 */
const Modal = {
    // Basic show/hide for HTML modals
    show: (modalId) => {
        const modal = document.getElementById(modalId);
        if (modal) {
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        }
    },

    // Professional Alerts using SweetAlert2
    alert: (title, text, icon = 'info') => {
        return Swal.fire({
            title: title,
            text: text,
            icon: icon,
            confirmButtonColor: '#fb2c36' // Using our primary red
        });
    },

    // Professional Confirmations using SweetAlert2
    confirm: (title, text, confirmButtonText = 'Yes, do it!') => {
        return Swal.fire({
            title: title,
            text: text,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#fb2c36',
            cancelButtonColor: '#6e7881',
            confirmButtonText: confirmButtonText
        });
    }
};

window.Modal = Modal;
