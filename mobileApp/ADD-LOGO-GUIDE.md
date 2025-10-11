# Adding Logo to Cookie Barrel Mobile App

## Overview
This guide will help you add the Cookie Barrel logo to your Android mobile app, including:
- App icon (launcher icon)
- Login screen logo
- Splash screen

## Step 1: Prepare Logo Assets

You need logo images in different sizes for Android. Here's what you need:

### Required Logo Sizes

| Density | Size (px) | Folder |
|---------|-----------|--------|
| mdpi    | 48x48     | mipmap-mdpi |
| hdpi    | 72x72     | mipmap-hdpi |
| xhdpi   | 96x96     | mipmap-xhdpi |
| xxhdpi  | 144x144   | mipmap-xxhdpi |
| xxxhdpi | 192x192   | mipmap-xxxhdpi |

### Logo for Login Screen
- **Size:** 512x512 px (high resolution)
- **Format:** PNG with transparency
- **Location:** `mobileApp/src/assets/images/logo.png`

## Step 2: Generate App Icons

### Option A: Using Android Asset Studio (Recommended)
1. Go to: https://romannurik.github.io/AndroidAssetStudio/icons-launcher.html
2. Upload your logo image
3. Configure:
   - Name: `ic_launcher`
   - Shape: Circle or Square (your choice)
   - Background: White or brand color
4. Download the generated ZIP
5. Extract and copy all folders to: `mobileApp/android/app/src/main/res/`

### Option B: Manual Creation
If you have a logo file, use an image editor to create these sizes:

**For App Icon:**
```
📁 mobileApp/android/app/src/main/res/
  📁 mipmap-mdpi/
    📄 ic_launcher.png (48x48)
    📄 ic_launcher_round.png (48x48)
  📁 mipmap-hdpi/
    📄 ic_launcher.png (72x72)
    📄 ic_launcher_round.png (72x72)
  📁 mipmap-xhdpi/
    📄 ic_launcher.png (96x96)
    📄 ic_launcher_round.png (96x96)
  📁 mipmap-xxhdpi/
    📄 ic_launcher.png (144x144)
    📄 ic_launcher_round.png (144x144)
  📁 mipmap-xxxhdpi/
    📄 ic_launcher.png (192x192)
    📄 ic_launcher_round.png (192x192)
```

### Option C: Using Online Tool
1. Go to: https://icon.kitchen/
2. Upload your logo
3. Select "Android" platform
4. Download and extract to the res folder

## Step 3: Create Assets Folder

```bash
# Create assets folder structure
cd D:\pos-app\mobileApp
mkdir -p src\assets\images
```

Place your logo file (512x512 or larger) as:
```
src/assets/images/logo.png
```

## Step 4: Install React Native Image Package (if needed)

The logo.png will be loaded using `require()`, which is already supported in React Native.

## Step 5: Update Login Screen with Logo

The LoginScreen has already been updated with logo support. The code looks like this:

```typescript
import { Image } from 'react-native';

// In the render:
<Image 
  source={require('../assets/images/logo.png')} 
  style={styles.logo}
  resizeMode="contain"
/>
```

## Step 6: Create Splash Screen (Optional)

### Create splash screen drawable

**File:** `android/app/src/main/res/drawable/splash_background.xml`

```xml
<?xml version="1.0" encoding="utf-8"?>
<layer-list xmlns:android="http://schemas.android.com/apk/res/android">
    <item android:drawable="@color/splash_background"/>
    <item>
        <bitmap
            android:gravity="center"
            android:src="@drawable/splash_logo"/>
    </item>
</layer-list>
```

**File:** `android/app/src/main/res/values/colors.xml`

```xml
<?xml version="1.0" encoding="utf-8"?>
<resources>
    <color name="splash_background">#FFFFFF</color>
    <color name="primary_color">#D97706</color>
</resources>
```

Place your splash logo (400x400 px) at:
```
android/app/src/main/res/drawable/splash_logo.png
```

## Step 7: Quick Setup with Placeholder

If you don't have a logo yet, here's a quick temporary solution:

### Create a simple text-based logo placeholder:

**File:** `android/app/src/main/res/drawable/logo_placeholder.xml`

```xml
<?xml version="1.0" encoding="utf-8"?>
<shape xmlns:android="http://schemas.android.com/apk/res/android"
    android:shape="oval">
    <solid android:color="#D97706"/>
    <size
        android:width="120dp"
        android:height="120dp"/>
</shape>
```

## Step 8: Testing

### Rebuild the app
```bash
cd D:\pos-app\mobileApp

# Clean build
cd android
./gradlew clean
cd ..

# Rebuild
npm run android
```

### Check the logo appears:
1. ✅ App icon in launcher
2. ✅ Logo on login screen
3. ✅ Splash screen (if implemented)

## Recommended Logo Specifications

### For Best Results:
- **File Format:** PNG (with transparency) or SVG
- **Size:** At least 512x512 px
- **Aspect Ratio:** Square (1:1)
- **Background:** Transparent or white
- **Style:** Simple, recognizable at small sizes
- **Colors:** Match your brand (Cookie Barrel orange/brown)

### Cookie Barrel Logo Suggestions:
- Cookie icon with company name
- Barrel with cookies
- Stylized "CB" monogram
- Cookie with bite taken out

## Using an Existing Logo

If you have a Cookie Barrel logo from the web app:

1. **Find the logo file** in the web frontend
2. **Convert to appropriate sizes** using:
   - Photoshop
   - GIMP
   - Online tools like ResizeImage.net
3. **Copy to mobile app** following the structure above

## Troubleshooting

### Logo not appearing on Login Screen
```bash
# Clear Metro bundler cache
npm start -- --reset-cache

# Rebuild
npm run android
```

### App icon not updating
```bash
# Uninstall app from emulator
adb uninstall com.mobileapp

# Rebuild
npm run android
```

### Image not found error
- Check file path: `src/assets/images/logo.png`
- Verify file name case sensitivity
- Ensure image file exists

## Next Steps

1. ✅ Add logo assets to all required folders
2. ✅ Update login screen to display logo
3. ⬜ Add splash screen with logo
4. ⬜ Add logo to app header/navigation
5. ⬜ Add loading animation with logo

## Resources

- **Android Asset Studio:** https://romannurik.github.io/AndroidAssetStudio/
- **Icon Kitchen:** https://icon.kitchen/
- **App Icon Generator:** https://www.appicon.co/
- **Image Resizer:** https://www.resizeimage.net/

---

**Need a logo designed?** Consider using:
- Canva (free templates)
- Figma (design tool)
- Hire a designer on Fiverr
- Use AI tools like DALL-E or Midjourney

Once you have your logo files ready, let me know and I'll help you integrate them!
