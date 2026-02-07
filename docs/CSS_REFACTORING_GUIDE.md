# CSS Refactoring Guide

## Overview

This guide explains how to refactor CSS files to use CSS variables, eliminate duplicates, and maintain consistency across the project.

## CSS Variable System

All colors and design tokens are centralized in `wwwroot/css/abstracts/variables.css`.

### Color Variables

#### Brand Colors
```css
--color-primary          /* Main brand color (red) */
--color-primary-hover    /* Hover state */
--color-primary-active   /* Active state */
--color-primary-light    /* Light variant */
```

#### Status Colors
```css
--color-success          /* Success state */
--color-success-bg       /* Success background */
--color-warning          /* Warning state */
--color-warning-bg       /* Warning background */
--color-error            /* Error state */
--color-error-bg         /* Error background */
--color-info             /* Info state */
--color-info-bg          /* Info background */
--color-secondary        /* Secondary color */
--color-danger           /* Danger state */
```

#### Text Colors
```css
--text-primary           /* Primary text */
--text-secondary         /* Secondary text */
--text-tertiary          /* Tertiary/muted text */
--text-inverse           /* Inverse text (white on dark) */
--color-text-muted       /* Muted text */
--color-text-dark        /* Dark text */
--color-text-medium      /* Medium text */
```

#### Background Colors
```css
--bg-primary             /* Primary background */
--bg-secondary           /* Secondary background */
--bg-tertiary            /* Tertiary background */
--bg-hover               /* Hover background */
--bg-active              /* Active background */
--bg-surface             /* Surface background */
--color-bg-light         /* Light background */
--color-bg-white         /* White background */
```

#### Border Colors
```css
--border-primary         /* Primary border */
--border-secondary       /* Secondary border */
--border-focus           /* Focus border */
--border-subtle          /* Subtle border */
--color-border-light     /* Light border */
--color-border-subtle    /* Subtle border */
--color-border-gray      /* Gray border */
```

#### Shadows
```css
--shadow-sm              /* Small shadow */
--shadow-md              /* Medium shadow */
--shadow-lg              /* Large shadow */
--shadow-xl              /* Extra large shadow */
--shadow-premium-subtle  /* Premium subtle shadow */
--shadow-premium-large   /* Premium large shadow */
--shadow-premium-glow    /* Premium glow effect */
```

#### Spacing
```css
--spacing-xs             /* 0.25rem */
--spacing-sm             /* 0.5rem */
--spacing-md             /* 1rem */
--spacing-lg             /* 1.5rem */
--spacing-xl             /* 2rem */
--spacing-2xl            /* 3rem */
--spacing-3xl            /* 4rem */
```

#### Border Radius
```css
--radius-sm              /* 0.375rem */
--radius-md              /* 0.5rem */
--radius-lg              /* 0.75rem */
--radius-xl              /* 1rem */
--radius-2xl             /* 1.5rem */
--radius-full            /* 9999px */
```

#### Transitions
```css
--transition-fast        /* 150ms ease-in-out */
--transition-base        /* 250ms ease-in-out */
--transition-slow        /* 350ms ease-in-out */
```

## Refactoring Process

### Step 1: Identify Hardcoded Colors

Run the audit script to find hardcoded colors:

```powershell
.\scripts\audit-css-colors.ps1
```

### Step 2: Map Colors to Variables

For each hardcoded color, determine the appropriate CSS variable:

| Hardcoded Color | CSS Variable | Usage |
|----------------|--------------|-------|
| `#ffffff` | `var(--color-bg-white)` | White backgrounds |
| `#1a202c` | `var(--color-text-dark)` | Dark text |
| `#718096` | `var(--color-text-muted)` | Muted text |
| `#e2e8f0` | `var(--color-border-light)` | Light borders |
| `#f7fafc` | `var(--color-bg-light)` | Light backgrounds |
| `#4a5568` | `var(--color-text-medium)` | Medium text |
| `#cbd5e0` | `var(--color-border-gray)` | Gray borders |
| `#f0f0f0` | `var(--color-border-subtle)` | Subtle borders |
| `#f8f9fa` | `var(--bg-hover)` | Hover backgrounds |
| `#28a745` | `var(--color-success)` | Success color |
| `#d4edda` | `var(--color-success-bg)` | Success background |
| `#ffc107` | `var(--color-warning)` | Warning color |
| `#dc3545` | `var(--color-danger)` | Danger color |
| `#007bff` | `var(--color-primary)` | Primary color |
| `#6c757d` | `var(--color-secondary)` | Secondary color |

### Step 3: Replace Hardcoded Colors

**Before:**
```css
.notification-dropdown .dropdown-item {
    border-bottom: 1px solid #f0f0f0;
}

.notification-dropdown .dropdown-item:hover {
    background-color: #f8f9fa;
}
```

**After:**
```css
.notification-dropdown .dropdown-item {
    border-bottom: 1px solid var(--color-border-subtle);
}

.notification-dropdown .dropdown-item:hover {
    background-color: var(--bg-hover);
}
```

### Step 4: Add Dark Mode Support

After replacing with variables, dark mode is automatically supported:

```css
/* Light mode (default) */
:root {
    --bg-hover: #f8f9fa;
    --color-border-subtle: #f0f0f0;
}

/* Dark mode */
[data-theme="dark"] {
    --bg-hover: var(--gray-800);
    --color-border-subtle: var(--gray-700);
}
```

## Eliminating Duplicates

### Common Duplicate Patterns

#### 1. Dropdown Styles

**Problem:** Dropdown styles repeated in multiple files

**Solution:** Consolidate in `components/dropdowns.css`

```css
/* components/dropdowns.css */
.dropdown-menu {
    background-color: var(--bg-primary);
    border: 1px solid var(--border-primary);
    box-shadow: var(--shadow-lg);
}

.dropdown-item:hover {
    background-color: var(--bg-hover);
}
```

#### 2. Badge Styles

**Problem:** Badge colors hardcoded in multiple places

**Solution:** Use badge component classes

```css
/* components/badges.css */
.badge-success {
    background-color: var(--color-success);
    color: var(--text-inverse);
}
```

#### 3. Shadow Definitions

**Problem:** Box shadows defined inline

**Solution:** Use shadow variables

```css
/* Before */
box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);

/* After */
box-shadow: var(--shadow-lg);
```

## Refactoring Checklist

- [ ] Run `audit-css-colors.ps1` to identify hardcoded colors
- [ ] Replace hex colors with CSS variables
- [ ] Replace rgba colors with CSS variables
- [ ] Remove duplicate style definitions
- [ ] Test in light mode
- [ ] Test in dark mode
- [ ] Verify responsive behavior
- [ ] Check browser compatibility
- [ ] Update documentation

## Adding New Colors

If you need a new color that doesn't exist in variables:

1. **Determine if it's truly needed** - Can you use an existing variable?
2. **Add to variables.css** - Add both light and dark mode versions
3. **Use semantic naming** - `--color-success-bg` not `--color-light-green`
4. **Document the usage** - Add a comment explaining when to use it

Example:

```css
:root {
    /* Highlight Colors */
    --color-highlight: #fef3c7;
    --color-highlight-border: #f59e0b;
}

[data-theme="dark"] {
    --color-highlight: #78350f;
    --color-highlight-border: #fbbf24;
}
```

## Common Refactoring Patterns

### Pattern 1: Login/Auth Pages

```css
/* Before */
.login-container {
    background: white;
    color: #1a202c;
    border: 1px solid #e2e8f0;
}

/* After */
.login-container {
    background: var(--color-bg-white);
    color: var(--color-text-dark);
    border: 1px solid var(--color-border-light);
}
```

### Pattern 2: Notification Styles

```css
/* Before */
.notification-item {
    background: #f8f9fa;
    border-bottom: 1px solid #f0f0f0;
}

/* After */
.notification-item {
    background: var(--bg-hover);
    border-bottom: 1px solid var(--color-border-subtle);
}
```

### Pattern 3: Status Indicators

```css
/* Before */
.status-success {
    background: #d4edda;
    color: #28a745;
}

/* After */
.status-success {
    background: var(--color-success-bg);
    color: var(--color-success);
}
```

## Testing

After refactoring, test:

1. **Visual consistency** - Colors should look the same
2. **Dark mode** - All elements should have proper dark mode colors
3. **Hover states** - Interactive elements should have visible hover states
4. **Focus states** - Form elements should have visible focus indicators
5. **Accessibility** - Color contrast should meet WCAG standards

## Tools

### Audit Script
```powershell
.\scripts\audit-css-colors.ps1
```

### Remove Inline Styles
```powershell
.\scripts\remove-inline-styles.ps1
```

## Best Practices

1. **Always use CSS variables** - Never hardcode colors
2. **Semantic naming** - Use purpose-based names, not color names
3. **Consistent spacing** - Use spacing variables
4. **Reusable components** - Create component classes for repeated patterns
5. **Dark mode first** - Always define dark mode variants
6. **Document changes** - Update this guide when adding new variables
7. **Test thoroughly** - Check both light and dark modes

## Migration Priority

1. **High Priority** - User-facing components (buttons, forms, cards)
2. **Medium Priority** - Layout components (header, footer, sidebar)
3. **Low Priority** - Admin/dashboard components
4. **Lowest Priority** - Third-party library overrides

## Resources

- [CSS Variables Documentation](abstracts/variables.css)
- [CSS Architecture Guide](CSS_ARCHITECTURE.md)
- [Component Library](../wwwroot/css/components/)
- [MDN CSS Variables](https://developer.mozilla.org/en-US/docs/Web/CSS/Using_CSS_custom_properties)
