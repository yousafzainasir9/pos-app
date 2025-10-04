# 🔄 Frontend Synchronization Guide

## ⚠️ CRITICAL: Frontend Updates Required

The new backend implementation changes the API response format. The frontend MUST be updated to handle the new format, otherwise authentication and error handling will break.

---

## 📋 Changes Summary

### What Changed in Backend API

#### 1. **Success Response Format**
**Before:**
```json
{
  "token": "...",
  "refreshToken": "...",
  "user": { ... }
}
```

**After:**
```json
{
  "success": true,
  "data": {
    "token": "...",
    "refreshToken": "...",
    "user": { ... }
  },
  "message": "Login successful"
}
```

#### 2. **Error Response Format**
**Before:**
```json
{
  "message": "Invalid credentials"
}
```

**After:**
```json
{
  "success": false,
  "error": {
    "errorCode": "AUTH_001",
    "message": "Invalid username or password",
    "errors": {
      "Username": ["Username is required"],
      "Password": ["Password must be at least 6 characters"]
    },
    "timestamp": "2025-01-04T12:00:00Z"
  }
}
```

#### 3. **Rate Limiting**
New HTTP 429 responses when rate limited:
```json
{
  "success": false,
  "error": {
    "errorCode": "SYS_001",
    "message": "Too many requests",
    "timestamp": "2025-01-04T12:00:00Z"
  }
}
```

---

## 🔧 Frontend Files to Update

### **4 Files Need Updates:**

1. **`types/index.ts`** - Add new response types
2. **`services/auth.service.ts`** - Parse new response format
3. **`services/api.service.ts`** - Handle new error format
4. **`contexts/AuthContext.tsx`** - Display validation errors

---

## 📝 Step-by-Step Implementation

### **Step 1: Update Types**

**File:** `frontend/src/types/index.ts`

**Action:** Replace the file with `types/index.v2.ts`

```bash
cd frontend/src/types
copy index.ts index.ts.backup
copy index.v2.ts index.ts
```

**Key Changes:**
- Added `ErrorResponse` interface
- Updated `ApiResponse<T>` to include `success`, `error`, and proper `data` typing

### **Step 2: Update Auth Service**

**File:** `frontend/src/services/auth.service.ts`

**Action:** Replace with `services/auth.service.v2.ts`

```bash
cd frontend/src/services
copy auth.service.ts auth.service.ts.backup
copy auth.service.v2.ts auth.service.ts
```

**Key Changes:**
- Handles `response.data.success` flag
- Extracts data from `response.data.data`
- Parses new error format with `errorCode` and field `errors`

### **Step 3: Update API Service**

**File:** `frontend/src/services/api.service.ts`

**Action:** Replace with `services/api.service.v2.ts`

```bash
cd frontend/src/services
copy api.service.ts api.service.ts.backup
copy api.service.v2.ts api.service.ts
```

**Key Changes:**
- Handles new error response structure
- Shows field-specific validation errors
- Handles HTTP 429 rate limiting
- Extracts error from `response.data.error`

### **Step 4: Update Auth Context**

**File:** `frontend/src/contexts/AuthContext.tsx`

**Action:** Replace with `contexts/AuthContext.v2.tsx`

```bash
cd frontend/src/contexts
copy AuthContext.tsx AuthContext.tsx.backup
copy AuthContext.v2.tsx AuthContext.tsx
```

**Key Changes:**
- Parses and displays validation errors
- Handles new error object structure
- Shows field-specific error messages

---

## 🧪 Testing After Updates

### **Test 1: Valid Login**
1. Open app: `http://localhost:5173`
2. Login with: `admin / Admin123!`
3. ✅ Should see "Login successful!" toast
4. ✅ Should redirect to dashboard

### **Test 2: Invalid Credentials**
1. Login with: `wrong / wrong`
2. ✅ Should see "Invalid username or password" toast
3. ✅ Should stay on login page

### **Test 3: Validation Errors**
1. Login with empty username/password
2. ✅ Should see field-specific errors:
   - "Username: Username is required"
   - "Password: Password is required"

### **Test 4: Rate Limiting**
1. Try to login 6 times with wrong credentials
2. ✅ On 6th attempt, should see "Too many requests" toast
3. ✅ Should be blocked for 5 minutes

### **Test 5: PIN Login**
1. Switch to PIN login tab
2. Enter PIN: `9999`, Store: Main
3. ✅ Should login successfully

### **Test 6: Token Refresh**
1. Login successfully
2. Wait for token to expire (or manipulate in localStorage)
3. Make API call
4. ✅ Token should refresh automatically
5. ✅ Request should succeed

---

## 🔍 What Each File Does

### **types/index.v2.ts**
Defines TypeScript interfaces for the new API format:
- `ApiResponse<T>` - Wrapper for all responses
- `ErrorResponse` - Standard error structure
- All other types remain unchanged

### **auth.service.v2.ts**
Handles authentication API calls:
- Extracts data from `response.data.data`
- Checks `response.data.success` flag
- Parses error from `response.data.error`
- Throws structured error object

### **api.service.v2.ts**
Generic API service with interceptors:
- Handles token refresh with new format
- Parses error response structure
- Shows appropriate error toasts
- Handles validation errors (400)
- Handles rate limiting (429)
- Handles authentication (401)
- Handles server errors (500)

### **AuthContext.v2.tsx**
React context for authentication:
- Catches errors from auth service
- Displays validation errors per field
- Shows user-friendly error messages
- Maintains same API for components

---

## ⚡ Quick Update Script

Run these commands to update all files at once:

```bash
# Navigate to frontend
cd frontend/src

# Backup originals
copy types\index.ts types\index.ts.backup
copy services\auth.service.ts services\auth.service.ts.backup
copy services\api.service.ts services\api.service.ts.backup
copy contexts\AuthContext.tsx contexts\AuthContext.tsx.backup

# Apply updates
copy types\index.v2.ts types\index.ts
copy services\auth.service.v2.ts services\auth.service.ts
copy services\api.service.v2.ts services\api.service.ts
copy contexts\AuthContext.v2.tsx contexts\AuthContext.tsx

# Verify
npm run build
```

---

## 🎯 Verification Checklist

After updating frontend files:

- [ ] Files backed up
- [ ] New files copied over
- [ ] No TypeScript errors
- [ ] App builds successfully: `npm run build`
- [ ] App runs: `npm run dev`
- [ ] Login works with valid credentials
- [ ] Error messages show for invalid credentials
- [ ] Validation errors display per field
- [ ] Rate limiting shows appropriate message
- [ ] PIN login works
- [ ] Token refresh works
- [ ] Logout works

---

## 🚨 Breaking Changes Impact

### **What Will Break if NOT Updated:**

❌ **Login will fail** - Cannot extract token from response  
❌ **Error messages won't show** - Error object structure different  
❌ **Validation errors invisible** - Field errors not parsed  
❌ **Token refresh broken** - New response format not handled  
❌ **Rate limiting errors not shown** - No handler for 429 status  

### **What Works After Update:**

✅ **All authentication flows**  
✅ **Clear error messages**  
✅ **Field-specific validation**  
✅ **Automatic token refresh**  
✅ **Rate limiting feedback**  
✅ **Better UX overall**  

---

## 📊 Before vs After Comparison

### **Error Handling**

**Before:**
```typescript
// Generic error message
toast.error(error.response?.data?.message || 'Login failed');
```

**After:**
```typescript
// Specific error messages with validation details
if (error.errors) {
  Object.entries(error.errors).forEach(([field, messages]) => {
    messages.forEach(msg => toast.error(`${field}: ${msg}`));
  });
} else {
  toast.error(error.message);
}
```

### **Response Parsing**

**Before:**
```typescript
const data = response.data;
localStorage.setItem('token', data.token);
```

**After:**
```typescript
if (!response.data.success || !response.data.data) {
  throw new Error(response.data.error?.message);
}
const loginData = response.data.data;
localStorage.setItem('token', loginData.token);
```

---

## 🔧 Manual Update Guide

If you prefer to update manually rather than using the .v2 files:

### **1. Update types/index.ts**

Add these interfaces at the top:

```typescript
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  error?: ErrorResponse;
}

export interface ErrorResponse {
  errorCode: string;
  message: string;
  errors?: Record<string, string[]>;
  stackTrace?: string;
  timestamp: string;
}
```

### **2. Update auth.service.ts login() method**

Replace the login method with:

```typescript
async login(credentials: LoginRequest): Promise<LoginResponse> {
  try {
    const response = await axios.post<ApiResponse<LoginResponse>>(
      `${API_BASE_URL}/auth/login`, 
      credentials
    );
    
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.error?.message || 'Login failed');
    }

    const loginData = response.data.data;
    
    if (loginData.token) {
      localStorage.setItem('token', loginData.token);
      localStorage.setItem('refreshToken', loginData.refreshToken);
      localStorage.setItem('user', JSON.stringify(loginData.user));
    }
    
    return loginData;
  } catch (error: any) {
    if (error.response?.data?.error) {
      const apiError = error.response.data.error;
      throw {
        message: apiError.message,
        errorCode: apiError.errorCode,
        errors: apiError.errors
      };
    }
    throw error;
  }
}
```

Apply same pattern to `pinLogin()` method.

### **3. Update api.service.ts error handling**

In the response interceptor, replace error handling:

```typescript
if (error.response?.data) {
  const apiError = error.response.data as ApiResponse<any>;
  
  if (apiError.error) {
    const errorMessage = apiError.error.message;
    const errorCode = apiError.error.errorCode;
    const fieldErrors = apiError.error.errors;

    if (error.response.status === 400 && fieldErrors) {
      Object.entries(fieldErrors).forEach(([field, messages]) => {
        messages.forEach(msg => toast.error(`${field}: ${msg}`));
      });
    } else if (error.response.status === 429) {
      toast.error('Too many requests. Please try again later.');
    } else {
      toast.error(errorMessage || 'An error occurred');
    }
  }
}
```

### **4. Update AuthContext.tsx error display**

In login and pinLogin methods, update error handling:

```typescript
catch (error: any) {
  const errorMessage = error.message || 
                      error.response?.data?.error?.message || 
                      'Login failed';
  
  if (error.errors) {
    Object.entries(error.errors).forEach(([field, messages]: [string, any]) => {
      if (Array.isArray(messages)) {
        messages.forEach(msg => toast.error(`${field}: ${msg}`));
      }
    });
  } else {
    toast.error(errorMessage);
  }
  
  throw error;
}
```

---

## 🆘 Troubleshooting

### **Issue: TypeScript errors after update**

**Solution:**
```bash
# Clear node_modules and reinstall
rm -rf node_modules
npm install

# Clear TypeScript cache
npm run build -- --force
```

### **Issue: Login returns "Cannot read property 'data'"**

**Cause:** Using old response format  
**Solution:** Ensure using `response.data.data` not `response.data`

### **Issue: Errors not showing**

**Cause:** Error structure not parsed correctly  
**Solution:** Check `response.data.error` exists

### **Issue: Validation errors not displaying**

**Cause:** Field errors not iterated  
**Solution:** Loop through `error.errors` object

---

## 📚 Additional Resources

- **Backend Changes:** See `IMPROVEMENTS_SUMMARY.md`
- **Error Codes:** See `QUICK_REFERENCE.md`
- **API Documentation:** Check Swagger at `https://localhost:7124/swagger`

---

## ✅ Final Checklist

Before deploying frontend:

- [ ] All 4 files updated
- [ ] TypeScript compiles without errors
- [ ] App builds successfully
- [ ] All tests pass (manual testing checklist above)
- [ ] Error handling verified
- [ ] Rate limiting tested
- [ ] Token refresh works
- [ ] Production build tested: `npm run build && npm run preview`

---

**Ready to update?** Follow the steps above in order! 🚀

**Questions?** Check the troubleshooting section or review the .v2 files for complete examples.
