# Quick Testing Guide - Customer Name Fix

## 🚀 Deploy & Test

### Step 1: Restart Backend (Required)
```bash
cd D:\pos-app\backend\src\POS.WebAPI
dotnet run
```
Wait for: "Now listening on: http://localhost:5000"

### Step 2: Clear Mobile App Cache & Restart
```bash
cd D:\pos-app\mobileApp

# Method 1: Reset cache
npm start -- --reset-cache

# Method 2: Or just restart normally
npm start
```

In a NEW terminal:
```bash
cd D:\pos-app\mobileApp
npm run android
```

### Step 3: Test on Mobile App

1. **Logout if already logged in**
   - Tap logout button in top right

2. **Login with customer account**
   - Username: `customer`
   - Password: (your password)

3. **Check checkout form**
   - Add any item to cart
   - Go to Cart → Checkout
   - **Expected:** Name field shows "Sarah Garcia" ✅
   - **Before:** Was showing "Test Customer" ❌

4. **Place a test order**
   - Fill out the form (name should already be "Sarah Garcia")
   - Place order
   - Note the order number

5. **Check web dashboard**
   - Go to web dashboard orders page
   - Find your test order
   - **Expected:** Customer name shows "Sarah Garcia" ✅
   - **Matches mobile checkout form** ✅

### Step 4: Verify Staff Login (Optional)

1. **Login to POS with staff account**
   - Any cashier/admin account

2. **Verify name displays correctly**
   - Staff member's name should show (not customer name)
   - Everything should work as before

## ✅ Success Criteria

- [x] Backend compiles and runs
- [x] Mobile app compiles and runs
- [x] Customer login works
- [x] Checkout form shows "Sarah Garcia"
- [x] Order places successfully
- [x] Web dashboard shows "Sarah Garcia"
- [x] Staff login still works normally

## 🐛 Troubleshooting

### Mobile app still shows "Test Customer"

**Solution:** Clear AsyncStorage
```bash
# Uninstall the app from device/emulator
# Then reinstall:
cd D:\pos-app\mobileApp
npm run android
```

### Backend error on login

**Check:** Make sure Customer record exists in database
```sql
-- Check if CustomerId links to valid Customer
SELECT u.Id, u.Username, u.CustomerId, c.FirstName, c.LastName
FROM Users u
LEFT JOIN Customers c ON u.CustomerId = c.Id
WHERE u.Username = 'customer';
```

### Still seeing old data

**Steps:**
1. Stop backend (Ctrl+C)
2. Stop mobile app Metro bundler (Ctrl+C)
3. Clear browser cache (for web dashboard)
4. Restart backend
5. Restart mobile app with reset cache

## 📝 What Got Fixed

| Location | Before | After |
|----------|--------|-------|
| Mobile Checkout Form | "Test Customer" ❌ | "Sarah Garcia" ✅ |
| Web Dashboard Orders | "Sarah Garcia" ✅ | "Sarah Garcia" ✅ |
| Data Source | User table | Customer table |

## 💾 Database Check (Optional)

To verify the data structure:

```sql
-- Check User record
SELECT Id, Username, FirstName, LastName, CustomerId, Role
FROM Users
WHERE Id = 14;

-- Check linked Customer record
SELECT c.*
FROM Customers c
INNER JOIN Users u ON u.CustomerId = c.Id
WHERE u.Id = 14;
```

## 📚 Documentation

Full details: `CUSTOMER_NAME_FIX_IMPLEMENTED.md`
