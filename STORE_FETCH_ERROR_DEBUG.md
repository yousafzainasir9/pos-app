# Store Fetch Error - Debugging Guide

## The Error
"Failed to fetch stores" appears on the Store Selection screen.

## Possible Causes

### 1. Backend Not Running ✋
**Test:** Open browser and go to:
```
http://localhost:5021/api/stores
```

**Expected:** Should see JSON response with stores
**If 404 or Connection Refused:** Backend is not running

### 2. No Stores in Database ⚠️
The database might not have any stores seeded.

**Fix:** Need to add stores to the database.

### 3. API Response Format Mismatch 🔧
Backend returns:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Store Name",
      "address": "123 Main St",
      ...
    }
  ],
  "message": "Stores retrieved successfully"
}
```

Mobile app expects: `response.data.data` (the array of stores)

### 4. CORS or Network Issue 🌐
The mobile app can't reach the backend.

## Quick Fix Steps

### Step 1: Test Backend API Directly

Open Command Prompt and run:
```bash
curl http://localhost:5021/api/stores
```

OR open in browser:
```
http://localhost:5021/api/stores
```

**Expected Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Main Street Store",
      "code": "MS001",
      "address": "123 Main Street",
      "city": "Sydney",
      "phone": "02-1234-5678",
      "email": "mainstreet@cookiebarrel.com.au",
      "taxRate": 10.0,
      "currency": "AUD",
      "isActive": true
    }
  ],
  "message": "Stores retrieved successfully"
}
```

### Step 2: Check Database for Stores

Run this SQL query:
```sql
SELECT * FROM Stores WHERE IsActive = 1;
```

**If no results:** You need to seed stores!

### Step 3: Seed Stores (If Empty)

Run this SQL:
```sql
INSERT INTO Stores (Name, Code, Address, City, State, PostalCode, Country, Phone, Email, TaxRate, Currency, IsActive, CreatedAt)
VALUES 
('Main Street Store', 'MS001', '123 Main Street', 'Sydney', 'NSW', '2000', 'Australia', '02-1234-5678', 'mainstreet@cookiebarrel.com.au', 10.0, 'AUD', 1, GETDATE()),
('Harbor Store', 'HB001', '456 Harbor Road', 'Sydney', 'NSW', '2000', 'Australia', '02-2345-6789', 'harbor@cookiebarrel.com.au', 10.0, 'AUD', 1, GETDATE()),
('Beach Store', 'BC001', '789 Beach Boulevard', 'Sydney', 'NSW', '2000', 'Australia', '02-3456-7890', 'beach@cookiebarrel.com.au', 10.0, 'AUD', 1, GETDATE());
```

### Step 4: Check Mobile App Logs

In your terminal where Metro is running, look for errors:
```
❌ API Error: ...
📍 Status: ...
📍 Response: ...
```

### Step 5: Test from Mobile App

After seeding stores, in the mobile app:
1. Kill the app completely
2. Restart it: `npx react-native run-android`
3. Login with PIN
4. Should now see stores

## Expected Mobile App Behavior

### Success Flow:
```
1. Login with PIN
2. API Call: GET http://10.0.2.2:5021/api/stores
3. Response: { success: true, data: [...stores...] }
4. Store Selection Screen shows list of stores
5. Select a store
6. Navigate to main app
```

### Error Flow:
```
1. Login with PIN
2. API Call: GET http://10.0.2.2:5021/api/stores
3. Response: Error or empty array
4. Store Selection Screen shows "Failed to fetch stores"
5. "Retry" button appears
```

## Testing Checklist

- [ ] Backend is running on port 5021
- [ ] Can access http://localhost:5021/api/stores in browser
- [ ] Database has at least one active store
- [ ] Mobile app can reach http://10.0.2.2:5021
- [ ] No errors in Metro bundler logs
- [ ] No errors in backend API logs

## Still Not Working?

### Check Backend Logs
Look at: `D:\pos-app\backend\src\POS.WebAPI\logs\`

The latest log file will show any API errors.

### Check Mobile Console Logs
In Metro bundler terminal, press 'j' to open debugger, then check Console tab.

### Verify API Client Base URL
Should be: `http://10.0.2.2:5021/api`

File: `D:\pos-app\mobileApp\src\api\client.ts`

## Quick Test Commands

### Test Backend:
```bash
# In browser or curl
curl http://localhost:5021/api/stores

# Expected: JSON with stores array
```

### Test from Android Emulator:
```bash
# From emulator's perspective (10.0.2.2 = host machine)
curl http://10.0.2.2:5021/api/stores
```

### Check Database:
```sql
-- Count active stores
SELECT COUNT(*) FROM Stores WHERE IsActive = 1;

-- List all stores
SELECT Id, Name, Code, IsActive FROM Stores;
```

## Common Solutions

| Problem | Solution |
|---------|----------|
| Backend not running | Start Visual Studio and run POS.WebAPI |
| No stores in DB | Run the INSERT SQL script above |
| Wrong API URL | Check client.ts has `http://10.0.2.2:5021/api` |
| 307 Redirect | Make sure Program.cs changes were applied |
| CORS error | Should be fixed by AllowAll policy in dev |

## Need the SQL to Seed Stores?

Run this in SQL Server Management Studio connected to your database:

```sql
-- Delete existing stores if any (optional)
-- DELETE FROM Stores;

-- Insert sample stores
INSERT INTO Stores (Name, Code, Address, City, State, PostalCode, Country, Phone, Email, TaxNumber, TaxRate, Currency, IsActive, OpeningTime, ClosingTime, CreatedAt)
VALUES 
('Cookie Barrel - Main Street', 'CB-MS', '123 Main Street', 'Sydney', 'NSW', '2000', 'Australia', 
 '02-1234-5678', 'mainstreet@cookiebarrel.com.au', 'ABN 12345678901', 10.0, 'AUD', 1, 
 '08:00:00', '20:00:00', GETDATE()),

('Cookie Barrel - Harbor Point', 'CB-HP', '456 Harbor Road', 'Sydney', 'NSW', '2000', 'Australia', 
 '02-2345-6789', 'harbor@cookiebarrel.com.au', 'ABN 12345678902', 10.0, 'AUD', 1, 
 '08:00:00', '20:00:00', GETDATE()),

('Cookie Barrel - Beach Side', 'CB-BS', '789 Beach Boulevard', 'Bondi', 'NSW', '2026', 'Australia', 
 '02-3456-7890', 'beach@cookiebarrel.com.au', 'ABN 12345678903', 10.0, 'AUD', 1, 
 '07:00:00', '21:00:00', GETDATE());

-- Verify
SELECT * FROM Stores;
```

After running this, restart the mobile app and try again!
