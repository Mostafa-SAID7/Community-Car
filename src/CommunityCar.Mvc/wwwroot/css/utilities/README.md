# CSS Utilities Documentation

This folder contains centralized utility CSS files that provide cross-cutting concerns for the entire application.

## Utility Files

### 1. **rtl.css** - Right-to-Left Support
Provides comprehensive RTL (Right-to-Left) support for Arabic, Hebrew, and other RTL languages.

**Features:**
- Text alignment adjustments
- Flex direction reversals
- Margin and padding swaps
- Border positioning
- Icon flipping
- Component-specific RTL adjustments (cards, forms, search, navigation, etc.)

**Usage:**
```html
<html dir="rtl">
  <!-- All RTL styles automatically apply -->
</html>
```

**Key Classes:**
- Automatic adjustments for `[dir="rtl"]` elements
- Icon flipping with `.icon-flip`
- All Bootstrap-like utilities automatically flip (ms/me, ps/pe, etc.)

---

### 2. **print.css** - Print Styles
Optimizes the application for printing, hiding interactive elements and ensuring readable output.

**Features:**
- Hides navigation, buttons, forms, and interactive elements
- Optimizes typography for print
- Ensures proper page breaks
- Shows URLs after links
- Optimizes images and tables
- Removes backgrounds and shadows

**Usage:**
Automatically applies when printing (Ctrl+P / Cmd+P)

**Utility Classes:**
- `.print-only` - Show only when printing
- `.no-print` / `.screen-only` - Hide when printing
- `.print-break-before` - Force page break before element
- `.print-break-after` - Force page break after element
- `.print-no-break` - Prevent page break inside element

---

### 3. **accessibility.css** - Accessibility (A11y)
Ensures the application is accessible to all users, including those using assistive technologies.

**Features:**
- Modern focus-visible states for keyboard navigation
- Skip links for screen readers
- Screen reader only content
- ARIA live regions
- High contrast mode support
- Reduced motion support
- Touch target size optimization
- Form accessibility (required fields, error states)
- Semantic HTML support

**Usage:**

**Focus States:**
```html
<!-- Automatically applies focus-visible to all interactive elements -->
<button>Click me</button>
```

**Screen Reader Only:**
```html
<span class="sr-only">Additional context for screen readers</span>
```

**Skip Links:**
```html
<a href="#main-content" class="skip-link">Skip to main content</a>
```

**Key Features:**
- `:focus-visible` for keyboard-only focus indicators
- `@media (prefers-reduced-motion: reduce)` disables animations
- `@media (prefers-contrast: high)` increases contrast
- Minimum 44x44px touch targets
- WCAG 2.1 compliant focus indicators

---

### 4. **responsive.css** - Responsive Design
Provides responsive utilities and breakpoint-based styles for all screen sizes.

**Breakpoints:**
- `xs`: 0px (mobile)
- `sm`: 576px (small tablets)
- `md`: 768px (tablets)
- `lg`: 992px (desktops)
- `xl`: 1200px (large desktops)
- `xxl`: 1400px (extra large desktops)

**Features:**
- Display utilities (d-sm-none, d-md-block, etc.)
- Responsive containers and grid
- Responsive spacing (margins, padding)
- Responsive typography
- Responsive flex utilities
- Responsive text alignment
- Component-specific responsive styles (cards, buttons, forms, tables, etc.)

**Usage:**

**Display Utilities:**
```html
<div class="d-none d-md-block">Visible on tablets and up</div>
<div class="d-block d-lg-none">Visible on mobile and tablets only</div>
```

**Responsive Text:**
```html
<p class="text-sm-center text-md-left">Centered on mobile, left on tablets+</p>
```

**Responsive Spacing:**
```html
<div class="mt-5 mt-md-3 mt-sm-2">Large margin on desktop, smaller on mobile</div>
```

**Responsive Width:**
```html
<div class="w-100 w-md-auto">Full width on mobile, auto on tablets+</div>
```

**Mobile/Desktop Only:**
```html
<div class="mobile-only">Only visible on mobile</div>
<div class="desktop-only">Only visible on desktop</div>
```

---

## Other Utility Files

### 5. **helpers.css**
General utility classes for common styling needs (spacing, display, sizing, etc.)

### 6. **animations.css**
Animation utilities and keyframes

### 7. **theme-utilities.css**
Theme-specific utilities (colors, backgrounds, borders)

### 8. **overflow-fix.css**
Fixes for overflow issues

---

## Import Order

The utilities are imported in `site.css` in the following order:

```css
@import 'utilities/helpers.css';
@import 'utilities/animations.css';
@import 'utilities/theme-utilities.css';
@import 'utilities/overflow-fix.css';
@import 'utilities/rtl.css';
@import 'utilities/print.css';
@import 'utilities/accessibility.css';
@import 'utilities/responsive.css';
```

**Important:** Utilities should be imported last to ensure they can override component styles when needed.

---

## Best Practices

### 1. **RTL Support**
- Always test your components in both LTR and RTL modes
- Use logical properties when possible (margin-inline-start instead of margin-left)
- Icons that indicate direction should flip in RTL

### 2. **Print Styles**
- Mark interactive elements with `.no-print` if they shouldn't appear in print
- Use `.print-only` for content that should only appear when printing
- Test print layouts regularly

### 3. **Accessibility**
- Always provide focus indicators for interactive elements
- Use semantic HTML elements
- Provide alternative text for images
- Test with keyboard navigation
- Test with screen readers
- Respect user preferences (reduced motion, high contrast)

### 4. **Responsive Design**
- Design mobile-first, then enhance for larger screens
- Test on multiple screen sizes
- Use responsive utilities instead of custom media queries when possible
- Ensure touch targets are at least 44x44px on mobile

---

## Migration Notes

The following styles have been extracted from component files and centralized:

**From `cards.css`:**
- RTL support for card badges, stats, meta, and tags
- Print styles for cards
- Accessibility focus states
- Reduced motion support
- Responsive card sizing

**From `posts.css`:**
- RTL support for search, filters, post type selector, and file preview
- Print styles for search and pagination
- Responsive styles for posts container, filters, and type selector

These styles are now automatically applied through the centralized utility files.

---

## Contributing

When adding new components:

1. **RTL**: Add RTL-specific adjustments to `rtl.css`
2. **Print**: Add print-specific styles to `print.css`
3. **Accessibility**: Ensure focus states and reduced motion support in `accessibility.css`
4. **Responsive**: Add responsive breakpoints to `responsive.css`

This keeps all cross-cutting concerns centralized and maintainable.
