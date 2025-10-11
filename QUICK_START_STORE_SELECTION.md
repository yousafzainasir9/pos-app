# Quick Start Guide - Store Selection Feature

## What Was Done

✅ **Fixed HTTP 307 Redirect** - Backend now accepts HTTP in development
✅ **Created Store Selection Screen** - New screen for choosing stores
✅ **Implemented Store Redux Slice** - State management for stores
✅ **Updated Navigation Flow** - Login → Store Selection → Main App
✅ **Updated Auth Logic** - Clear store on logout
✅ **Updated Stores API** - Added getStores() method

## Next Steps to Test

### 1. Rebuild the Mobile App (IMPORTANT!)

The Gradle configuration was updated, so you must rebuild:

```bash
cd D:\pos-app\mobileApp

# Clean build
cd android
./gradlew clean
cd ..

# Reinstall app
npx react-native run-android
```

### 2. Restart Backend API

Make sure the backend changes are applied:

```bash
# From Visual Studio - Stop and Start the API
# OR from command line:
cd D:\pos-app\backend\src\POS.WebAPI
dotnet run --launch-profile https
```

The API should now be running on:
- HTTP: `http://localhost:5021`
- HTTPS: `https://localhost:7021`

### 3. Test the Login Flow

1. **Open the mobile app**
2. **Click "PIN" tab**
3. **Enter PIN:** `1234`
4. **Click "Login with PIN"**
5. **You should see the Store Selection screen** ✨
6. **Select a store**
7. **You should navigate to the main app**

## Expected Behavior

### First Time Login:
```
Login Screen (PIN: 1234)
    ↓
Store Selection Screen (Choose a store)
    ↓
Main App (Home, Cart, Orders tabs)
```

### Subsequent App Opens:
```
App Start
    ↓
Main App (Store already selected, saved in AsyncStorage)
```

### After Logout:
```
Logout
    ↓
Login Screen
    ↓
Store Selection Screen (Need to select store again)
    ↓
Main App
```

## Verification Checklist

- [ ] Mobile app rebuilds without errors
- [ ] Backend API is running on port 5021 (HTTP)
- [ ] Can login with PIN 1234
- [ ] Store Selection screen appears after login
- [ ] Can see list of stores
- [ ] Can select a store
- [ ] Navigates to main app after store selection
- [ ] Store selection persists after closing/reopening app
- [ ] Store selection is cleared after logout

## If Issues Occur

### Mobile App Won't Build:
```bash
# Clear all caches
cd D:\pos-app\mobileApp
cd android
./gradlew clean
cd ..
rm -rf node_modules
npm install
npx react-native run-android
```

### Backend API Returns 307:
- Make sure you stopped and restarted the API
- Check Program.cs has the updated code
- Verify you're running in Development mode

### Store Selection Screen Not Showing:
1. Check Redux DevTools (if available)
2. Look at React Native logs: `npx react-native log-android`
3. Check if stores API returns data: Test in Postman `GET http://localhost:5021/api/stores`

### Navigation Issues:
- Make sure all new files are created
- Restart the Metro bundler: Press 'r' in terminal
- If needed, restart app completely

## Quick Test API Endpoints

### Test PIN Login:
```bash
curl -X POST http://localhost:5021/api/auth/pin-login \
  -H "Content-Type: application/json" \
  -d '{"pin":"1234","storeId":0}'
```

### Test Get Stores:
```bash
curl http://localhost:5021/api/stores
```

## File Locations

All new and modified files are in:

**New Files:**
- `D:\pos-app\mobileApp\src\screens\StoreSelectionScreen.tsx`
- `D:\pos-app\mobileApp\src\store\slices\storeSlice.ts`

**Modified Files:**
- `D:\pos-app\backend\src\POS.WebAPI\Program.cs`
- `D:\pos-app\mobileApp\android\app\build.gradle`
- `D:\pos-app\mobileApp\src\store\store.ts`
- `D:\pos-app\mobileApp\src\navigation\AppNavigator.tsx`
- `D:\pos-app\mobileApp\src\store\slices\authSlice.ts`
- `D:\pos-app\mobileApp\src\api\stores.api.ts`

## Success Indicators

When everything works correctly, you should see:

1. **Login with PIN:** Console logs show successful API call
2. **Store Selection Screen:** Shows list of stores with selection UI
3. **Store Selection:** Selected store is highlighted
4. **Navigation:** Automatically moves to main app
5. **Persistence:** Reopening app skips store selection

## Need Help?

If you encounter any issues:
1. Check the console logs in React Native debugger
2. Check backend API logs
3. Verify all files were created/modified correctly
4. Try the troubleshooting steps in STORE_SELECTION_IMPLEMENTATION.md

---

**Ready to test!** 🚀

Just rebuild the mobile app and restart the backend, then follow the test steps above.
