# 🎯 Quick Reference: POS Security Improvements

## 📦 Files Created

### Application Layer (POS.Application)
```
DTOs/Auth/
├── LoginRequestDto.cs
├── PinLoginRequestDto.cs
├── RefreshTokenRequestDto.cs
├── LoginResponseDto.cs
└── UserDto.cs

Validators/Auth/
├── LoginRequestValidator.cs
├── PinLoginRequestValidator.cs
└── RefreshTokenRequestValidator.cs

Common/Constants/
├── AuthConstants.cs
└── ErrorCodes.cs

Common/Exceptions/
├── ApplicationException.cs
├── AuthenticationException.cs
├── ValidationException.cs
├── NotFoundException.cs
└── BusinessRuleException.cs

Common/Models/
├── ApiResponse.cs
└── ErrorResponse.cs

Interfaces/
└── ISecurityService.cs
```

### Infrastructure Layer (POS.Infrastructure)
```
Services/Security/
└── SecurityService.cs
```

### WebAPI Layer (POS.WebAPI)
```
Configuration/
└── RateLimitOptions.cs

Middleware/
└── ExceptionHandlingMiddleware.cs

Controllers/
└── AuthController.v2.cs
```

---

## 🔑 Key Constants

```csharp
// AuthConstants
RefreshTokenByteSize = 32
AccessTokenExpiryMinutes = 60
RefreshTokenExpiryDays = 7
MaxLoginAttempts = 5
LockoutDurationMinutes = 15
MinPasswordLength = 8
PinLength = 4
```

---

## 🚦 Error Codes Reference

### Authentication (AUTH_xxx)
- `AUTH_001` - Invalid credentials
- `AUTH_002` - Invalid PIN
- `AUTH_003` - Expired token
- `AUTH_004` - Invalid refresh token
- `AUTH_005` - Account locked
- `AUTH_006` - Account disabled

### Validation (VAL_xxx)
- `VAL_001` - Required field
- `VAL_002` - Invalid format
- `VAL_003` - Out of range

### Business Logic (BIZ_xxx)
- `BIZ_001` - Insufficient stock
- `BIZ_002` - Shift already open
- `BIZ_003` - No active shift
- `BIZ_004` - Invalid payment amount

### Resources (RES_xxx)
- `RES_001` - Not found
- `RES_002` - Already exists
- `RES_003` - Conflict

### System (SYS_xxx)
- `SYS_001` - Internal error
- `SYS_002` - Database error
- `SYS_003` - External service error

---

## 🎨 Error Response Format

```json
{
  "success": false,
  "data": null,
  "message": null,
  "error": {
    "errorCode": "AUTH_001",
    "message": "Invalid username or password",
    "errors": {
      "Username": ["Username is required"],
      "Password": ["Password must be at least 8 characters"]
    },
    "stackTrace": "...", // Development only
    "timestamp": "2025-01-04T12:00:00Z"
  }
}
```

---

## 🔐 Security Service Usage

```csharp
// Generate secure token
var token = _securityService.GenerateSecureToken(32);

// Hash token for storage
var hash = _securityService.HashToken(token);

// Verify token
bool isValid = _securityService.VerifyToken(token, hash);
```

---

## ⚡ Rate Limiting

### General API
- **Limit:** 100 requests
- **Window:** 60 seconds
- **Queue:** 2 requests

### Auth Endpoints
- **Limit:** 5 requests
- **Window:** 5 minutes (300 seconds)
- **Queue:** 0 requests

### Apply to Endpoint
```csharp
[HttpPost("login")]
[EnableRateLimiting("auth")]
public async Task<ActionResult> Login(...)
```

---

## 🎯 Exception Throwing Examples

```csharp
// Authentication errors
throw AuthenticationException.InvalidCredentials();
throw AuthenticationException.InvalidPin();
throw AuthenticationException.ExpiredToken();

// Validation errors
throw new ValidationException("Invalid input");

// Not found errors
throw new NotFoundException("User", userId);

// Business rule errors
throw BusinessRuleException.InsufficientStock(productName);
throw BusinessRuleException.NoActiveShift();
```

---

## 📝 Validation Rules

### LoginRequestDto
- Username: Required, 3-50 characters
- Password: Required, minimum 6 characters

### PinLoginRequestDto
- PIN: Required, exactly 4 digits
- StoreId: Required, greater than 0

### RefreshTokenRequestDto
- RefreshToken: Required

---

## 🔄 Migration Required

```bash
# Create migration
cd backend/src/POS.Infrastructure
dotnet ef migrations add AddRefreshTokenHashToUser -s ../POS.WebAPI

# Apply migration
cd ../POS.WebAPI
dotnet ef database update

# Or use migrator
cd ../POS.Migrator
dotnet run
```

---

## 📦 NuGet Packages to Install

```bash
# POS.Application
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions

# POS.WebAPI
dotnet add package Microsoft.AspNetCore.RateLimiting
```

---

## ⚙️ Program.cs Updates

### Service Registration
```csharp
// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// Security Service
builder.Services.AddScoped<ISecurityService, SecurityService>();

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("general", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromSeconds(60);
    });

    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(5);
    });
});
```

### Middleware Pipeline
```csharp
// Add FIRST in pipeline
app.UseExceptionHandling();

// Add BEFORE UseAuthentication()
app.UseRateLimiter();

// Existing middleware
app.UseAuthentication();
app.UseAuthorization();
```

---

## 🧪 Testing Commands

```bash
# Build
dotnet build

# Run API
cd backend/src/POS.WebAPI
dotnet run

# Run tests (when created)
cd backend/tests
dotnet test

# Check Swagger
# Open: https://localhost:7124/swagger
```

---

## 📊 Success Metrics

After implementation:
- ✅ No plain text refresh tokens in database
- ✅ Rate limiting active on auth endpoints
- ✅ Consistent error responses
- ✅ Input validation on all requests
- ✅ Proper exception handling
- ✅ No magic numbers in code
- ✅ All DTOs in proper layer

---

## 🎬 Quick Start Commands

```bash
# 1. Install packages
cd backend/src/POS.Application
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions

cd ../POS.WebAPI
dotnet add package Microsoft.AspNetCore.RateLimiting

# 2. Create migration
cd ../POS.Infrastructure
dotnet ef migrations add AddRefreshTokenHashToUser -s ../POS.WebAPI

# 3. Apply migration
cd ../POS.Migrator
dotnet run

# 4. Build and run
cd ../POS.WebAPI
dotnet build
dotnet run
```

---

## 🔍 What Changed in AuthController

### Token Generation
**Before:**
```csharp
var refreshToken = GenerateRefreshToken();
user.RefreshToken = refreshToken; // Plain text!
```

**After:**
```csharp
var refreshToken = _securityService.GenerateSecureToken(
    AuthConstants.RefreshTokenByteSize);
var refreshTokenHash = _securityService.HashToken(refreshToken);
user.RefreshToken = refreshTokenHash; // Hashed!
```

### Error Handling
**Before:**
```csharp
catch (Exception ex)
{
    return StatusCode(500, "An error occurred");
}
```

**After:**
```csharp
catch (AuthenticationException)
{
    throw; // Middleware handles it
}
```

### Response Format
**Before:**
```csharp
return Ok(new LoginResponseDto { ... });
```

**After:**
```csharp
return Ok(ApiResponse<LoginResponseDto>
    .SuccessResponse(response, "Login successful"));
```

---

## 📚 Additional Resources

- Implementation Guide: `IMPLEMENTATION_GUIDE.md`
- Original README: `README.md`
- Theme Documentation: `documentation/theme-documentation-index.md`

---

**Version:** 1.0  
**Last Updated:** 2025-01-04  
**Compatibility:** .NET 9, EF Core 9
