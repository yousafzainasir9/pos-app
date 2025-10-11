# Store Selection After Login - Implementation Guide

## Problem
1. PIN login was sending `storeId: 0` which was correct for customer login
2. After successful login, the app wasn't navigating properly
3. User needs to select a store after logging in

## Solution
Implemented a new flow where users:
1. Login with PIN (no store required)
2. Select their preferred store from a list
3. Then access the main app

## Changes Made

### 1. Created Store Selection Screen
**File:** `D:\pos-app\mobileApp\src\screens\StoreSelectionScreen.tsx`

New screen that:
- Displays all active stores in a scrollable list
- Shows store details (name, address, phone)
- Allows user to select a store
- Saves selection to AsyncStorage
- Shows visual feedback for selected store

### 2. Created Store Slice (Redux)
**File:** `D:\pos-app\mobileApp\src\store\slices\storeSlice.ts`

Features:
- Fetches stores from API
- Manages selected store state
- Persists selection to AsyncStorage
- Restores selection on app restart
- Clear selection on logout

### 3. Updated Redux Store Configuration
**File:** `D:\pos-app\mobileApp\src\store\store.ts`

Added `storeReducer` to the Redux store configuration.

### 4. Updated Navigation Flow
**File:** `D:\pos-app\mobileApp\src\navigation\AppNavigator.tsx`

New navigation logic:
```
Not Authenticated → Login Screen
    ↓
Authenticated but No Store → Store Selection Screen
    ↓
Authenticated with Store → Main App (Tabs)
```

Added:
- Import for StoreSelectionScreen
- Import for store slice actions
- StoreSelection to route types
- Conditional rendering based on `selectedStoreId`
- Restore selected store on app start
- Prevent back navigation from store selection

### 5. Updated Auth Slice
**File:** `D:\pos-app\mobileApp\src\store\slices\authSlice.ts`

- Clear selected store ID from AsyncStorage on logout

### 6. Updated Stores API
**File:** `D:\pos-app\mobileApp\src\api\stores.api.ts`

- Added `getStores()` method to fetch all stores

## Backend API (Already Working)

### PIN Login Endpoint
`POST /api/auth/pin-login`

**Request:**
```json
{
  "pin": "1234",
  "storeId": 0
}
```

**Response:**
```json
{
  "success": true,
  "message": "PIN login successful",
  "data": {
    "token": "jwt_token_here",
    "refreshToken": "refresh_token_here",
    "expiresIn": 86400,
    "user": {
      "id": 1,
      "username": "customer",
      "email": "customer@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "role": "Customer",
      "storeId": null,
      "storeName": null,
      "customerId": 1,
      "phone": "0400000000"
    }
  }
}
```

The backend correctly handles `storeId: 0` for customers by looking up users with `Role == Customer` and matching PIN only.

### Stores Endpoint
`GET /api/stores`

Returns list of all active stores.

## User Flow

### Customer Login Flow:
1. **Login Screen**
   - User enters 4-digit PIN
   - Clicks "Login with PIN"
   - PIN is sent with `storeId: 0`

2. **Store Selection Screen** (New!)
   - Shows list of available stores
   - User selects preferred store
   - Selection is saved locally
   - Automatically navigates to main app

3. **Main App**
   - User can browse products
   - Add items to cart
   - Place orders
   - View order history

### Guest Login Flow:
1. **Login Screen**
   - User clicks "Continue as Guest"
   - Skips directly to main app (no store selection required)

## Testing

### Test the PIN Login:
1. Run the backend API
2. Run the mobile app
3. Click "PIN" tab on login screen
4. Enter PIN: `1234`
5. Click "Login with PIN"
6. Should see Store Selection Screen
7. Select a store
8. Should navigate to main app

### Test Store Selection:
1. After logging in, you should see the Store Selection screen
2. Verify all active stores are displayed
3. Select a store
4. Verify the selected store is highlighted
5. Verify you can now access the main app
6. Close and reopen the app
7. Should skip store selection (stored in AsyncStorage)

### Test Logout:
1. Navigate to profile/settings
2. Logout
3. Login again
4. Should show Store Selection screen again (cleared on logout)

## Files Created
- `D:\pos-app\mobileApp\src\screens\StoreSelectionScreen.tsx` - New screen
- `D:\pos-app\mobileApp\src\store\slices\storeSlice.ts` - New Redux slice

## Files Modified
- `D:\pos-app\mobileApp\src\store\store.ts` - Added store reducer
- `D:\pos-app\mobileApp\src\navigation\AppNavigator.tsx` - Updated navigation logic
- `D:\pos-app\mobileApp\src\store\slices\authSlice.ts` - Clear store on logout
- `D:\pos-app\mobileApp\src\api\stores.api.ts` - Added getStores method

## Database Notes

The backend correctly handles customer login:
- Customers don't have a `storeId` in their user record
- PIN login with `storeId: 0` looks for customers by PIN only
- After login, customer selects preferred store in the app
- Store selection is saved locally (not sent to backend)

## Future Enhancements

1. **Store-Specific Features:**
   - Show products available at selected store
   - Show store hours and availability
   - Allow switching stores without logout

2. **Store Search:**
   - Add search functionality to find stores
   - Filter by location/distance
   - Show store on map

3. **Remember Last Store:**
   - Auto-select last used store
   - Add "Change Store" option in profile

4. **Store Details:**
   - Show store images
   - Show operating hours
   - Show available services

## Troubleshooting

### Store Selection Screen Not Showing:
- Check Redux state: `console.log(selectedStoreId)`
- Verify user is authenticated: `console.log(isAuthenticated)`
- Check stores API is responding: Test in Postman

### Stores Not Loading:
- Check API endpoint: `http://10.0.2.2:5021/api/stores`
- Verify backend is running
- Check network logs in React Native debugger

### Navigation Not Working:
- Verify all screens are imported correctly
- Check navigation conditions in AppNavigator
- Ensure Redux state is updating properly

### AsyncStorage Issues:
- Clear app data and reinstall
- Check AsyncStorage keys: `authToken`, `refreshToken`, `user`, `selectedStoreId`

## Summary

The new flow improves the user experience by:
1. ✅ Allowing PIN login without requiring store upfront
2. ✅ Letting users choose their preferred store after login
3. ✅ Persisting store selection for convenience
4. ✅ Maintaining security (only authenticated users can select stores)
5. ✅ Supporting both customer and guest flows

The implementation is complete and ready for testing!
