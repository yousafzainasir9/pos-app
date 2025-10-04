# Theme Customization - Quick Start Guide

## For Administrators

### Accessing Theme Settings

1. **Login as Admin**
   ```
   Username: admin
   Password: Admin123!
   ```

2. **Navigate to Theme Settings**
   - Click your name (top right)
   - Select "Theme Settings" with palette icon 🎨

### Quick Theme Change

**Switch to a Preset Theme:**
1. Go to Theme Settings page
2. Click on any preset theme card:
   - **Default** - Cookie Barrel branding
   - **Dark** - Dark mode
   - **Eco** - Green theme
   - **Premium** - Purple theme
3. Changes apply instantly!

### Customize Colors

**Change Primary Color:**
1. Scroll to "Primary Colors" section
2. Click the color picker next to "Primary"
3. Select your brand color
4. Scroll to bottom
5. Click "Save All Color Changes"

**Change Logo Colors:**
1. Scroll to "Logo Colors" section
2. Update:
   - **Logo Background** - Navbar background
   - **Logo Text** - Company name color
   - **Logo Icon** - Cookie icon color
3. Click "Save All Color Changes"

### Update Company Name

1. Find "Company Branding" card
2. Enter new name (e.g., "My Bakery POS")
3. Click "Save Company Name"
4. Name updates everywhere instantly!

### Add Custom Logo

1. Find "Logo" card
2. Enter image URL
3. Click "Save Logo"
4. Logo appears in navbar

### Preview Changes

1. Click "Show Preview" button (top right)
2. See buttons, badges, and cards with new colors
3. Navigate through the app to see full effect

### Reset Theme

**Restore to Default:**
1. Click "Reset to Default" (top right)
2. Confirm the action
3. All customizations cleared
4. Back to Cookie Barrel defaults

---

## For Developers

### Using Theme in React Components

```typescript
import { useTheme } from '@/theme/ThemeContext';

function MyComponent() {
  const { theme } = useTheme();
  
  return (
    <div style={{ backgroundColor: theme.colors.primary }}>
      {theme.companyName}
    </div>
  );
}
```

### Using CSS Variables

```css
.my-button {
  background-color: var(--color-primary);
  color: var(--color-text-light);
}

.my-button:hover {
  background-color: var(--color-primary-hover);
}
```

### Using Utility Classes

```jsx
<Button className="btn-theme-primary">
  Themed Button
</Button>

<div className="bg-theme-accent text-theme-light">
  Accent Background
</div>

<span className="text-theme-primary">
  Primary Text
</span>
```

### Available Color Variables

**Primary Colors:**
- `--color-primary`
- `--color-primary-hover`
- `--color-primary-active`
- `--color-primary-light`
- `--color-primary-dark`

**Secondary Colors:**
- `--color-secondary`
- `--color-secondary-hover`
- `--color-secondary-active`
- `--color-secondary-light`
- `--color-secondary-dark`

**Accent Colors:**
- `--color-accent`
- `--color-accent-hover`
- `--color-accent-light`

**Logo Colors:**
- `--color-logo-background`
- `--color-logo-text`
- `--color-logo-accent`
- `--color-logo-icon`

**Status Colors:**
- `--color-success` / `--color-success-light`
- `--color-warning` / `--color-warning-light`
- `--color-danger` / `--color-danger-light`
- `--color-info` / `--color-info-light`

**Text Colors:**
- `--color-text-primary`
- `--color-text-secondary`
- `--color-text-muted`
- `--color-text-light`
- `--color-text-dark`

### Common Use Cases

**1. Themed Button:**
```jsx
<button className="btn-theme-primary">
  Click Me
</button>
```

**2. Themed Card:**
```jsx
<div className="card card-theme">
  <div className="card-body">
    Content
  </div>
</div>
```

**3. Custom Background:**
```jsx
<div style={{ 
  backgroundColor: theme.colors.accent,
  color: theme.colors.textLight 
}}>
  Accent Section
</div>
```

**4. Logo with Theme Colors:**
```jsx
<div className="logo-container">
  <span className="logo-icon">🍪</span>
  <span className="logo-text">{theme.companyName}</span>
</div>
```

---

## Color Customization Tips

### Choosing Colors

**Primary Color:**
- Main brand color
- Used for buttons, links, headers
- Should be bold and eye-catching
- Example: `#0d6efd` (blue)

**Secondary Color:**
- Supporting color
- Used for less important elements
- Should complement primary
- Example: `#6c757d` (gray)

**Accent Color:**
- Highlight color
- Used for special actions
- Creates visual interest
- Example: `#d97706` (orange)

### Color Harmony

**Monochromatic:**
- Use shades of one color
- Professional and clean
- Example: Blues (#0d6efd → #084298)

**Complementary:**
- Use opposite colors on color wheel
- High contrast, vibrant
- Example: Blue + Orange

**Analogous:**
- Use adjacent colors
- Harmonious, natural
- Example: Blue + Green + Teal

### Accessibility

**Contrast Requirements:**
- Text on background: 4.5:1 ratio (WCAG AA)
- Large text: 3:1 ratio
- Test with online contrast checkers

**Good Combinations:**
- Light text (#ffffff) on dark bg (#212529)
- Dark text (#212529) on light bg (#f8f9fa)
- Avoid: Gray text on gray background

### Logo Colors

**Dark Navbar (Default):**
- Background: Dark (`#2c3e50`)
- Text: White (`#ffffff`)
- Accent: Brand color (`#d97706`)

**Light Navbar:**
- Background: Light (`#ffffff`)
- Text: Dark (`#212529`)
- Accent: Brand color

---

## Common Customization Scenarios

### Scenario 1: Rebrand to New Colors

**Goal:** Change from Cookie Barrel to "Sweet Treats" with pink theme

**Steps:**
1. Change company name to "Sweet Treats"
2. Select **Premium** theme (purple/pink)
3. Customize primary to your pink: `#ec4899`
4. Update logo colors:
   - Background: `#831843`
   - Text: `#ffffff`
   - Accent: `#f472b6`
5. Save all changes

### Scenario 2: Dark Mode for Night Shifts

**Goal:** Reduce eye strain during night operations

**Steps:**
1. Select **Dark** theme
2. Optionally adjust:
   - Background to darker: `#0a0f1a`
   - Primary to softer blue: `#60a5fa`
3. Save changes

### Scenario 3: Match Existing Brand

**Goal:** Match POS to company website colors

**Steps:**
1. Get hex codes from website
2. Update Primary Colors section with brand primary
3. Update Accent Colors with brand accent
4. Update Logo Colors to match
5. Test contrast for readability
6. Save all changes

### Scenario 4: Seasonal Theme

**Goal:** Holiday theme (Christmas - red and green)

**Steps:**
1. Primary: Christmas Red `#dc2626`
2. Accent: Christmas Green `#16a34a`
3. Logo colors: Match theme
4. Add festive company name suffix
5. Save changes
6. Reset after holiday season

---

## Troubleshooting

### Colors Not Applying?

**Solution 1: Hard Refresh**
- Press `Ctrl + F5` (Windows)
- Press `Cmd + Shift + R` (Mac)

**Solution 2: Clear Cache**
1. Open Developer Tools (F12)
2. Right-click refresh button
3. Select "Empty Cache and Hard Reload"

**Solution 3: Check localStorage**
1. Open Developer Tools (F12)
2. Go to Application → Local Storage
3. Find `pos-theme` key
4. Delete it
5. Reload page

### Logo Not Showing?

**Check:**
- URL is accessible
- Image format is supported (PNG, JPG, SVG)
- No CORS issues
- URL is HTTPS if app is HTTPS

### Theme Not Saving?

**Check:**
- Browser allows localStorage
- No private/incognito mode
- Storage quota not exceeded
- Try different browser

### Colors Look Wrong?

**Check:**
- Hex codes are valid (#rrggbb)
- Contrast is sufficient
- Not color blind mode active
- Monitor color calibration

---

## Best Practices

### Do's ✅

- Test theme changes in preview
- Save changes after each section
- Keep contrast ratios high
- Document custom colors
- Reset if unsure

### Don'ts ❌

- Don't use too many colors
- Don't ignore contrast
- Don't forget to save
- Don't use pure black/white everywhere
- Don't change colors too frequently

---

## Quick Reference

### File Locations

```
frontend/src/
├── theme/
│   ├── theme.config.ts      # Color definitions
│   ├── ThemeContext.tsx     # Theme management
│   └── theme.css           # CSS variables
└── pages/
    └── ThemeSettingsPage.tsx # Admin UI
```

### Important URLs

- Theme Settings: `/theme`
- Access: Admin role only

### Storage

- Saves to: `localStorage`
- Keys: `pos-theme`, `pos-theme-name`
- Auto-restore: On page reload

---

## Support

Need help?
1. Check full documentation: `theme-customization.md`
2. Reset to default and try again
3. Contact development team
4. Check browser console for errors

---

## Summary

The theme system allows you to:
- ✅ Choose from 4 preset themes
- ✅ Customize 40+ color variables
- ✅ Update company name
- ✅ Add custom logo
- ✅ Preview changes live
- ✅ Reset to defaults
- ✅ Save preferences automatically

Perfect for matching your brand and improving usability!
