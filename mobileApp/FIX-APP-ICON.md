# Quick Fix: Replace App Icon

## The Problem
The XML adaptive icon isn't working without proper PNG foreground images.

## Quick Solution

### Step 1: Disable Adaptive Icons (Temporarily)
```bash
cd D:\pos-app\mobileApp\android\app\src\main\res

# Rename these files to disable them
ren mipmap-anydpi-v26\ic_launcher.xml ic_launcher.xml.disabled
ren mipmap-anydpi-v26\ic_launcher_round.xml ic_launcher_round.xml.disabled
```

### Step 2: Uninstall Old App
```bash
adb uninstall com.mobileapp
```

### Step 3: Rebuild
```bash
cd D:\pos-app\mobileApp
npm run android
```

This will use the default PNG icons already in the mipmap folders.

---

## To Get a Proper Cookie Barrel Icon

### Use Icon.Kitchen (Easiest - 2 minutes):

1. Go to: **https://icon.kitchen/**

2. **Create Icon:**
   - Click "Icon" tab
   - Click "Text" and type "CB"
   - Set background color: `#D97706`
   - Set text color: `#FFFFFF`
   - Choose "Circle" or "Square" shape

3. **Download:**
   - Click "Download"
   - Select "Android"
   - Save the ZIP file

4. **Install:**
   ```bash
   # Extract the ZIP
   # Copy all folders from the zip to:
   D:\pos-app\mobileApp\android\app\src\main\res\
   # Replace all existing mipmap folders
   ```

5. **Rebuild:**
   ```bash
   adb uninstall com.mobileapp
   npm run android
   ```

---

## Alternative: Android Asset Studio

1. Go to: **https://romannurik.github.io/AndroidAssetStudio/icons-launcher.html**

2. **Create:**
   - Choose "Text" option
   - Enter "CB"
   - Set color scheme
   - Download ZIP

3. **Install the same way as above**

---

## For Now: Let's See the Login Screen Logo

The app **login screen logo** should work fine! The app icon issue is separate.

**Run the app and check the login screen:**
```bash
cd D:\pos-app\mobileApp
npm run android
```

The login screen should show the nice "CB" logo even if the app icon isn't perfect yet.

Screenshot the login screen and let me know if that logo looks good! 📱
