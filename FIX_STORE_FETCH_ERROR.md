# Fix "Failed to fetch stores" Error

## The Problem
The Store Selection screen shows "Failed to fetch stores" error after login.

## Most Likely Cause
**The database doesn't have any stores!** The API endpoint exists and works, but there's no data to return.

## Quick Fix - 3 Steps

### Step 1: Check if Backend is Running
Open browser and go to:
```
http://localhost:5021/api/stores
```

**Expected:** JSON response like:
```json
{
  "success": true,
  "data": [...],
  "message": "Stores retrieved successfully"
}
```

**If error:** Make sure backend API is running in Visual Studio

### Step 2: Seed the Stores in Database

**Option A - Using SQL Script (Recommended):**
1. Open SQL Server Management Studio
2. Connect to your database server
3. Open the file: `D:\pos-app\backend\SeedStores.sql`
4. Make sure you're connected to `POSDatabase`
5. Click Execute (or press F5)

**Option B - Manual SQL:**
Run this in SSMS:
```sql
USE POSDatabase;

INSERT INTO Stores (Name, Code, Address, City, State, PostalCode, Country, Phone, Email, TaxNumber, TaxRate, Currency, IsActive, OpeningTime, ClosingTime, CreatedAt, UpdatedAt)
VALUES 
('Cookie Barrel - Main Street', 'CB-MS', '123 Main Street', 'Sydney', 'NSW', '2000', 'Australia', 
 '02-1234-5678', 'mainstreet@cookiebarrel.com.au', 'ABN 12345678901', 10.0, 'AUD', 1, 
 '08:00:00', '20:00:00', GETDATE(), GETDATE()),

('Cookie Barrel - Harbor Point', 'CB-HP', '456 Harbor Road', 'Sydney', 'NSW', '2000', 'Australia', 
 '02-2345-6789', 'harbor@cookiebarrel.com.au', 'ABN 12345678902', 10.0, 'AUD', 1, 
 '08:00:00', '20:00:00', GETDATE(), GETDATE()),

('Cookie Barrel - Beach Side', 'CB-BS', '789 Beach Boulevard', 'Bondi', 'NSW', '2026', 'Australia', 
 '02-3456-7890', 'beach@cookiebarrel.com.au', 'ABN 12345678903', 10.0, 'AUD', 1, 
 '07:00:00', '21:00:00', GETDATE(), GETDATE());

-- Verify
SELECT * FROM Stores WHERE IsActive = 1;
```

### Step 3: Retry in Mobile App

1. Click the "Retry" button on the error screen
   OR
2. Go back to login and login again

You should now see the list of stores! 🎉

## Verify It Worked

### In Browser:
```
http://localhost:5021/api/stores
```

Should show:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Cookie Barrel - Main Street",
      "address": "123 Main Street",
      "city": "Sydney",
      "phone": "02-1234-5678",
      ...
    },
    {
      "id": 2,
      "name": "Cookie Barrel - Harbor Point",
      ...
    }
  ],
  "message": "Stores retrieved successfully"
}
```

### In Mobile App:
- Should see a list of stores with:
  - Store icon
  - Store name
  - Address
  - Phone number
- Can select a store (gets highlighted)
- After selection, see "Store selected!" message at bottom

## Troubleshooting

### Still seeing error after seeding?

**Check 1:** Verify stores are in database
```sql
SELECT COUNT(*) FROM Stores WHERE IsActive = 1;
-- Should return 3 or more
```

**Check 2:** Test API directly
```bash
curl http://localhost:5021/api/stores
```

**Check 3:** Check mobile app logs
Look in Metro bundler terminal for:
```
🚀 API Request: GET /stores
✅ API Response: 200 /stores
📦 Response Data: {...}
```

**Check 4:** Restart everything
1. Close mobile app completely
2. Stop backend API
3. Start backend API
4. Restart mobile app: `npx react-native run-android`

### Database Connection Issues?

Make sure your connection string in `appsettings.Development.json` is correct:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=Zai;Database=POSDatabase;User Id=sa;Password=1234;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

## What the SQL Script Does

The `SeedStores.sql` script:
1. ✅ Checks if stores already exist
2. ✅ If no stores, adds 5 sample stores
3. ✅ If stores exist, shows them
4. ✅ Displays final store count
5. ✅ Safe to run multiple times (won't duplicate)

## Expected Result

After seeding stores, your Store Selection screen should look like:

```
┌─────────────────────────────────────┐
│        Select Your Store            │
│   Welcome, John! Choose a store     │
├─────────────────────────────────────┤
│                                     │
│  🏪  Cookie Barrel - Main Street   │
│      123 Main Street               │
│      02-1234-5678                  │
│                                     │
│  🏪  Cookie Barrel - Harbor Point  │
│      456 Harbor Road               │
│      02-2345-6789                  │
│                                     │
│  🏪  Cookie Barrel - Beach Side    │
│      789 Beach Boulevard           │
│      02-3456-7890                  │
│                                     │
└─────────────────────────────────────┘
```

Click on any store to select it and continue to the main app! 🎉

---

**TL;DR:** 
1. Run `D:\pos-app\backend\SeedStores.sql` in SQL Server Management Studio
2. Click "Retry" in the mobile app
3. Select a store
4. Done! ✅
