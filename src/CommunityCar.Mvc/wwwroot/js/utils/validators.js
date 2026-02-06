/**
 * Validation Utilities
 */
const Validators = {
    isEmail: (email) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email),
    isRequired: (value) => value && value.trim().length > 0
};

window.Validators = Validators;
