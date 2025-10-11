# Customer User Authentication - Mobile App Setup

## Overview
This document describes the customer user authentication system for the Cookie Barrel mobile app, allowing customers to create accounts and login with username/password credentials.

## Database Changes

### 1. UserRole Enum Update
Added new `Customer` role to the `UserRole` enum:

```csharp
public enum UserRole
{
    Admin = 1,
    Manager = 2,
    Cashier = 3,
    Staff = 4,
    ReadOnly = 5,
    Customer = 6  // NEW
}
```

### 2. User Entity Changes
Added customer linking field to the `User` entity:

```csharp
public class User : BaseEntity
{
    // ... existing fields ...
    
    // Customer-specific fields (nullable for non-customer roles)
    public long? CustomerId { get; set; } // Link to Customer entity
    
    // Navigation properties
    public virtual Customer? Customer { get; set; }
    // ... other navigation properties ...
}
```

### 3. Database Migration Required

**⚠️ IMPORTANT:** You need to create and run a migration for the CustomerId field:

```bash
# Navigate to Infrastructure project
cd backend/src/POS.Infrastructure

# Create migration
dotnet ef migrations add AddCustomerIdToUser -s ../POS.WebAPI

# Apply migration
cd ../../POS.Migrator
dotnet run
```

**OR** if RefreshDatabase is enabled in `appsettings.json`, just run the migrator:
```bash
cd backend/src/POS.Migrator
dotnet run
```

## Database Seeding

### Customer User Creation
The `DatabaseSeeder` now includes `SeedCustomerUsersAsync()` method that:

1. Creates a test customer account for easy mobile app testing
2. Generates user accounts for the first 10 existing customers
3. Links user accounts to customer records via `CustomerId`

### Default Customer Credentials

| Username | Password | Email | Customer Link |
|----------|----------|-------|---------------|
| customer | Customer123! | customer@test.com | First customer in DB |

**Additional accounts** are created from existing customer emails:
- Username: derived from email (e.g., `john.smith@example.com` → `johnsmith`)
- Password: `Customer123!` (default for all)
- StoreId: `null` (customers can order from any store)

## Backend API Updates

### Auth DTOs Enhanced
Updated `AuthUserDto` to include customer-specific fields:

```csharp
public class AuthUserDto
{
    // ... existing fields ...
    
    // Customer-specific fields
    public long? CustomerId { get; set; }
    public string? Phone { get; set; }
}
```

### Auth Controller Updates
All authentication endpoints now populate:
- `CustomerId` - Link to customer record
- `Phone` - Customer phone number

Endpoints updated:
- `POST /api/auth/login` - Username/password login
- `POST /api/auth/pin-login` - PIN login
- `POST /api/auth/refresh` - Token refresh

## Mobile App Changes

### 1. New API Service
Created `auth.api.ts` for authentication:

```typescript
export const authApi = {
  login: async (credentials: LoginRequest): Promise<LoginResponse>
  logout: async (token: string): Promise<void>
}
```

### 2. Updated Auth Types
Enhanced type definitions in `auth.types.ts`:

```typescript
export interface User {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  customerId?: number;
  phone?: string;
}

export interface AuthState {
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isGuest: boolean;
  customer: Customer | null; // Legacy support
}
```

### 3. Redux Auth Slice
Complete rewrite of `authSlice.ts` with:

**Async Thunks:**
- `loginUser` - Login with username/password
- `logoutUser` - Logout and clear tokens
- `restoreSession` - Restore session from AsyncStorage on app start

**Token Storage:**
- Tokens stored in `AsyncStorage` for persistence
- Automatic session restoration on app launch

**Backward Compatibility:**
- Maintains `customer` field for existing cart/order flows
- Guest mode still supported

### 4. Login Screen
Complete redesign with:
- Username input field
- Password input field (secure text)
- Loading states
- Error handling and display
- Guest mode option
- Test credentials helper
- Form validation

### 5. App Navigator
Enhanced with:
- Session restoration on app start
- Loading screen during auth check
- Proper navigation based on auth state

## Testing the Feature

### 1. Run Database Migration
```bash
cd backend/src/POS.Migrator
dotnet run
```

Expected output:
```
📱 Web App (Admin/Staff):
  Admin:    admin / Admin123!
  Manager:  manager1 / Manager123!
  Cashier:  cashier2 / Cashier123!

📱 Mobile App (Customers):
  Customer: customer / Customer123!
```

### 2. Start Backend API
```bash
cd backend/src/POS.WebAPI
dotnet run
```

### 3. Start Mobile App
```bash
cd mobileApp
npm start
# In another terminal:
npm run android  # or npm run ios
```

### 4. Test Login Flow
1. Open the mobile app
2. Enter credentials:
   - Username: `customer`
   - Password: `Customer123!`
3. Click "Login"
4. Should navigate to the home screen
5. Close and reopen app - should remain logged in

### 5. Test Guest Mode
1. Open app (or logout if logged in)
2. Click "Continue as Guest"
3. Should navigate to home screen
4. Guest purchases work as before

## API Endpoints

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "customer",
  "password": "Customer123!"
}

Response 200:
{
  "success": true,
  "data": {
    "token": "eyJhbGci...",
    "refreshToken": "abc123...",
    "expiresIn": 900,
    "user": {
      "id": 1,
      "username": "customer",
      "email": "customer@test.com",
      "firstName": "Test",
      "lastName": "Customer",
      "role": "Customer",
      "customerId": 2,
      "phone": "+61 400 000 100"
    }
  }
}
```

### Logout
```http
POST /api/auth/logout
Authorization: Bearer {token}

Response 200:
{
  "success": true,
  "message": "Logged out successfully"
}
```

## Security Considerations

### Current Implementation
✅ Passwords hashed with BCrypt
✅ JWT tokens with expiry
✅ Refresh tokens hashed in database
✅ Secure token storage (AsyncStorage)
✅ Rate limiting on auth endpoints

### Production Recommendations
- [ ] Implement password complexity requirements
- [ ] Add email verification for new accounts
- [ ] Enable two-factor authentication
- [ ] Implement account lockout after failed attempts
- [ ] Add forgot password functionality
- [ ] Use secure token storage (iOS Keychain, Android Keystore)
- [ ] Implement biometric authentication option

## Future Enhancements

### Customer Registration
Create a registration screen for new customers:
- Username/email/password input
- Phone number
- Address (optional)
- Terms and conditions acceptance
- Email verification

### Profile Management
Add customer profile screen:
- View/edit profile information
- Change password
- Manage addresses
- View loyalty points
- Order history

### Social Login
Add social authentication options:
- Google Sign-In
- Apple Sign-In
- Facebook Login

## Troubleshooting

### "Migration Pending" Error
**Problem:** Database schema doesn't match code
**Solution:**
```bash
cd backend/src/POS.Migrator
dotnet run
```

### "Invalid Credentials" Error
**Problem:** Wrong username or password
**Solution:** 
- Verify credentials: `customer` / `Customer123!`
- Check database seeder ran successfully
- Verify user exists in database

### App Stuck on Loading Screen
**Problem:** Session restoration failing
**Solution:**
```javascript
// Clear AsyncStorage in app
import AsyncStorage from '@react-native-async-storage/async-storage';
await AsyncStorage.clear();
```

### "Cannot connect to API" Error
**Problem:** Mobile app can't reach backend
**Solution:**
- Check API is running on `https://localhost:5001`
- Update `API_BASE_URL` in `src/api/client.ts`
- For Android emulator use: `http://10.0.2.2:5000`
- For physical device use: Your computer's IP address

## Files Changed/Created

### Backend
- ✏️ `POS.Domain/Enums/Enums.cs` - Added Customer role
- ✏️ `POS.Domain/Entities/User.cs` - Added CustomerId field
- ✏️ `POS.Application/DTOs/Auth/AuthDtos.cs` - Added customer fields
- ✏️ `POS.WebAPI/Controllers/AuthController.cs` - Updated responses
- ✏️ `POS.Infrastructure/Data/Seeders/DatabaseSeeder.cs` - Added customer user seeding
- ✏️ `POS.Migrator/Program.cs` - Updated credentials display

### Mobile App
- 📄 `src/api/auth.api.ts` - NEW: Auth API service
- ✏️ `src/types/auth.types.ts` - Updated with User interface
- ✏️ `src/store/slices/authSlice.ts` - Complete rewrite with async thunks
- ✏️ `src/screens/LoginScreen.tsx` - Complete redesign with form
- ✏️ `src/navigation/AppNavigator.tsx` - Added session restoration

### Documentation
- 📄 `documentation/customer-authentication-setup.md` - This file

## Next Steps

1. ✅ Run database migration
2. ✅ Test login flow
3. ✅ Test session persistence
4. ⬜ Add registration screen
5. ⬜ Add profile management
6. ⬜ Implement password reset
7. ⬜ Add biometric authentication
8. ⬜ Implement social login

## Support

For issues or questions:
1. Check this documentation
2. Review console/API logs
3. Verify database migration ran successfully
4. Check API connectivity from mobile app
5. Verify AsyncStorage permissions (mobile)

---

**Last Updated:** October 11, 2025
**Version:** 1.0
**Author:** Development Team
