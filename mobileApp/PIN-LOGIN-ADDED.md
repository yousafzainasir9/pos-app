# PIN Login Added to Mobile App

## ✅ What Was Added

1. **PIN Login Mode** - Toggle between Username/Password and PIN login
2. **Store Selector** - Dropdown to select store for PIN login
3. **4-Digit PIN Input** - Numeric keypad with secure entry
4. **Test Credentials Display** - Shows test PINs for easy testing

## 📦 Install Required Package

The Picker component needs to be installed:

```bash
cd D:\pos-app\mobileApp

# Install React Native Picker
npm install @react-native-picker/picker

# Link native dependencies (if needed)
npx react-native link @react-native-picker/picker
```

## 🚀 Quick Start

### 1. Install Dependencies
```bash
cd D:\pos-app\mobileApp
npm install @react-native-picker/picker
```

### 2. Rebuild App
```bash
# Start Metro
npm start -- --reset-cache

# In another terminal
npx react-native run-android
```

### 3. Test PIN Login

**Switch to PIN mode**, then:

**Store 1 (Cookie Barrel Main):**
- Admin PIN: `9999`
- Cashier PIN: `2002`

**Store 2 (Cookie Barrel Westfield):**
- Admin PIN: `9999`  
- Cashier PIN: `2005`

**Store 3 (Cookie Barrel Airport):**
- Admin PIN: `9999`
- Cashier PIN: `2008`

## 🎨 Features

### Login Modes
- **Username Tab** - Traditional login with username/password
- **PIN Tab** - Quick login with PIN and store selection

### PIN Login Flow
1. Select store from dropdown
2. Enter 4-digit PIN
3. Click "Login with PIN"
4. Authenticated!

### Validation
- PIN must be exactly 4 digits
- Only numeric input allowed
- Store must be selected
- Real-time error feedback

## 🔧 How It Works

### API Endpoints Used
```typescript
// Username login
POST /api/auth/login
{ username, password }

// PIN login  
POST /api/auth/pin-login
{ pin, storeId }
```

### Redux Actions
```typescript
loginUser({ username, password })    // Username login
pinLoginUser({ pin, storeId })       // PIN login
```

### Store Loading
- Stores loaded from: `GET /api/stores`
- Cached in component state
- Auto-selects first store

## 📱 UI/UX

### Mode Toggle
```
┌─────────────────────────┐
│ [Username] [  PIN  ]    │  ← Segmented control
└─────────────────────────┘
```

### Username Mode
- Username input
- Password input (secure)
- Login button
- Test credentials hint

### PIN Mode
- Store dropdown
- 4-digit PIN input (numeric keypad)
- Login with PIN button
- Staff PIN hints

## 🧪 Testing Checklist

- [ ] Install @react-native-picker/picker
- [ ] Rebuild app
- [ ] Backend API running
- [ ] Test username login (customer / Customer123!)
- [ ] Switch to PIN tab
- [ ] Select store from dropdown
- [ ] Test admin PIN (9999)
- [ ] Test cashier PIN (2002)
- [ ] Verify error messages work
- [ ] Test guest mode still works

## 🐛 Troubleshooting

### "Cannot find module '@react-native-picker/picker'"
```bash
npm install @react-native-picker/picker
npx react-native run-android
```

### Picker not showing or crashing
```bash
cd android
./gradlew clean
cd ..
npm run android
```

### "No stores available"
- Check backend API is running
- Verify stores exist in database
- Check API URL in client.ts

### PIN login fails
- Verify store is selected
- Check PIN is exactly 4 digits
- Ensure user exists for that store
- Check backend logs

## 📝 Default PINs Reference

| Role | PIN | Stores |
|------|-----|--------|
| Admin | 9999 | All stores |
| Manager 1 | 1001 | Store 1 |
| Manager 2 | 1002 | Store 2 |
| Manager 3 | 1003 | Store 3 |
| Cashier 2 | 2002 | Store 1 |
| Cashier 3 | 2003 | Store 1 |
| Cashier 4 | 2004 | Store 1 |
| Cashier 5 | 2005 | Store 2 |
| Cashier 6 | 2006 | Store 2 |
| Cashier 7 | 2007 | Store 2 |
| Cashier 8 | 2008 | Store 3 |
| Cashier 9 | 2009 | Store 3 |
| Cashier 10 | 2010 | Store 3 |

## 🎯 Next Steps

1. ✅ PIN login implemented
2. ✅ Store selection added
3. ⬜ Test on physical device
4. ⬜ Add biometric authentication
5. ⬜ Add "Remember Store" preference
6. ⬜ Add PIN visibility toggle

---

**Ready to test!** Install the picker package and rebuild the app! 🚀
