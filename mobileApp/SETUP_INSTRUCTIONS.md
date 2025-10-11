# Cookie Barrel Mobile App - Setup Instructions

## ✅ Files Created

All the foundational files have been created in `D:\pos-app\CookieBarrelMobile\src\`

### Project Structure:
```
CookieBarrelMobile/
├── src/
│   ├── api/
│   │   ├── client.ts              ✅ API client with interceptors
│   │   ├── products.api.ts        ✅ Products API service
│   │   ├── orders.api.ts          ✅ Orders API service
│   │   └── stores.api.ts          ✅ Stores API service
│   ├── components/
│   │   ├── common/                ✅ Common components folder
│   │   ├── products/              ✅ Product components folder
│   │   └── orders/                ✅ Order components folder
│   ├── screens/
│   │   ├── HomeScreen.tsx         ✅ Home/Products screen
│   │   ├── CartScreen.tsx         ✅ Shopping cart screen
│   │   ├── OrdersScreen.tsx       ✅ Orders list screen
│   │   ├── CheckoutScreen.tsx     ✅ Checkout screen
│   │   ├── LoginScreen.tsx        ✅ Login/Guest screen
│   │   └── OrderDetailScreen.tsx  ✅ Order details screen
│   ├── store/
│   │   ├── store.ts               ✅ Redux store configuration
│   │   └── slices/
│   │       ├── cartSlice.ts       ✅ Cart state management
│   │       ├── authSlice.ts       ✅ Auth state management
│   │       └── orderSlice.ts      ✅ Order state management
│   ├── navigation/
│   │   └── AppNavigator.tsx       ✅ Navigation setup
│   ├── types/
│   │   ├── product.types.ts       ✅ Product type definitions
│   │   ├── order.types.ts         ✅ Order type definitions
│   │   └── auth.types.ts          ✅ Auth type definitions
│   ├── utils/
│   │   ├── currency.ts            ✅ Currency formatting utilities
│   │   └── validation.ts          ✅ Validation utilities
│   ├── constants/
│   │   └── theme.ts               ✅ Theme configuration
│   └── App.tsx                    ✅ Main app component
└── package.json                   ✅ Package configuration
```

## 🚀 Next Steps - Complete React Native Setup

### Step 1: Initialize React Native Project

Since we've created the `src` folder structure, you now need to initialize React Native properly:

```bash
# Open PowerShell or Command Prompt
cd D:\pos-app

# Remove the CookieBarrelMobile folder temporarily
# (We'll move our src folder back in after initialization)

# Create React Native project
npx react-native@latest init CookieBarrelMobile --template react-native-template-typescript

# Wait for it to complete...
```

### Step 2: Replace Default Files

After React Native initialization completes:

```bash
cd CookieBarrelMobile

# The src folder we created has all the files
# React Native will have created its own default src/App.tsx
# Our files are already in place, so you just need to install dependencies
```

### Step 3: Install Dependencies

```bash
# Navigation
npm install @react-navigation/native @react-navigation/bottom-tabs @react-navigation/native-stack
npm install react-native-screens react-native-safe-area-context

# State Management
npm install @reduxjs/toolkit react-redux

# API & Storage
npm install axios
npm install @react-native-async-storage/async-storage

# UI Components
npm install react-native-paper
npm install react-native-vector-icons

# Date handling
npm install date-fns
```

### Step 4: Configure Vector Icons (Android)

Edit `android/app/build.gradle` and add at the bottom:

```gradle
apply from: "../../node_modules/react-native-vector-icons/fonts.gradle"
```

### Step 5: Update index.js

Edit `index.js` in the root to import from our App:

```javascript
import {AppRegistry} from 'react-native';
import App from './src/App';
import {name as appName} from './app.json';

AppRegistry.registerComponent(appName, () => App);
```

### Step 6: Configure API URL

Edit `src/api/client.ts` and update the API_BASE_URL:

```typescript
// For Android Emulator (default):
const API_BASE_URL = 'http://10.0.2.2:5000/api';

// For Physical Device, find your computer's IP:
// Run 'ipconfig' in cmd and use your IPv4 Address:
// const API_BASE_URL = 'http://192.168.1.XXX:5000/api';
```

### Step 7: Test Run

```bash
# Start Metro bundler in one terminal
npm start

# In another terminal, run Android
npx react-native run-android
```

## ✅ What You Should See

After successful setup, you should see:
1. **Login Screen** - with "Continue as Guest" button
2. After clicking guest login:
   - **Home Tab** - "Products Coming Soon"
   - **Cart Tab** - "Cart Coming Soon"
   - **Orders Tab** - "Orders Coming Soon"
3. **Navigation working** - can switch between tabs
4. **No errors** in console

## 🔧 Troubleshooting

### Metro Bundler Issues
```bash
npm start -- --reset-cache
```

### Build Errors
```bash
cd android
./gradlew clean
cd ..
npx react-native run-android
```

### Cannot Find Module Errors
```bash
npm install
cd android
./gradlew clean
cd ..
```

### Vector Icons Not Showing
Make sure you added this to `android/app/build.gradle`:
```gradle
apply from: "../../node_modules/react-native-vector-icons/fonts.gradle"
```

Then rebuild:
```bash
npx react-native run-android
```

## 📱 Testing Connection to Backend

1. Make sure your backend is running on `http://localhost:5000`
2. The app is configured to connect to it via:
   - Emulator: `http://10.0.2.2:5000` (maps to localhost)
   - Physical Device: Update to your computer's IP

## 🎯 Week 1 Progress

### Day 1: ✅ COMPLETE
- [x] Project structure created
- [x] All configuration files created
- [x] Redux store setup
- [x] API services created
- [x] Navigation setup
- [x] Placeholder screens created

### Day 2-3: NEXT
Once the app is running, we'll implement:
- Product listing
- Product search and filters
- Category filters
- Product cards with images

## 📞 Need Help?

If you encounter any issues:
1. Check the troubleshooting section above
2. Run `npx react-native doctor` to diagnose setup issues
3. Check Metro bundler logs for specific errors
4. Ensure Android Studio and emulator are properly set up

## 🎉 Ready for Day 2?

Once you see the app running with the login screen and can click "Continue as Guest" to see the tabs, you're ready for Day 2 where we'll build the actual product browsing functionality!

---

**Next Task:** Run through steps 1-7 above to complete the React Native setup, then let me know when you're ready to implement the Product Display features (Day 2-3).
