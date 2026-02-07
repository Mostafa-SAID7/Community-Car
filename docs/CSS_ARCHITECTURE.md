# CSS Architecture Guide

## Overview

This document describes the CSS architecture for the CommunityCar project. We follow a modular, component-based approach with global utility classes to minimize inline styles and improve maintainability.

## Directory Structure

```
wwwroot/css/
├── abstracts/          # Variables, mixins, breakpoints
│   ├── variables.css
│   └── breakpoints.css
├── base/               # Reset, typography, base styles
│   ├── reset.css
│   ├── typography.css
│   └── base.css
├── layout/             # Layout components
│   ├── header.css
│   ├── footer.css
│   ├── navigation.css
│   ├── grid.css
│   └── sidebar.css
├── components/         # Reusable UI components
│   ├── buttons.css
│   ├── cards.css
│   ├── forms.css
│   ├── modals.css
│   ├── badges.css
│   ├── dropdowns.css
│   ├── avatars.css
│   ├── content.css
│   ├── notifications.css
│   ├── tags.css
│   └── categories.css
├── pages/              # Page-specific styles
│   ├── home.css
│   ├── dashboard.css
│   ├── qa.css
│   └── security.css
├── utilities/          # Utility classes
│   ├── helpers.css
│   ├── animations.css
│   └── theme-utilities.css
├── responsive/         # Responsive breakpoints
│   ├── _mobile.css
│   ├── _tablet.css
│   └── _desktop.css
├── site.css            # Main import file
└── feed.css            # Feed-specific styles
```

## CSS Principles

### 1. No Inline Styles
Avoid using `style=""` attributes in HTML. Instead, use utility classes or create component classes.

**❌ Bad:**
```html
<div style="width: 350px; max-height: 500px; overflow-y: auto;">
```

**✅ Good:**
```html
<div class="dropdown-menu-xl max-h-500 overflow-y-auto">
```

### 2. Component-Based Styling
Each UI component should have its own CSS file in the `components/` directory.

### 3. Utility-First Approach
Use utility classes for common patterns like spacing, sizing, and positioning.

## Utility Classes Reference

### Display
- `.d-none`, `.d-block`, `.d-flex`, `.d-inline-block`

### Positioning
- `.position-relative`, `.position-absolute`, `.position-sticky`
- `.sticky-sidebar` - Sticky positioning with top: 80px

### Sizing
- `.size-32`, `.size-40`, `.size-45`, `.size-48` - Fixed square dimensions
- `.size-200` - Height: 200px
- `.max-w-250`, `.max-w-320`, `.max-w-350` - Max widths
- `.max-h-480`, `.max-h-500` - Max heights
- `.w-100`, `.h-100` - Full width/height

### Spacing
- `.m-{0-5}`, `.mt-{0-5}`, `.mb-{0-5}` - Margins
- `.p-{0-5}` - Padding
- `.gap-{1-5}` - Flex/Grid gaps

### Typography
- `.text-xs`, `.text-sm`, `.text-base`, `.text-lg`, `.text-xl` - Font sizes
- `.fw-normal`, `.fw-medium`, `.fw-semibold`, `.fw-bold` - Font weights
- `.line-height-tight`, `.line-height-normal`, `.line-height-relaxed`
- `.letter-spacing-wide` - Letter spacing: 0.05em
- `.text-truncate` - Ellipsis overflow

### Borders & Radius
- `.rounded`, `.rounded-lg`, `.rounded-xl`, `.rounded-2xl`, `.rounded-circle`

### Opacity
- `.opacity-25`, `.opacity-50`, `.opacity-75`, `.opacity-100`

### Object Fit
- `.object-cover`, `.object-contain`

### Overflow
- `.overflow-auto`, `.overflow-hidden`, `.overflow-y-auto`, `.overflow-x-auto`

### Flex Utilities
- `.flex-row`, `.flex-column`, `.flex-wrap`, `.flex-grow-1`
- `.justify-content-{start|end|center|between|around}`
- `.align-items-{start|end|center|baseline|stretch}`

### Z-Index
- `.z-1`, `.z-10`, `.z-50`, `.z-100`, `.z-1000`

## Component Classes

### Badges
```html
<!-- Notification badge -->
<span class="badge bg-danger notification-badge">5</span>

<!-- Badge sizes -->
<span class="badge badge-xs">XS</span>
<span class="badge badge-sm">SM</span>
<span class="badge badge-lg">LG</span>

<!-- Badge variants -->
<span class="badge badge-primary">Primary</span>
<span class="badge badge-success">Success</span>
<span class="badge badge-danger">Danger</span>
```

### Dropdowns
```html
<!-- Dropdown menu sizes -->
<div class="dropdown-menu dropdown-menu-sm">...</div>
<div class="dropdown-menu dropdown-menu-md">...</div>
<div class="dropdown-menu dropdown-menu-lg">...</div>
<div class="dropdown-menu dropdown-menu-xl">...</div>

<!-- Notification dropdown -->
<div class="dropdown-menu notification-dropdown">...</div>

<!-- Share dropdown -->
<div class="dropdown-menu share-dropdown">...</div>
```

### Avatars
```html
<!-- Avatar sizes -->
<div class="avatar avatar-xs"><img src="..." /></div>
<div class="avatar avatar-sm"><img src="..." /></div>
<div class="avatar avatar-md"><img src="..." /></div>
<div class="avatar avatar-lg"><img src="..." /></div>

<!-- Brand logo -->
<div class="brand-logo-circle">
    <i class="fas fa-car"></i>
</div>
```

### Content Display
```html
<!-- Readable content -->
<div class="question-content">...</div>
<div class="answer-content">...</div>
<div class="content-readable">...</div>

<!-- Metadata text -->
<div class="metadata-text">
    <i class="fas fa-clock"></i>
    <span>2 hours ago</span>
</div>

<!-- Admin section header -->
<div class="admin-section-header">Admin</div>

<!-- Star rating -->
<div class="star-rating">
    <i class="fas fa-star star filled"></i>
    <i class="fas fa-star star filled"></i>
    <i class="fas fa-star star empty"></i>
</div>
```

### Cards
```html
<!-- Card variants -->
<div class="card card-flat">...</div>
<div class="card card-elevated">...</div>
<div class="card card-interactive">...</div>
<div class="card card-resolved">...</div>
<div class="card card-accepted">...</div>
```

## Migration Guide

### Removing Inline Styles

1. **Identify the pattern**: Look at what the inline style does
2. **Check utilities**: See if a utility class exists
3. **Create component class**: If it's a repeated pattern, create a component class
4. **Update the view**: Replace inline style with class

### Example Migration

**Before:**
```html
<div style="width: 350px; max-height: 500px; overflow-y: auto;">
    <span style="font-size: 0.75rem; color: #6c757d;">
        Posted 2 hours ago
    </span>
</div>
```

**After:**
```html
<div class="dropdown-menu-xl max-h-500 overflow-y-auto">
    <span class="metadata-text">
        Posted 2 hours ago
    </span>
</div>
```

## Best Practices

1. **Use semantic class names**: `.notification-badge` instead of `.badge-top-right`
2. **Combine utilities**: Use multiple utility classes instead of creating single-use components
3. **Keep specificity low**: Avoid deep nesting and !important
4. **Mobile-first**: Write base styles for mobile, then add responsive overrides
5. **Dark mode support**: Use CSS variables and provide dark mode variants
6. **Consistent spacing**: Use the spacing scale (0.25rem, 0.5rem, 1rem, 1.5rem, 3rem)
7. **Reusable components**: If a pattern appears 3+ times, create a component class

## Adding New Styles

### For a new component:
1. Create a new file in `components/` (e.g., `components/timeline.css`)
2. Add the import to `site.css`
3. Use BEM naming convention if needed
4. Provide dark mode variants

### For a new utility:
1. Add to `utilities/helpers.css`
2. Follow the existing naming pattern
3. Keep it simple and single-purpose

## Dark Mode

All components should support dark mode using the `[data-theme="dark"]` selector:

```css
.notification-dropdown {
    background-color: var(--bg-primary);
}

[data-theme="dark"] .notification-dropdown {
    background-color: rgba(30, 30, 30, 0.95);
}
```

## Performance

- **Minimize CSS**: All CSS is bundled and minified in production
- **Critical CSS**: Consider inlining critical above-the-fold styles
- **Lazy load**: Page-specific CSS can be loaded on demand
- **Purge unused**: Use PurgeCSS or similar tools to remove unused styles

## Tools & Scripts

### Remove Inline Styles Script
Run this PowerShell script to remove `<style>` blocks from views:

```powershell
.\scripts\remove-inline-styles.ps1
```

## Resources

- [CSS Variables Reference](abstracts/variables.css)
- [Utility Classes](utilities/helpers.css)
- [Component Library](components/)
- [Responsive Breakpoints](abstracts/breakpoints.css)
