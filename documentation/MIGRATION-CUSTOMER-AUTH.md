# Quick Start: Customer Authentication Migration

## Step 1: Create Database Migration

```bash
cd D:\pos-app\backend\src\POS.Infrastructure

# Create the migration
dotnet ef migrations add AddCustomerIdToUser -s ..\POS.WebAPI

# You should see output like:
# Build started...
# Build succeeded.
# Done. To undo this action, use 'ef migrations remove'
```

## Step 2: Apply Migration

### Option A: Using Migrator (Recommended)
```bash
cd D:\pos-app\backend\src\POS.Migrator
dotnet run
```

### Option B: Using EF Core Directly
```bash
cd D:\pos-app\backend\src\POS.WebAPI
dotnet ef database update --project ..\POS.Infrastructure
```

## Step 3: Verify Migration

After running the migrator, you should see:

```
========================================
   DATABASE SETUP COMPLETED ✅
========================================

Default Credentials:

📱 Web App (Admin/Staff):
  Admin:    admin / Admin123!
  Manager:  manager1 / Manager123!
  Cashier:  cashier2 / Cashier123!

📱 Mobile App (Customers):
  Customer: customer / Customer123!
```

## Step 4: Test Backend Login

### Using cURL:
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "customer",
    "password": "Customer123!"
  }'
```

### Using Postman:
1. POST to `http://localhost:5000/api/auth/login`
2. Body (JSON):
   ```json
   {
     "username": "customer",
     "password": "Customer123!"
   }
   ```
3. Should return:
   ```json
   {
     "success": true,
     "data": {
       "token": "eyJhbG...",
       "user": {
         "role": "Customer",
         "customerId": 2
       }
     }
   }
   ```

## Step 5: Test Mobile App

```bash
cd D:\pos-app\mobileApp

# Start Metro bundler
npm start

# In another terminal, run the app
npm run android
# or
npm run ios
```

**Login Steps:**
1. Open app
2. Enter username: `customer`
3. Enter password: `Customer123!`
4. Tap "Login"
5. Should navigate to Home screen
6. Close app and reopen - should stay logged in

## Troubleshooting

### Error: "A migration for 'AddCustomerIdToUser' already exists"
```bash
# Remove the last migration
cd D:\pos-app\backend\src\POS.Infrastructure
dotnet ef migrations remove -s ..\POS.WebAPI
```

### Error: "The entity type 'User' requires a primary key"
This means there's an issue with the User entity. Check that:
- `User` inherits from `BaseEntity`
- `BaseEntity` has an `Id` property

### Mobile App: "Cannot connect to server"
Update the API URL for your device:

**Android Emulator:**
```typescript
// src/api/client.ts
const API_BASE_URL = 'http://10.0.2.2:5000/api';
```

**iOS Simulator:**
```typescript
const API_BASE_URL = 'http://localhost:5000/api';
```

**Physical Device:**
```typescript
// Use your computer's local IP
const API_BASE_URL = 'http://192.168.1.100:5000/api';
```

## Quick Commands Reference

```bash
# Backend - Run API
cd D:\pos-app\backend\src\POS.WebAPI
dotnet run

# Backend - Run Migrator
cd D:\pos-app\backend\src\POS.Migrator
dotnet run

# Mobile - Start
cd D:\pos-app\mobileApp
npm start

# Mobile - Android
npm run android

# Mobile - iOS
npm run ios

# Mobile - Clear cache
npm start -- --reset-cache
```

## Files to Check After Migration

### Backend Database
```sql
-- Check User table has CustomerId column
SELECT TOP 1 * FROM Users WHERE Role = 6; -- Customer role

-- Verify customer user exists
SELECT Username, Email, Role, CustomerId 
FROM Users 
WHERE Username = 'customer';
```

### Mobile App AsyncStorage
To check what's stored:
```javascript
import AsyncStorage from '@react-native-async-storage/async-storage';

// In LoginScreen or a debug function
const checkStorage = async () => {
  const token = await AsyncStorage.getItem('authToken');
  const user = await AsyncStorage.getItem('user');
  console.log('Token:', token);
  console.log('User:', user);
};
```

## Next Steps After Successful Migration

1. ✅ Customer login working
2. ⬜ Test placing orders as logged-in customer
3. ⬜ Test order history retrieval
4. ⬜ Add customer registration screen
5. ⬜ Add profile/settings screen
6. ⬜ Add password change functionality
7. ⬜ Add forgot password flow

---

**Need Help?** Check `documentation/customer-authentication-setup.md` for detailed information.
