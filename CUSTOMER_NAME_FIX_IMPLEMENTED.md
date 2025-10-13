# Customer Name Display Fix - Implemented

## Problem Solved
The mobile app was showing "Test Customer" in the checkout form but orders appeared as "Sarah Garcia" on the web dashboard. This happened because:

- **User Table** (for authentication): FirstName="Test", LastName="Customer"
- **Customer Table** (for customer data): FirstName="Sarah", LastName="Garcia"
- Mobile app was loading from User table, but web dashboard displays from Customer table

## Solution Implemented

Modified `AuthController.cs` to load Customer data when `user.CustomerId` exists, for all three authentication endpoints:

### Changes Made:

1. **Login Endpoint** - Lines 107-132
2. **PIN Login Endpoint** - Lines 226-255
3. **Refresh Token Endpoint** - Lines 317-346

### What Changed:

```csharp
// Load Customer data if user is a customer
Customer? customer = null;
if (user.CustomerId.HasValue)
{
    customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(user.CustomerId.Value);
}

// Use Customer data with fallback to User data
User = new AuthUserDto
{
    FirstName = customer?.FirstName ?? user.FirstName,  // "Sarah" or "Test"
    LastName = customer?.LastName ?? user.LastName,      // "Garcia" or "Customer"
    Phone = customer?.Phone ?? user.Phone                // Customer phone priority
}
```

## How It Works Now

### For Customer Users (Role: Customer):
1. User logs in with username/password/PIN
2. Backend checks `user.CustomerId` → finds Customer record
3. Returns Customer.FirstName ("Sarah") + Customer.LastName ("Garcia")
4. Mobile app displays "Sarah Garcia" in checkout form ✅
5. Orders show "Sarah Garcia" on web dashboard ✅

### For Staff Users (Role: Cashier/Admin/Manager):
1. User logs in
2. Backend checks `user.CustomerId` → null
3. Returns User.FirstName + User.LastName (staff name)
4. POS displays staff member's name ✅
5. No impact on existing functionality ✅

## Testing Steps

1. **Backend: Restart the server**
   ```bash
   cd D:\pos-app\backend\src\POS.WebAPI
   dotnet run
   ```

2. **Mobile App: Clear cache and restart**
   ```bash
   cd D:\pos-app\mobileApp
   # Stop Metro bundler (Ctrl+C)
   # Clear AsyncStorage by uninstalling app or:
   npm start -- --reset-cache
   # In new terminal:
   npm run android
   ```

3. **Test Scenario:**
   - Login to mobile app with customer account (username: customer, password: [password])
   - Check that checkout form now shows "Sarah Garcia"
   - Place a test order
   - Verify web dashboard shows "Sarah Garcia" for the new order

4. **Verify Staff Login (POS):**
   - Login to POS with cashier/admin account
   - Verify staff name displays correctly
   - No changes to existing functionality

## Database Records

### User Table (ID: 14)
```
Username: customer
FirstName: Test
LastName: Customer
CustomerId: [points to Customer table]
Role: Customer
```

### Customer Table (ID: ?)
```
FirstName: Sarah
LastName: Garcia
Phone: +61 413 219 270
```

## Files Modified

- ✅ `backend/src/POS.WebAPI/Controllers/AuthController.cs`
  - Login method: Added Customer data loading
  - PinLogin method: Added Customer data loading
  - RefreshToken method: Added Customer data loading

## No Mobile App Changes Needed

The mobile app already uses `user.firstName` and `user.lastName` correctly. Once the backend sends the right data, everything works automatically!

## Rollback

If you need to revert:

```bash
cd D:\pos-app
git checkout backend/src/POS.WebAPI/Controllers/AuthController.cs
```

## Expected Results

### Before Fix:
- Mobile checkout form: "Test Customer"
- Web dashboard orders: "Sarah Garcia"
- ❌ Inconsistent

### After Fix:
- Mobile checkout form: "Sarah Garcia" ✅
- Web dashboard orders: "Sarah Garcia" ✅
- ✅ Consistent everywhere!

## Notes

- The User table data ("Test Customer") remains unchanged - it's still used for authentication
- The Customer table data ("Sarah Garcia") is now properly loaded and used for display
- The solution respects the separation between User (authentication) and Customer (business data)
- Staff users without CustomerId continue working exactly as before
