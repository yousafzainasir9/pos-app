# Week 1 Day 1 - Setup Checklist

## ✅ Files Already Created

All these files have been created for you:

### Configuration & Setup
- [x] `src/api/client.ts` - API client
- [x] `src/constants/theme.ts` - Theme configuration
- [x] `package.json` - Dependencies list

### Type Definitions
- [x] `src/types/product.types.ts`
- [x] `src/types/order.types.ts`
- [x] `src/types/auth.types.ts`

### Redux Store
- [x] `src/store/store.ts`
- [x] `src/store/slices/cartSlice.ts`
- [x] `src/store/slices/authSlice.ts`
- [x] `src/store/slices/orderSlice.ts`

### API Services
- [x] `src/api/products.api.ts`
- [x] `src/api/orders.api.ts`
- [x] `src/api/stores.api.ts`

### Utilities
- [x] `src/utils/currency.ts`
- [x] `src/utils/validation.ts`

### Navigation
- [x] `src/navigation/AppNavigator.tsx`
- [x] `src/App.tsx`

### Screens (Placeholders)
- [x] `src/screens/HomeScreen.tsx`
- [x] `src/screens/CartScreen.tsx`
- [x] `src/screens/OrdersScreen.tsx`
- [x] `src/screens/CheckoutScreen.tsx`
- [x] `src/screens/LoginScreen.tsx`
- [x] `src/screens/OrderDetailScreen.tsx`

### Documentation
- [x] `README.md`
- [x] `SETUP_INSTRUCTIONS.md`
- [x] `setup-and-run.bat`
- [x] `run-android.bat`

---

## 🚀 What You Need To Do Now

### Step 1: Initialize React Native (Required)

The folder structure exists, but you need to initialize React Native properly:

```bash
cd D:\pos-app\CookieBarrelMobile
```

Since the src folder already exists, you have two options:

**Option A: Install dependencies in existing structure**
```bash
npm install
```

**Option B: Create fresh React Native project and copy our files**
```bash
# Go back to parent
cd D:\pos-app

# Rename current folder
rename CookieBarrelMobile CookieBarrelMobile_backup

# Create new React Native project
npx react-native@latest init CookieBarrelMobile --template react-native-template-typescript

# Copy our src folder
xcopy CookieBarrelMobile_backup\src CookieBarrelMobile\src /E /I /Y

# Copy our files
copy CookieBarrelMobile_backup\*.md CookieBarrelMobile\
copy CookieBarrelMobile_backup\*.bat CookieBarrelMobile\

cd CookieBarrelMobile
```

### Step 2: Install All Dependencies

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

### Step 3: Configure Vector Icons

Edit `android/app/build.gradle` and add this line at the END of the file:

```gradle
apply from: "../../node_modules/react-native-vector-icons/fonts.gradle"
```

### Step 4: Update index.js

Edit `index.js` in the root folder to import from our src/App:

```javascript
import {AppRegistry} from 'react-native';
import App from './src/App';
import {name as appName} from './app.json';

AppRegistry.registerComponent(appName, () => App);
```

### Step 5: Configure Backend URL

Edit `src/api/client.ts` and verify/update the API URL:

For Android Emulator (default):
```typescript
const API_BASE_URL = 'http://10.0.2.2:5000/api';
```

For Physical Device:
```bash
# Find your IP in cmd:
ipconfig

# Then update to:
const API_BASE_URL = 'http://YOUR_IP_ADDRESS:5000/api';
```

### Step 6: Start Backend

Make sure your backend is running:
```bash
cd D:\pos-app\backend
dotnet run
```

### Step 7: Run the Mobile App

```bash
# Option 1: Use the batch file
run-android.bat

# Option 2: Manual
# Terminal 1:
npm start

# Terminal 2:
npx react-native run-android
```

---

## ✅ Success Criteria

You've completed Day 1 setup when:

1. **Metro bundler starts** without errors
2. **App builds** and installs on Android emulator/device
3. **Login screen shows** with "Continue as Guest" button
4. **Can click guest login** and see the 3 tabs (Home, Cart, Orders)
5. **Can switch between tabs** successfully
6. **No red error screens** appear
7. **Console shows** no critical errors

---

## 🐛 Common Issues & Fixes

### "Cannot find module" errors
```bash
npm install
```

### Build fails
```bash
cd android
./gradlew clean
cd ..
npx react-native run-android
```

### Metro bundler cache issues
```bash
npm start -- --reset-cache
```

### App doesn't connect to backend
- Check backend is running on port 5000
- Verify API URL in `src/api/client.ts`
- For physical device, use your computer's IP
- Check Windows Firewall settings

### Vector icons don't show
- Verify you added the gradle line
- Clean and rebuild:
  ```bash
  cd android
  ./gradlew clean
  cd ..
  npx react-native run-android
  ```

---

## 📞 Next Steps

Once everything is running:

1. ✅ Verify you see the login screen
2. ✅ Click "Continue as Guest"
3. ✅ Verify you see 3 tabs at bottom
4. ✅ Verify you can switch between tabs
5. ✅ Take a screenshot if you want!

Then notify me and we'll move to **Day 2-3: Product Display Implementation**!

---

## 📊 Week 1 Progress Tracker

- [x] **Day 1**: Project Setup & Configuration
- [ ] **Day 2-3**: Product Display, Search, Filters
- [ ] **Day 4**: Shopping Cart Implementation
- [ ] **Day 5**: Login & Order Placement

**Current Status:** Day 1 Complete - Ready for Testing! 🎉
