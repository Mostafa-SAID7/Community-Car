/**
 * Dropdown Component Logic
 */
const Dropdown = {
    toggle: (dropdownId) => {
        const dropdown = document.getElementById(dropdownId);
        if (dropdown) dropdown.classList.toggle('show');
    }
};

window.Dropdown = Dropdown;
