# 🔧 Swagger Schema Conflict Fix

## Problem
**Error:** `InvalidOperationException: Can't use schemaId "$UserDto" for type "$POS.Application.DTOs.UserDto". The same schemaId is already used for type "$POS.Application.DTOs.Auth.UserDto"`

## Root Cause
Two classes with the same name `UserDto` existed in different namespaces:
1. `POS.Application.DTOs.UserDto` - Used for user management endpoints
2. `POS.Application.DTOs.Auth.UserDto` - Used for authentication responses

Swagger/OpenAPI cannot differentiate between classes with the same name, even if they're in different namespaces.

## Solution Applied ✅
Renamed the Auth UserDto to be more specific:
- **Old:** `POS.Application.DTOs.Auth.UserDto`
- **New:** `POS.Application.DTOs.Auth.AuthUserDto`

## Files Changed

### 1. `src/POS.Application/DTOs/Auth/AuthDtos.cs`
```csharp
// Changed from:
public class UserDto { ... }

// To:
public class AuthUserDto { ... }

// Updated LoginResponseDto:
public class LoginResponseDto
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public required AuthUserDto User { get; set; }  // Changed
}
```

### 2. `src/POS.WebAPI/Controllers/AuthController.cs`
Updated all three login methods to use `AuthUserDto`:
- `Login()` - Line 82
- `PinLogin()` - Line 155
- `RefreshToken()` - Line 225

```csharp
// Changed all instances from:
User = new UserDto { ... }

// To:
User = new AuthUserDto { ... }
```

## Testing

### Build the project:
```bash
cd D:\pos-app\backend
dotnet build
```

### Run the API:
```bash
cd D:\pos-app\backend
run.bat
```

### Verify Swagger:
1. Navigate to: `https://localhost:7xxx/swagger`
2. Swagger UI should load without errors
3. Check the schemas section - you should now see:
   - `UserDto` (for user management)
   - `AuthUserDto` (for authentication)

### Test Auth Endpoints:
```bash
POST /api/Auth/login
{
  "username": "admin",
  "password": "Admin123!"
}
```

Expected response:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "...",
    "refreshToken": "...",
    "expiresIn": 3600,
    "user": {
      "id": 1,
      "username": "admin",
      "email": "admin@pos.com",
      "firstName": "System",
      "lastName": "Administrator",
      "role": "Admin",
      "storeId": 1,
      "storeName": "Main Store",
      "hasActiveShift": false,
      "activeShiftId": null
    }
  }
}
```

## Alternative Solutions (Not Used)

### Option 2: Configure Swagger Schema IDs
Add custom schema ID selector in `Program.cs`:
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);
});
```

### Option 3: Use Namespace in DTO Names
Keep both classes but add more context:
- `UserManagementDto` instead of `UserDto`
- `AuthenticationUserDto` instead of `Auth.UserDto`

## Why Solution 1 is Best
1. ✅ Clear naming - `AuthUserDto` clearly indicates it's for authentication
2. ✅ No Swagger configuration needed
3. ✅ Follows single responsibility principle
4. ✅ Prevents future naming conflicts
5. ✅ More maintainable and readable

## Status
🟢 **FIXED** - Swagger should now load without schema conflicts

## Next Steps
1. Build the solution
2. Run the API
3. Test all auth endpoints
4. Verify Swagger documentation

---

**Fixed Date:** 2025-10-05  
**Issue Type:** Schema Conflict  
**Severity:** High (API wouldn't start)  
**Resolution:** Class Rename
