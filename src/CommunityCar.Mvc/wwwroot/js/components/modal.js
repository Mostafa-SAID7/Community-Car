/**
 * Modal and Alert Component Logic using SweetAlert2
 */
const Modal = {
    // Basic show/hide for HTML modals
    show: (modalId) => {
        const modal = document.getElementById(modalId);
        if (modal) {
            const bsModal = bootstrap.Modal.getOrCreateInstance(modal);
            bsModal.show();
        }
    },

    // Standard high-end configuration for SweetAlert2
    _premiumConfig: {
        customClass: {
            popup: 'premium-swal-popup',
            title: 'premium-swal-title',
            htmlContainer: 'premium-swal-html',
            icon: 'premium-swal-icon',
            confirmButton: 'premium-swal-confirm',
            cancelButton: 'premium-swal-cancel',
            actions: 'premium-swal-actions'
        },
        buttonsStyling: false, // Use our own CSS classes
        backdrop: `rgba(0,0,0,0.4)`,
        showConfirmButton: true,
    },

    // Professional Alerts using SweetAlert2
    alert: (title, text, icon = 'info') => {
        return Swal.fire({
            ...Modal._premiumConfig,
            title: title,
            text: text,
            icon: icon,
            confirmButtonText: window.I18n?.translations.alerts.great || 'Great!'
        });
    },

    // Professional Confirmations using SweetAlert2
    confirm: (title, text, confirmButtonText = 'Yes, do it!') => {
        return Swal.fire({
            ...Modal._premiumConfig,
            title: title,
            text: text,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: confirmButtonText || window.I18n?.translations.alerts.yes || 'Yes, do it!',
            cancelButtonText: window.I18n?.translations.alerts.cancel || 'Cancel'
        });
    }
};

window.Modal = Modal;
