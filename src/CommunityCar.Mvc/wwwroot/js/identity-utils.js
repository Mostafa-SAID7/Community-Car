/**
 * Identity Utilities
 * Shared logic for Login, Register, and Password Reset pages.
 */

function togglePassword(inputId, iconId) {
    const passwordInput = document.getElementById(inputId || 'floatingPassword');
    const toggleIcon = document.getElementById(iconId || 'toggleIcon');

    if (!passwordInput || !toggleIcon) return;

    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        toggleIcon.classList.remove('fa-eye');
        toggleIcon.classList.add('fa-eye-slash');
    } else {
        passwordInput.type = 'password';
        toggleIcon.classList.remove('fa-eye-slash');
        toggleIcon.classList.add('fa-eye');
    }
}

/**
 * Validate password requirements
 * Requirements: Min 6 chars, 1 uppercase, 1 lowercase, 1 digit
 */
function validatePasswordRequirements(password) {
    const requirements = {
        length: password.length >= 6,
        uppercase: /[A-Z]/.test(password),
        lowercase: /[a-z]/.test(password),
        digit: /[0-9]/.test(password)
    };
    
    return {
        isValid: requirements.length && requirements.uppercase && requirements.lowercase && requirements.digit,
        requirements: requirements
    };
}

/**
 * Show password requirements feedback
 */
function showPasswordFeedback(inputId, feedbackId) {
    const passwordInput = document.getElementById(inputId);
    const feedbackElement = document.getElementById(feedbackId);
    
    if (!passwordInput || !feedbackElement) return;
    
    // Hide server-side validation errors for password field
    const passwordValidationSpan = passwordInput.parentElement.querySelector('.text-danger');
    
    passwordInput.addEventListener('input', function() {
        const password = this.value;
        const validation = validatePasswordRequirements(password);
        
        // Hide server validation error when user starts typing
        if (passwordValidationSpan) {
            passwordValidationSpan.style.display = 'none';
        }
        
        let feedbackHtml = '<div class="password-requirements mt-2">';
        feedbackHtml += `<small class="${validation.requirements.length ? 'text-success' : 'text-danger'}">`;
        feedbackHtml += `${validation.requirements.length ? '✓' : '✗'} At least 6 characters</small><br>`;
        feedbackHtml += `<small class="${validation.requirements.uppercase ? 'text-success' : 'text-danger'}">`;
        feedbackHtml += `${validation.requirements.uppercase ? '✓' : '✗'} One uppercase letter</small><br>`;
        feedbackHtml += `<small class="${validation.requirements.lowercase ? 'text-success' : 'text-danger'}">`;
        feedbackHtml += `${validation.requirements.lowercase ? '✓' : '✗'} One lowercase letter</small><br>`;
        feedbackHtml += `<small class="${validation.requirements.digit ? 'text-success' : 'text-danger'}">`;
        feedbackHtml += `${validation.requirements.digit ? '✓' : '✗'} One digit</small>`;
        feedbackHtml += '</div>';
        
        feedbackElement.innerHTML = feedbackHtml;
    });
}
