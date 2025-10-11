# Complete Setup Guide - Step by Step

## ⚠️ IMPORTANT: React Native Project Not Yet Initialized

I see you've run `npm install`, but the React Native project structure (android, ios folders) doesn't exist yet because we need to initialize React Native first.

## 🚀 Complete Setup Process

### Option 1: Fresh React Native Init (RECOMMENDED)

Since the Android folder doesn't exist yet, let's create a proper React Native project:

```bash
# Step 1: Go to parent directory
cd D:\pos-app

# Step 2: Backup our current folder
rename CookieBarrelMobile CookieBarrelMobile_files

# Step 3: Create React Native project
npx react-native@latest init CookieBarrelMobile --template react-native-template-typescript

# Step 4: Wait for it to complete (this takes a few minutes)
# It will create the full project structure with android and ios folders

# Step 5: Copy our src folder
xcopy CookieBarrelMobile_files\src CookieBarrelMobile\src /E /I /Y

# Step 6: Copy our documentation files
copy CookieBarrelMobile_files\*.md CookieBarrelMobile\
copy CookieBarrelMobile_files\*.bat CookieBarrelMobile\

# Step 7: Go into the new project
cd CookieBarrelMobile
```

### Step 8: Install Dependencies

```bash
npm install @react-navigation/native @react-navigation/bottom-tabs @react-navigation/native-stack
npm install react-native-screens react-native-safe-area-context
npm install @reduxjs/toolkit react-redux
npm install axios
npm install @react-native-async-storage/async-storage
npm install react-native-paper
npm install react-native-vector-icons
npm install date-fns
```

### Step 9: Configure Vector Icons

Now the `android` folder exists! Edit this file:

**File: `android/app/build.gradle`**

Find the end of the file and add this line:

```gradle
apply from: "../../node_modules/react-native-vector-icons/fonts.gradle"
```

**Full example of what the bottom of the file should look like:**

```gradle
dependencies {
    // ... other dependencies ...
}

apply from: file("../../node_modules/@react-native-community/cli-platform-android/native_modules.gradle"); applyNativeModulesAppBuildGradle(project)

// ADD THIS LINE HERE:
apply from: "../../node_modules/react-native-vector-icons/fonts.gradle"
```

### Step 10: Update index.js

Edit the root `index.js` file:

```javascript
import {AppRegistry} from 'react-native';
import App from './src/App';  // Changed from './App' to './src/App'
import {name as appName} from './app.json';

AppRegistry.registerComponent(appName, () => App);
```

### Step 11: Configure API URL

Edit `src/api/client.ts` and verify the API URL:

```typescript
// For Android Emulator (default):
const API_BASE_URL = 'http://10.0.2.2:5000/api';

// For Physical Device (uncomment and use your IP):
// const API_BASE_URL = 'http://192.168.1.XXX:5000/api';
```

To find your IP:
```bash
ipconfig
```
Look for "IPv4 Address"

### Step 12: Run the App!

```bash
# Terminal 1: Start Metro Bundler
npm start

# Terminal 2: Run Android
npx react-native run-android
```

Or use the batch file:
```bash
run-android.bat
```

---

## Option 2: Manual Android Folder Creation (NOT RECOMMENDED)

If you want to keep the current structure, you'll need to manually create the android folder structure, but this is complex and error-prone. **Option 1 is much easier!**

---

## 📝 Visual Guide: Where to Add Vector Icons Config

**File Location:** `CookieBarrelMobile/android/app/build.gradle`

**What to look for:**
```gradle
android {
    namespace "com.cookiebarrelmobile"
    // ... other config ...
}

dependencies {
    implementation("com.facebook.react:react-android")
    // ... other dependencies ...
}

// OTHER APPLY FROM STATEMENTS (DON'T REMOVE THESE):
apply from: file("../../node_modules/@react-native-community/cli-platform-android/native_modules.gradle")

// ADD THIS NEW LINE HERE ⬇️
apply from: "../../node_modules/react-native-vector-icons/fonts.gradle"
```

**Screenshot of what it should look like:**
```
Line 250: apply from: file("../../node_modules/@react-native-community/cli-platform-android/native_modules.gradle"); applyNativeModulesAppBuildGradle(project)
Line 251: 
Line 252: apply from: "../../node_modules/react-native-vector-icons/fonts.gradle"  ← ADD THIS
Line 253: 
```

---

## ✅ Verification Checklist

After completing setup:

- [ ] `android` folder exists
- [ ] `ios` folder exists (if on Mac)
- [ ] `node_modules` folder exists
- [ ] `src` folder has all our files
- [ ] `android/app/build.gradle` has vector icons line
- [ ] `index.js` imports from `./src/App`
- [ ] All dependencies installed
- [ ] Metro bundler starts without errors
- [ ] App builds and runs

---

## 🐛 Common Issues

### "Cannot find module './App'"

**Fix:** Make sure `index.js` imports from `./src/App`:
```javascript
import App from './src/App';  // Not './App'
```

### "android folder not found"

**Fix:** You need to run `npx react-native init` first (see Option 1 above)

### Vector icons show as boxes

**Fix:** 
1. Make sure the gradle line is added
2. Clean and rebuild:
   ```bash
   cd android
   ./gradlew clean
   cd ..
   npx react-native run-android
   ```

---

## 🎯 Next Steps

Once you complete these steps and the app runs successfully:

1. ✅ See login screen
2. ✅ Click "Continue as Guest"
3. ✅ See 3 tabs with icons
4. ✅ Can switch between tabs
5. ✅ No errors

Then you're ready for **Day 2: Product Display Implementation**!

---

## 💡 Quick Summary

**The issue:** React Native project not fully initialized yet (no android folder)

**The solution:** Run `npx react-native init` to create the full project structure, then copy our src files

**Then:** Add the vector icons configuration to `android/app/build.gradle`

**Finally:** Run the app with `npx react-native run-android`

---

Need help with any step? Let me know where you get stuck!
