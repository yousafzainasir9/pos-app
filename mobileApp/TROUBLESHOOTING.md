# 🔧 Troubleshooting Guide - Cookie Barrel Mobile

## Common Setup Issues on Windows

### Issue 1: "Cannot find module" errors

**Symptoms:**
- Red error screen
- "Cannot find module '@react-navigation/native'"
- Metro bundler crashes

**Solution:**
```bash
# Delete node_modules and reinstall
rm -rf node_modules
npm install

# Or on Windows:
rmdir /s node_modules
npm install
```

---

### Issue 2: "Unable to resolve module"

**Symptoms:**
- "Unable to resolve module `react-native-vector-icons`"
- Icons don't show

**Solution:**
```bash
# Make sure you added to android/app/build.gradle:
apply from: "../../node_modules/react-native-vector-icons/fonts.gradle"

# Then clean and rebuild:
cd android
./gradlew clean
cd ..
npx react-native run-android
```

---

### Issue 3: Build fails with Gradle errors

**Symptoms:**
- Build fails during Android compilation
- "Could not resolve all files for configuration"

**Solution:**
```bash
# Clean Gradle cache
cd android
./gradlew clean
./gradlew cleanBuildCache

# Go back and rebuild
cd ..
npx react-native run-android
```

---

### Issue 4: Metro bundler won't start

**Symptoms:**
- Port 8081 already in use
- "Something is already running on port 8081"

**Solution:**
```bash
# Find and kill process on port 8081
netstat -ano | findstr :8081
taskkill /PID <PID_NUMBER> /F

# Or use a different port:
npm start -- --port 8082
```

---

### Issue 5: App crashes on launch

**Symptoms:**
- App installs but crashes immediately
- Red error screen on device

**Solution:**
```bash
# Clear Metro cache
npm start -- --reset-cache

# In another terminal:
npx react-native run-android
```

---

### Issue 6: "SDK location not found"

**Symptoms:**
- "SDK location not found. Define location with sdk.dir..."

**Solution:**
Create or edit `android/local.properties`:
```properties
sdk.dir=C:\\Users\\YourUsername\\AppData\\Local\\Android\\Sdk
```

Replace `YourUsername` with your actual Windows username.

---

### Issue 7: Cannot connect to backend

**Symptoms:**
- Network request failed
- Timeout errors
- Cannot fetch products

**Solution:**

For **Android Emulator**:
```typescript
// src/api/client.ts
const API_BASE_URL = 'http://10.0.2.2:5000/api';
```

For **Physical Device**:
```bash
# Find your computer's IP:
ipconfig
# Look for IPv4 Address: 192.168.1.XXX

# Then update src/api/client.ts:
const API_BASE_URL = 'http://192.168.1.XXX:5000/api';
```

**Also check:**
1. Backend is running: `cd backend && dotnet run`
2. Firewall allows connections
3. Device and computer on same network

---

### Issue 8: TypeScript errors

**Symptoms:**
- Red squiggly lines in VS Code
- Type errors in terminal

**Solution:**
```bash
# Install TypeScript types
npm install --save-dev @types/react @types/react-native

# Restart TypeScript server in VS Code:
# Ctrl+Shift+P > "TypeScript: Restart TS Server"
```

---

### Issue 9: "watchman" errors

**Symptoms:**
- "Watchman crawl failed"
- File watching issues

**Solution:**
```bash
# Clear watchman cache
watchman watch-del-all

# Restart Metro
npm start -- --reset-cache
```

If watchman not installed (Windows):
```bash
# Watchman is optional on Windows
# You can ignore these warnings
```

---

### Issue 10: Slow Metro bundler

**Symptoms:**
- Metro bundler takes forever
- App loading is slow

**Solution:**
```bash
# Clear Metro cache
npm start -- --reset-cache

# Clear all caches
cd android
./gradlew clean
cd ..
rm -rf node_modules
npm install
npm start
```

---

### Issue 11: "Unable to boot simulator"

**Symptoms:**
- Android emulator won't start
- Emulator crashes

**Solution:**

1. Open Android Studio
2. Go to Tools > Device Manager
3. Delete old emulator
4. Create new emulator:
   - Pixel 5 or newer
   - API Level 30 or higher
   - x86_64 architecture

---

### Issue 12: Hot reload not working

**Symptoms:**
- Changes don't appear
- Need to rebuild every time

**Solution:**
```bash
# Enable Fast Refresh in app:
# Shake device or Ctrl+M in emulator
# Enable "Fast Refresh"

# Or restart with cache clear:
npm start -- --reset-cache
```

---

### Issue 13: "Command not found: react-native"

**Symptoms:**
- `react-native: command not found`

**Solution:**
```bash
# Use npx instead:
npx react-native run-android

# Or install globally:
npm install -g react-native-cli
```

---

### Issue 14: Icons show as squares/boxes

**Symptoms:**
- Tab icons are squares
- Vector icons don't display

**Solution:**
```bash
# Make sure this is in android/app/build.gradle:
apply from: "../../node_modules/react-native-vector-icons/fonts.gradle"

# Rebuild:
cd android
./gradlew clean
cd ..
npx react-native run-android
```

---

### Issue 15: "Java version" errors

**Symptoms:**
- "Unsupported class file major version"
- Java version conflicts

**Solution:**
```bash
# Install Java JDK 17
# Download from: https://adoptium.net/

# Set JAVA_HOME environment variable:
# System Properties > Environment Variables
# Add: JAVA_HOME = C:\Program Files\Eclipse Adoptium\jdk-17.x.x
```

---

## 🚨 Emergency Reset

If nothing works and you want to start fresh:

```bash
# 1. Clean everything
cd D:\pos-app\CookieBarrelMobile
rm -rf node_modules
rm -rf android/build
rm -rf android/app/build

# 2. Clear Metro cache
npm start -- --reset-cache
# Press Ctrl+C to stop

# 3. Reinstall
npm install

# 4. Clean Gradle
cd android
./gradlew clean
cd ..

# 5. Run fresh
npx react-native run-android
```

---

## 📞 Still Having Issues?

### Check React Native Doctor
```bash
npx react-native doctor
```

This will diagnose common setup problems.

### View Logs
```bash
# Android logs:
npx react-native log-android

# Metro bundler logs:
# Already visible in the npm start terminal
```

### Check Android Logcat
```bash
# In Android Studio:
# View > Tool Windows > Logcat
# Filter by "ReactNative" or your app name
```

---

## ✅ Verification Steps

Before asking for help, verify:

1. [ ] Node.js 18+ installed (`node --version`)
2. [ ] npm working (`npm --version`)
3. [ ] Android Studio installed
4. [ ] Java JDK 17 installed (`java --version`)
5. [ ] Android SDK installed (check Android Studio)
6. [ ] Emulator created and working
7. [ ] All dependencies installed (`npm install`)
8. [ ] Vector icons configured in build.gradle
9. [ ] Backend running (if testing API calls)
10. [ ] Correct API URL in client.ts

---

## 💡 Pro Tips

1. **Always check Metro bundler output** - errors appear there first
2. **Use Android Studio Logcat** - shows native crashes
3. **Clear cache when in doubt** - `npm start -- --reset-cache`
4. **Restart emulator** - sometimes it's just stuck
5. **Check file paths** - Windows uses backslashes
6. **One change at a time** - easier to debug
7. **Read error messages carefully** - they usually tell you what's wrong

---

## 🎯 Getting Help

When reporting issues, provide:

1. **Exact error message** - copy/paste from terminal
2. **What you tried** - what steps you've taken
3. **Your environment** - OS, Node version, React Native version
4. **When it happens** - during build, runtime, etc.
5. **Screenshots** - if applicable

**Example:**
```
Error: Unable to resolve module `react-redux`
Environment: Windows 11, Node 18.0.0, RN 0.73
Tried: npm install, clearing cache
Happens: During Metro bundler start
```

This helps diagnose the issue faster!

---

**Remember:** Most issues are fixable with a clean cache and rebuild. Don't give up! 🚀
