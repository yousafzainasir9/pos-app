# Mobile App Not Connecting to Backend - Troubleshooting

## Problem
Mobile app login button doesn't hit backend API breakpoints.

## Quick Diagnosis

### Step 1: Check Backend is Running

```bash
cd D:\pos-app\backend\src\POS.WebAPI
dotnet run
```

**Look for:**
```
Now listening on: http://localhost:XXXX
Now listening on: https://localhost:7021
```

**Note the HTTP port number!**

### Step 2: Test Backend Locally

Open PowerShell:

```powershell
# Test the API is working
Invoke-WebRequest -Uri "http://localhost:5021/api/stores" -Method GET

# Or try port 5000
Invoke-WebRequest -Uri "http://localhost:5000/api/stores" -Method GET
```

If you get a response, backend is working!

### Step 3: Update Mobile App API URL

The mobile app needs to use the correct port.

**Current setting:** `http://10.0.2.2:5021/api`

**Edit:** `D:\pos-app\mobileApp\src\api\client.ts`

**Change to match your backend port:**

```typescript
// If backend shows: http://localhost:5021
const API_BASE_URL = 'http://10.0.2.2:5021/api';

// If backend shows: http://localhost:5000  
const API_BASE_URL = 'http://10.0.2.2:5000/api';
```

### Step 4: Add Logging to Mobile App

Let's see what's actually happening. Update the client to log requests:

```typescript
// Add this interceptor BEFORE the auth interceptor
apiClient.interceptors.request.use(
  (config) => {
    console.log('🚀 API Request:', config.method?.toUpperCase(), config.url);
    console.log('📍 Full URL:', config.baseURL + config.url);
    return config;
  },
  (error) => {
    console.log('❌ Request Error:', error);
    return Promise.reject(error);
  }
);

// Add this interceptor AFTER the response interceptor
apiClient.interceptors.response.use(
  (response) => {
    console.log('✅ API Response:', response.status, response.config.url);
    return response;
  },
  (error) => {
    console.log('❌ API Error:', error.message);
    if (error.response) {
      console.log('📍 Status:', error.response.status);
      console.log('📍 Data:', error.response.data);
    } else if (error.request) {
      console.log('📍 No response received');
      console.log('📍 Request:', error.request);
    }
    return Promise.reject(error);
  }
);
```

### Step 5: Check Mobile App Logs

In Metro bundler, you should see logs when you try to login:
```
🚀 API Request: POST /auth/login
📍 Full URL: http://10.0.2.2:5021/api/auth/login
```

If you see:
- **Connection timeout** → Backend not reachable
- **Network error** → Wrong URL/port
- **404 Not Found** → Wrong endpoint
- **500 Server Error** → Backend error

### Step 6: Common Issues

#### Issue: "Network Error" or "Connection Refused"

**Solution 1:** Check firewall
```powershell
# Allow port in Windows Firewall
netsh advfirewall firewall add rule name="ASP.NET Core Web API" dir=in action=allow protocol=TCP localport=5021
```

**Solution 2:** Backend not listening on all interfaces

Edit `Properties/launchSettings.json`:
```json
"applicationUrl": "http://0.0.0.0:5021;https://localhost:7021"
```

**Solution 3:** Wrong emulator IP

- Android Emulator: `10.0.2.2`
- iOS Simulator: `localhost`
- Physical Device: Your computer's IP (e.g., `192.168.1.100`)

#### Issue: Backend breakpoint not hit but app works

The backend IS being hit, but your breakpoint might be in the wrong place.

**Set breakpoint here:**
- `AuthController.cs` → Line 50 (inside `Login` method)
- First line after `[HttpPost("login")]`

### Step 7: Full Diagnostic Commands

Run these in order:

```bash
# 1. Backend
cd D:\pos-app\backend\src\POS.WebAPI
dotnet run

# 2. In another terminal - Test backend
curl http://localhost:5021/api/stores

# 3. Mobile app with logging
cd D:\pos-app\mobileApp
# Edit client.ts to add logging (see Step 4)
npm start

# 4. In another terminal
npx react-native run-android

# 5. Watch Metro bundler logs for API requests
```

## Quick Fix: Use Different Port Profile

Try running backend with HTTP-only profile:

```bash
cd D:\pos-app\backend\src\POS.WebAPI
dotnet run --launch-profile http
```

This will use port **5000**, then update mobile app:

```typescript
const API_BASE_URL = 'http://10.0.2.2:5000/api';
```

## Test Login from Mobile

1. Open app
2. Switch to PIN tab
3. Enter: `1234`
4. Click "Login with PIN"
5. Watch Metro bundler console for logs

## Still Not Working?

### Last Resort: Check Network

```bash
# On emulator, open terminal in Android Studio
# Tools → Device Manager → Your Emulator → Show Advanced Settings → Terminal

# Inside emulator terminal:
curl http://10.0.2.2:5021/api/stores

# If this works, the issue is in your mobile app code
# If this fails, the issue is network/backend configuration
```

---

## Expected Working Flow

```
Mobile App (10.0.2.2:5021)
    ↓
Android Emulator Network Layer
    ↓
Host Machine (localhost:5021)
    ↓
ASP.NET Core WebAPI
    ↓
Your Breakpoint Hit! ✅
```

---

**Most Common Fix:**

The backend is running on port **5000** or **7021**, but mobile is trying **5021**.

**Solution:**
1. Check backend console for actual port
2. Update `client.ts` to match
3. Reload mobile app (press 'r' in Metro)
4. Try login again

Let me know what port your backend is actually running on and we'll fix it! 🚀
