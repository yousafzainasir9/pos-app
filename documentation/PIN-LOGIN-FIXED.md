# PIN Login Fixed for Mobile App

## ✅ Changes Made

### Backend (AuthController.cs)
Updated PIN login endpoint to support customers without store:

**Before:** Required PIN + StoreId (staff only)  
**After:** 
- If `StoreId > 0` → Staff login (PIN + StoreId)
- If `StoreId = 0` → Customer login (PIN only, checks Role=Customer)

### Mobile App (LoginScreen.tsx)
Updated PIN login to call correct endpoint:

**Before:** Used regular login with PIN as username/password  
**After:** Uses `/api/auth/pin-login` with `storeId: 0`

## 🎯 API Endpoints

### Username/Password Login
```
POST /api/auth/login
{
  "username": "customer",
  "password": "Customer123!"
}
```

### PIN Login (Customers)
```
POST /api/auth/pin-login
{
  "pin": "1234",
  "storeId": 0
}
```

### PIN Login (Staff)
```
POST /api/auth/pin-login
{
  "pin": "9999",
  "storeId": 1
}
```

## 🚀 Testing

### 1. Rebuild Backend in Visual Studio
- The code changes are already saved
- Press **Shift+F5** to stop
- Press **F5** to restart with changes

### 2. Reload Mobile App
```bash
# In Metro bundler, press 'r' to reload
```

### 3. Set Breakpoint
- Open `AuthController.cs`
- Set breakpoint on line ~157 (the User? user = null; line)

### 4. Test PIN Login
- Open mobile app
- Switch to **PIN** tab
- Enter: `1234`
- Click "Login with PIN"
- Visual Studio should pause at breakpoint
- Step through and verify `storeId: 0` and customer lookup works

### 5. Check Metro Logs
Should see:
```
🚀 API Request: POST /auth/pin-login
📦 Data: {"pin":"1234","storeId":0}
✅ API Response: 200 /auth/pin-login
```

## ✅ Test Credentials

### Mobile App - Customers
| Method | Credentials |
|--------|-------------|
| Username/Password | customer / Customer123! |
| PIN | 1234 |

### Web App - Staff
| Method | Credentials | Store |
|--------|-------------|-------|
| PIN | 9999 | Any (Admin) |
| PIN | 2002 | Store 1 (Cashier) |

## 🔍 How It Works

```
Mobile App (PIN: 1234, StoreId: 0)
    ↓
POST /api/auth/pin-login
    ↓
AuthController.PinLogin()
    ↓
Check: Is storeId > 0?
    ├─ Yes → Find staff user with PIN + StoreId
    └─ No  → Find customer user with PIN + Role=Customer ✅
    ↓
Generate JWT Token
    ↓
Return user data + token
    ↓
Mobile App: Logged in! 🎉
```

## 📋 Backend Logic

```csharp
if (request.StoreId > 0)
{
    // Staff login - requires specific store
    user = await _unitOfWork.Repository<User>().Query()
        .FirstOrDefaultAsync(u => 
            u.Pin == request.Pin && 
            u.IsActive && 
            u.StoreId == request.StoreId);
}
else
{
    // Customer login - no store, just PIN
    user = await _unitOfWork.Repository<User>().Query()
        .FirstOrDefaultAsync(u => 
            u.Pin == request.Pin && 
            u.IsActive && 
            u.Role == UserRole.Customer);
}
```

## 🎯 Ready to Test!

1. ✅ Backend code updated
2. ✅ Mobile app code updated  
3. ✅ Backend running in VS (port 5021)
4. ✅ Mobile app API pointing to correct port
5. ✅ Breakpoint set in AuthController

**Now try logging in with PIN: 1234** 🚀

Your breakpoint should hit and the customer should login successfully!

---

**Expected Result:**
- Metro logs show POST to `/auth/pin-login`
- VS breakpoint hits
- User object found with Role=Customer
- Login successful
- Mobile app navigates to home screen ✅
