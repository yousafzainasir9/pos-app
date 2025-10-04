# 🏗️ Architecture Improvements Diagram

## Before vs After Architecture

### BEFORE - Old Structure
```
┌─────────────────────────────────────────────┐
│           WebAPI Layer                      │
│  ┌───────────────────────────────────────┐  │
│  │  AuthController.cs                    │  │
│  │  • DTOs defined in controller ❌      │  │
│  │  • Generic error handling ❌          │  │
│  │  • Plain text tokens ❌               │  │
│  │  • Magic numbers ❌                   │  │
│  │  • No rate limiting ❌                │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│        Application Layer                    │
│  • No validators ❌                         │
│  • No custom exceptions ❌                  │
│  • No constants ❌                          │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│       Infrastructure Layer                  │
│  • No security service ❌                   │
│  • Basic data access only                  │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│          Domain Layer                       │
│  • User entity (RefreshToken plain) ❌      │
└─────────────────────────────────────────────┘
```

### AFTER - Improved Structure
```
┌─────────────────────────────────────────────┐
│           WebAPI Layer                      │
│  ┌───────────────────────────────────────┐  │
│  │  Configuration/                       │  │
│  │  └── RateLimitOptions.cs ✅           │  │
│  ├───────────────────────────────────────┤  │
│  │  Middleware/                          │  │
│  │  └── ExceptionHandlingMiddleware ✅   │  │
│  ├───────────────────────────────────────┤  │
│  │  Controllers/                         │  │
│  │  └── AuthController.cs                │  │
│  │      • Uses DTOs from App layer ✅    │  │
│  │      • Typed exceptions ✅            │  │
│  │      • Security service ✅            │  │
│  │      • Constants ✅                   │  │
│  │      • Rate limiting ✅               │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│        Application Layer                    │
│  ┌───────────────────────────────────────┐  │
│  │  DTOs/Auth/                           │  │
│  │  • LoginRequestDto ✅                 │  │
│  │  • PinLoginRequestDto ✅              │  │
│  │  • RefreshTokenRequestDto ✅          │  │
│  │  • LoginResponseDto ✅                │  │
│  │  • UserDto ✅                         │  │
│  ├───────────────────────────────────────┤  │
│  │  Validators/Auth/                     │  │
│  │  • LoginRequestValidator ✅           │  │
│  │  • PinLoginRequestValidator ✅        │  │
│  │  • RefreshTokenRequestValidator ✅    │  │
│  ├───────────────────────────────────────┤  │
│  │  Common/Constants/                    │  │
│  │  • AuthConstants ✅                   │  │
│  │  • ErrorCodes ✅                      │  │
│  ├───────────────────────────────────────┤  │
│  │  Common/Exceptions/                   │  │
│  │  • ApplicationException ✅            │  │
│  │  • AuthenticationException ✅         │  │
│  │  • ValidationException ✅             │  │
│  │  • NotFoundException ✅               │  │
│  │  • BusinessRuleException ✅           │  │
│  ├───────────────────────────────────────┤  │
│  │  Common/Models/                       │  │
│  │  • ApiResponse<T> ✅                  │  │
│  │  • ErrorResponse ✅                   │  │
│  ├───────────────────────────────────────┤  │
│  │  Interfaces/                          │  │
│  │  • ISecurityService ✅                │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│       Infrastructure Layer                  │
│  ┌───────────────────────────────────────┐  │
│  │  Services/Security/                   │  │
│  │  • SecurityService ✅                 │  │
│  │    - HashToken()                      │  │
│  │    - VerifyToken()                    │  │
│  │    - GenerateSecureToken()            │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│          Domain Layer                       │
│  • User entity                              │
│    - RefreshToken (now stores hash) ✅      │
└─────────────────────────────────────────────┘
```

---

## Request Flow - Authentication

### Login Request Flow
```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │ POST /api/auth/login
       │ { username, password }
       ↓
┌──────────────────────────────────────┐
│  Rate Limiting Middleware            │
│  • Check: < 5 attempts in 5 min? ✅  │
└──────┬───────────────────────────────┘
       ↓ (if allowed)
┌──────────────────────────────────────┐
│  Exception Handling Middleware       │
│  • Wraps entire request              │
└──────┬───────────────────────────────┘
       ↓
┌──────────────────────────────────────┐
│  FluentValidation                    │
│  • Validate username (3-50 chars) ✅ │
│  • Validate password (min 6 chars) ✅│
└──────┬───────────────────────────────┘
       ↓ (if valid)
┌──────────────────────────────────────┐
│  AuthController.Login()              │
│  1. Query user from database         │
│  2. Verify password (BCrypt)         │
│  3. Generate JWT token               │
│  4. Generate refresh token           │
│  5. Hash refresh token (SHA256) ✅   │
│  6. Store hash in database ✅        │
│  7. Return token to client           │
└──────┬───────────────────────────────┘
       ↓ (success)
┌──────────────────────────────────────┐
│  ApiResponse<LoginResponseDto>       │
│  {                                   │
│    success: true,                    │
│    data: {                           │
│      token: "eyJ...",                │
│      refreshToken: "plain",          │
│      user: { ... }                   │
│    }                                 │
│  }                                   │
└──────┬───────────────────────────────┘
       ↓
┌──────────────┐
│   Client     │
│ (stores both)│
└──────────────┘
```

### Error Flow
```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │ POST /api/auth/login
       │ { username: "wrong", password: "wrong" }
       ↓
┌──────────────────────────────────────┐
│  AuthController.Login()              │
│  • User not found or password wrong  │
│  • throw AuthenticationException     │
│    .InvalidCredentials() ✅          │
└──────┬───────────────────────────────┘
       ↓ (exception thrown)
┌──────────────────────────────────────┐
│  Exception Handling Middleware       │
│  • Catch AuthenticationException     │
│  • Extract error code: AUTH_001 ✅   │
│  • Extract message ✅                │
│  • Create ErrorResponse ✅           │
│  • Set status: 401 ✅               │
└──────┬───────────────────────────────┘
       ↓
┌──────────────────────────────────────┐
│  ApiResponse (Error)                 │
│  {                                   │
│    success: false,                   │
│    error: {                          │
│      errorCode: "AUTH_001",          │
│      message: "Invalid credentials", │
│      timestamp: "2025-01-04..."      │
│    }                                 │
│  }                                   │
└──────┬───────────────────────────────┘
       ↓
┌──────────────┐
│   Client     │
│ (shows error)│
└──────────────┘
```

---

## Security Flow - Token Hashing

### Token Generation & Storage
```
Client                AuthController          SecurityService         Database
  │                         │                       │                    │
  │   Login Request         │                       │                    │
  │────────────────────────>│                       │                    │
  │                         │                       │                    │
  │                         │  GenerateSecureToken()│                    │
  │                         │──────────────────────>│                    │
  │                         │                       │                    │
  │                         │  "abc123xyz..." ◄─────│                    │
  │                         │  (plain token)        │                    │
  │                         │                       │                    │
  │                         │  HashToken(token)     │                    │
  │                         │──────────────────────>│                    │
  │                         │                       │                    │
  │                         │  "7f3e9..." ◄─────────│                    │
  │                         │  (SHA256 hash)        │                    │
  │                         │                       │                    │
  │                         │  Store hash           │                    │
  │                         │────────────────────────────────────────────>│
  │                         │                       │    RefreshToken    │
  │                         │                       │    = "7f3e9..."    │
  │                         │                       │                    │
  │  Response               │                       │                    │
  │<────────────────────────│                       │                    │
  │  { token: "...",        │                       │                    │
  │    refreshToken:        │                       │                    │
  │    "abc123xyz..." }     │                       │                    │
  │  (plain to client)      │                       │                    │
  │                         │                       │                    │

Client stores plain token
Database stores hash ✅

If database is breached:
❌ Before: Attacker gets plain tokens
✅ After:  Attacker only gets hashes (useless)
```

### Token Verification
```
Client                AuthController          SecurityService         Database
  │                         │                       │                    │
  │  Refresh Request        │                       │                    │
  │  { refreshToken:        │                       │                    │
  │    "abc123xyz..." }     │                       │                    │
  │────────────────────────>│                       │                    │
  │                         │                       │                    │
  │                         │  HashToken(token)     │                    │
  │                         │──────────────────────>│                    │
  │                         │                       │                    │
  │                         │  "7f3e9..." ◄─────────│                    │
  │                         │                       │                    │
  │                         │  Find user by hash    │                    │
  │                         │────────────────────────────────────────────>│
  │                         │                       │  WHERE             │
  │                         │                       │  RefreshToken      │
  │                         │                       │  = "7f3e9..." ✅   │
  │                         │  User found ◄─────────────────────────────│
  │                         │                       │                    │
  │  New tokens             │                       │                    │
  │<────────────────────────│                       │                    │
  │                         │                       │                    │
```

---

## Rate Limiting Flow

### Normal Usage (< 5 attempts)
```
┌────────────┐
│  Client    │
└──────┬─────┘
       │ Attempt 1
       ↓
┌──────────────────────────────┐
│  Rate Limiter                │
│  Attempts: 1/5 ✅            │
│  Window: 5 minutes           │
└──────┬───────────────────────┘
       ↓ Allow
┌──────────────┐
│ AuthController│
│  Process login│
└───────────────┘

       │ Attempt 2
       ↓
┌──────────────────────────────┐
│  Rate Limiter                │
│  Attempts: 2/5 ✅            │
└──────┬───────────────────────┘
       ↓ Allow
┌──────────────┐
│ AuthController│
└───────────────┘
```

### Rate Limit Exceeded (> 5 attempts)
```
┌────────────┐
│  Client    │
└──────┬─────┘
       │ Attempt 6 (within 5 min)
       ↓
┌──────────────────────────────┐
│  Rate Limiter                │
│  Attempts: 6/5 ❌            │
│  BLOCKED!                    │
└──────┬───────────────────────┘
       ↓ Reject
┌──────────────────────────────┐
│  HTTP 429 Too Many Requests  │
│  {                           │
│    error: "Rate limit..."    │
│    retryAfter: 300s          │
│  }                           │
└──────┬───────────────────────┘
       ↓
┌────────────┐
│  Client    │
│ Must wait  │
│ 5 minutes  │
└────────────┘
```

---

## Validation Flow

### With FluentValidation
```
Request: { username: "ab", password: "123" }
       ↓
┌───────────────────────────────────────┐
│  LoginRequestValidator                │
│  • Username min 3 chars ❌            │
│  • Password min 6 chars ❌            │
└───────┬───────────────────────────────┘
       ↓ (validation fails)
┌───────────────────────────────────────┐
│  ValidationException                  │
│  Errors: {                            │
│    "Username": [                      │
│      "must be at least 3 characters"  │
│    ],                                 │
│    "Password": [                      │
│      "must be at least 6 characters"  │
│    ]                                  │
│  }                                    │
└───────┬───────────────────────────────┘
       ↓
┌───────────────────────────────────────┐
│  Exception Handling Middleware        │
│  • Catch ValidationException          │
│  • Create ErrorResponse               │
│  • Status: 400 Bad Request            │
└───────┬───────────────────────────────┘
       ↓
┌───────────────────────────────────────┐
│  ApiResponse (Error)                  │
│  {                                    │
│    success: false,                    │
│    error: {                           │
│      errorCode: "VAL_001",            │
│      message: "Validation failed",    │
│      errors: {                        │
│        "Username": ["..."],           │
│        "Password": ["..."]            │
│      }                                │
│    }                                  │
│  }                                    │
└───────┬───────────────────────────────┘
       ↓
    Client
    (shows field-specific errors)
```

---

## Files Organization

```
POS Application
│
├── Domain (Entities, Enums)
│   └── User.cs ✅ (documented RefreshToken field)
│
├── Application (Business Logic)
│   ├── DTOs/Auth/ ✅
│   │   ├── LoginRequestDto
│   │   ├── PinLoginRequestDto
│   │   ├── RefreshTokenRequestDto
│   │   ├── LoginResponseDto
│   │   └── UserDto
│   │
│   ├── Validators/Auth/ ✅
│   │   ├── LoginRequestValidator
│   │   ├── PinLoginRequestValidator
│   │   └── RefreshTokenRequestValidator
│   │
│   ├── Common/
│   │   ├── Constants/ ✅
│   │   │   ├── AuthConstants
│   │   │   └── ErrorCodes
│   │   │
│   │   ├── Exceptions/ ✅
│   │   │   ├── ApplicationException
│   │   │   ├── AuthenticationException
│   │   │   ├── ValidationException
│   │   │   ├── NotFoundException
│   │   │   └── BusinessRuleException
│   │   │
│   │   └── Models/ ✅
│   │       ├── ApiResponse<T>
│   │       └── ErrorResponse
│   │
│   └── Interfaces/ ✅
│       └── ISecurityService
│
├── Infrastructure (Data Access, Services)
│   └── Services/Security/ ✅
│       └── SecurityService
│
└── WebAPI (Controllers, Middleware)
    ├── Configuration/ ✅
    │   └── RateLimitOptions
    │
    ├── Middleware/ ✅
    │   └── ExceptionHandlingMiddleware
    │
    └── Controllers/
        └── AuthController ✅ (updated)
```

---

## Summary of Improvements

### Security: 🔴 → 🟢
- ✅ Token hashing (SHA256)
- ✅ Rate limiting (5/5min)
- ✅ Input validation
- ✅ Secure token generation

### Code Quality: 🟡 → 🟢
- ✅ Clean architecture
- ✅ Typed exceptions
- ✅ Constants (no magic numbers)
- ✅ Proper layer separation

### Maintainability: 🟡 → 🟢
- ✅ Reusable validators
- ✅ Centralized error codes
- ✅ Consistent API responses
- ✅ Comprehensive documentation

### Developer Experience: 🟡 → 🟢
- ✅ Clear error messages
- ✅ Easy to extend
- ✅ Well documented
- ✅ Testing ready
