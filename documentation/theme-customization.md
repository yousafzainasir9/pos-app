# Theme Customization System Documentation

## Overview

The Cookie Barrel POS system now includes a comprehensive theming system that allows administrators to customize colors, branding, and appearance across the entire application.

## Features

✅ **Customizable Color Schemes**
- Primary colors (5 shades)
- Secondary colors (5 shades)
- Accent colors (3 shades)
- Status colors (Success, Warning, Danger, Info)
- Logo colors (Background, Text, Accent, Icon)
- Neutral colors (Backgrounds, Borders, Surfaces)
- Text colors (Primary, Secondary, Muted, Light, Dark)

✅ **Preset Themes**
- Default (Cookie Barrel Blue/Orange)
- Dark Mode
- Eco/Green Theme
- Premium/Purple Theme

✅ **Company Branding**
- Customizable company name
- Custom logo URL support
- Dynamic page title
- Favicon support

✅ **Live Preview**
- Real-time color updates
- Instant application of changes
- Preview components

✅ **Persistent Storage**
- Saves theme to localStorage
- Restores theme on page reload
- Reset to default option

## File Structure

```
frontend/src/theme/
├── theme.config.ts      # Theme configurations and color definitions
├── ThemeContext.tsx     # React Context for theme management
├── theme.css           # CSS variables and utility classes
```

```
frontend/src/pages/
├── ThemeSettingsPage.tsx  # Admin UI for theme customization
```

## How It Works

### 1. Theme Configuration (`theme.config.ts`)

Defines all available themes and color schemes:

```typescript
export interface ThemeColors {
  primary: string;
  primaryHover: string;
  // ... all color properties
}

export interface ThemeConfig {
  colors: ThemeColors;
  companyName: string;
  logoUrl?: string;
  favicon?: string;
}
```

**Available Themes:**
- `default`: Cookie Barrel branding (Blue/Orange)
- `dark`: Dark mode with blue accents
- `eco`: Green/environmental theme
- `premium`: Purple/pink premium theme

### 2. Theme Context (`ThemeContext.tsx`)

Provides theme management throughout the application:

```typescript
const { 
  theme,           // Current theme config
  themeName,       // Current theme name
  setTheme,        // Switch to a preset theme
  updateColors,    // Update specific colors
  updateCompanyName,  // Update company name
  updateLogo,      // Update logo URL
  resetTheme       // Reset to default
} = useTheme();
```

**Features:**
- Applies theme colors to CSS variables
- Updates Bootstrap variables
- Saves to localStorage
- Restores on reload

### 3. CSS Variables (`theme.css`)

All colors are available as CSS custom properties:

```css
--color-primary
--color-primary-hover
--color-primary-active
--color-logo-background
--color-logo-text
/* ... and many more */
```

**Usage in CSS:**
```css
.my-component {
  background-color: var(--color-primary);
  color: var(--color-text-light);
}
```

**Usage with Utility Classes:**
```jsx
<div className="bg-theme-primary text-theme-light">
  Themed content
</div>
```

## Using the Theme System

### For Administrators

1. **Access Theme Settings:**
   - Login as Admin
   - Click user dropdown (top right)
   - Select "Theme Settings"

2. **Choose a Preset Theme:**
   - Select from: Default, Dark, Eco, or Premium
   - Changes apply instantly

3. **Customize Colors:**
   - Scroll to color sections
   - Click color picker or enter hex code
   - Click "Save All Color Changes"

4. **Update Branding:**
   - Change company name
   - Add custom logo URL
   - Click respective save buttons

5. **Preview Changes:**
   - Click "Show Preview"
   - See buttons, badges, and cards with new colors
   - Navigate app to see full effect

6. **Reset Theme:**
   - Click "Reset to Default"
   - Confirms before resetting
   - Clears all customizations

### For Developers

#### 1. Using Theme in Components

```typescript
import { useTheme } from '@/theme/ThemeContext';

const MyComponent = () => {
  const { theme } = useTheme();
  
  return (
    <div style={{ 
      backgroundColor: theme.colors.primary,
      color: theme.colors.textLight 
    }}>
      {theme.companyName}
    </div>
  );
};
```

#### 2. Using CSS Variables

```css
.custom-button {
  background-color: var(--color-primary);
  border-color: var(--color-primary-dark);
}

.custom-button:hover {
  background-color: var(--color-primary-hover);
}
```

#### 3. Using Utility Classes

```jsx
<Button className="btn-theme-primary">
  Primary Button
</Button>

<Badge className="badge-theme-accent">
  Accent Badge
</Badge>

<div className="bg-theme-surface border-theme-primary">
  Themed Card
</div>
```

#### 4. Adding New Theme Colors

**Step 1**: Add to `theme.config.ts`
```typescript
export interface ThemeColors {
  // ... existing colors
  customColor: string;
  customColorHover: string;
}

export const defaultTheme: ThemeConfig = {
  colors: {
    // ... existing colors
    customColor: '#ff0000',
    customColorHover: '#cc0000',
  }
};
```

**Step 2**: Add to `theme.css`
```css
:root {
  --color-custom: #ff0000;
  --color-custom-hover: #cc0000;
}

.bg-custom {
  background-color: var(--color-custom) !important;
}

.text-custom {
  color: var(--color-custom) !important;
}
```

**Step 3**: Use in components
```jsx
<div style={{ backgroundColor: theme.colors.customColor }}>
  or
</div>
<div className="bg-custom">
  Custom colored background
</div>
```

#### 5. Creating New Preset Themes

Add to `theme.config.ts`:

```typescript
export const myCustomTheme: ThemeConfig = {
  companyName: 'My POS System',
  colors: {
    primary: '#your-color',
    // ... define all colors
  }
};

export const availableThemes = {
  default: defaultTheme,
  dark: darkTheme,
  eco: ecoTheme,
  premium: premiumTheme,
  custom: myCustomTheme,  // Add here
};
```

## CSS Classes Reference

### Background Colors
- `.bg-theme-primary`
- `.bg-theme-secondary`
- `.bg-theme-accent`
- `.bg-theme-surface`
- `.bg-theme-background`

### Text Colors
- `.text-theme-primary`
- `.text-theme-secondary`
- `.text-theme-accent`
- `.text-theme-muted`

### Borders
- `.border-theme-primary`
- `.border-theme-secondary`
- `.border-theme-accent`

### Buttons
- `.btn-theme-primary`
- `.btn-theme-secondary`
- `.btn-theme-accent`

### Logo Elements
- `.logo-container` - Logo background
- `.logo-text` - Logo text color
- `.logo-icon` - Logo icon color
- `.logo-accent` - Logo accent color

### Cards
- `.card-theme` - Themed card with hover effect

### Navbar
- `.navbar-theme` - Themed navigation bar

### Tables
- `.table-theme` - Themed table

## Bootstrap Integration

The theme system automatically updates Bootstrap CSS variables:

```
--bs-primary → var(--color-primary)
--bs-secondary → var(--color-secondary)
--bs-success → var(--color-success)
--bs-danger → var(--color-danger)
--bs-warning → var(--color-warning)
--bs-info → var(--color-info)
```

This means all Bootstrap components (buttons, badges, alerts, etc.) automatically use the themed colors.

## Preset Theme Details

### Default Theme (Cookie Barrel)
- **Primary**: Blue (#0d6efd)
- **Accent**: Cookie Brown/Orange (#d97706)
- **Best for**: Default bakery branding

### Dark Theme
- **Primary**: Light Blue (#3b82f6)
- **Background**: Dark Navy (#0f172a)
- **Best for**: Night shifts, reduced eye strain

### Eco Theme
- **Primary**: Green (#059669)
- **Accent**: Emerald (#10b981)
- **Best for**: Eco-friendly, organic branding

### Premium Theme
- **Primary**: Purple (#7c3aed)
- **Accent**: Pink (#ec4899)
- **Best for**: Luxury, premium positioning

## LocalStorage Keys

- `pos-theme`: JSON of current theme configuration
- `pos-theme-name`: Name of current preset theme

## API Integration (Future Enhancement)

To save themes to the backend:

```typescript
// Save theme to server
await api.post('/settings/theme', theme);

// Load theme from server
const theme = await api.get('/settings/theme');
setTheme(theme.data);
```

## Best Practices

1. **Always use CSS variables** for colors instead of hardcoded values
2. **Test with all preset themes** to ensure components work well
3. **Use semantic color names** (primary, secondary) not specific colors (blue, red)
4. **Provide contrast** - ensure text is readable on all backgrounds
5. **Document custom colors** when adding new theme properties
6. **Test accessibility** - maintain WCAG AA contrast ratios

## Troubleshooting

### Colors not updating?
- Check browser console for errors
- Verify CSS variables are defined in theme.css
- Clear localStorage and try again
- Hard refresh (Ctrl+F5)

### Theme not persisting?
- Check localStorage is enabled
- Verify no errors in ThemeContext
- Check browser storage limits

### Custom logo not showing?
- Verify logo URL is accessible
- Check CORS settings if external image
- Verify image format (PNG, JPG, SVG)

## Future Enhancements

- [ ] Font customization
- [ ] Dark mode auto-detection
- [ ] Multiple logo variants (light/dark)
- [ ] Theme export/import
- [ ] Server-side theme storage
- [ ] Theme marketplace
- [ ] Advanced CSS customization
- [ ] Per-user theme preferences

## Support

For theme-related issues:
1. Check this documentation
2. Verify theme.config.ts has all required colors
3. Check browser console for errors
4. Test with default theme
5. Contact development team

## Summary

The theming system provides:
- ✅ Easy customization via UI
- ✅ Preset themes for quick switching
- ✅ Full color control (40+ colors)
- ✅ Company branding options
- ✅ Persistent storage
- ✅ Real-time updates
- ✅ Developer-friendly API
- ✅ Bootstrap integration

Administrators can customize the POS appearance to match their brand without any coding!
