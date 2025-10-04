# API Port Configuration Update

## Issue
Swagger UI was failing to load with the error:
```
Failed to fetch https://localhost:7021/swagger/v1/swagger.json
```

The backend was configured to run on ports 5001/5000, but the application was trying to access port 7021.

## Root Cause
Mismatch between:
- **launchSettings.json**: Configured for ports 5001/5000
- **Running application**: Using port 7021
- **Frontend API calls**: Pointing to port 5001

## Solution
Updated all configurations to use consistent ports **7021 (HTTPS)** and **5021 (HTTP)**.

## Changes Made

### 1. Backend Launch Settings
**File**: `backend/src/POS.WebAPI/Properties/launchSettings.json`

**Changed:**
```json
"applicationUrl": "https://localhost:7021;http://localhost:5021"
```

**Previous:**
```json
"applicationUrl": "https://localhost:5001;http://localhost:5000"
```

### 2. Frontend API Service
**File**: `frontend/src/services/api.service.ts`

**Changed:**
```typescript
const API_BASE_URL = 'https://localhost:7021/api';
```

**Previous:**
```typescript
const API_BASE_URL = 'https://localhost:5001/api';
```

### 3. Frontend Auth Service
**File**: `frontend/src/services/auth.service.ts`

**Changed:**
```typescript
const API_BASE_URL = 'https://localhost:7021/api';
```

**Previous:**
```typescript
const API_BASE_URL = 'https://localhost:5001/api';
```

## Port Configuration

### Development Ports

| Service | HTTPS Port | HTTP Port |
|---------|-----------|-----------|
| **Backend API** | 7021 | 5021 |
| **Frontend** | N/A | 3000 |
| **Swagger UI** | 7021 | 5021 |

### Accessing the Application

**Backend API:**
- HTTPS: `https://localhost:7021/api`
- HTTP: `http://localhost:5021/api`

**Swagger Documentation:**
- HTTPS: `https://localhost:7021/swagger`
- HTTP: `http://localhost:5021/swagger`

**Frontend Application:**
- `http://localhost:3000`

## Running the Application

### 1. Start Backend
```bash
cd backend/src/POS.WebAPI
dotnet run --launch-profile https
```

The API will start on:
- `https://localhost:7021`
- `http://localhost:5021`

### 2. Start Frontend
```bash
cd frontend
npm run dev
```

The frontend will start on:
- `http://localhost:3000`

### 3. Access Swagger
Open your browser and navigate to:
- `https://localhost:7021/swagger`

## Testing Swagger

1. **Navigate to Swagger UI**: `https://localhost:7021/swagger`
2. **You should see**: API documentation with all endpoints
3. **Test authentication**:
   - Expand `Auth` → `POST /api/auth/login`
   - Click "Try it out"
   - Enter credentials:
     ```json
     {
       "username": "admin",
       "password": "Admin123!"
     }
     ```
   - Click "Execute"
   - Copy the token from the response
   - Click "Authorize" button at the top
   - Enter: `Bearer <your-token-here>`
   - Click "Authorize"
   - Now you can test protected endpoints

## Troubleshooting

### Swagger Still Not Loading

**Check if backend is running:**
```bash
# Check if port is in use
netstat -ano | findstr :7021
```

**If port is already in use:**
1. Stop the other application using the port
2. Or change the port in `launchSettings.json`

**Restart the backend:**
```bash
cd backend/src/POS.WebAPI
dotnet run
```

### HTTPS Certificate Issues

If you see SSL/HTTPS errors:

**Trust the development certificate:**
```bash
dotnet dev-certs https --trust
```

**Clear browser cache and restart:**
- Clear browser cache
- Restart browser
- Navigate to `https://localhost:7021/swagger`

### CORS Issues

If frontend can't connect to backend:

**Check CORS policy in Program.cs:**
- Should allow `http://localhost:3000`
- AllowAll policy should be active in development

### Port Conflicts

**If 7021 is already in use, update these files:**

1. `launchSettings.json` - Change `applicationUrl`
2. `api.service.ts` - Change `API_BASE_URL`
3. `auth.service.ts` - Change `API_BASE_URL`

**Example for port 7022:**
```json
"applicationUrl": "https://localhost:7022;http://localhost:5022"
```

## Environment Variables

For production or different environments, you can use environment variables:

**Frontend (.env file):**
```
VITE_API_BASE_URL=https://localhost:7021/api
```

**Backend (appsettings.json):**
```json
{
  "Urls": "https://localhost:7021;http://localhost:5021"
}
```

## Files Modified

1. `backend/src/POS.WebAPI/Properties/launchSettings.json`
2. `frontend/src/services/api.service.ts`
3. `frontend/src/services/auth.service.ts`

## Next Steps

After making these changes:

1. **Stop** the backend if it's running
2. **Restart** the backend: `dotnet run`
3. **Verify** Swagger loads: `https://localhost:7021/swagger`
4. **Restart** the frontend: `npm run dev`
5. **Test** login and API calls from the frontend

## Notes

- Always use HTTPS (port 7021) in production
- HTTP (port 5021) is available for testing only
- The frontend automatically redirects API calls through the configured base URL
- Swagger is only enabled in Development environment for security

## Success Criteria

✅ Swagger UI loads successfully at `https://localhost:7021/swagger`
✅ All API endpoints are visible in Swagger
✅ Can authenticate using the login endpoint
✅ Frontend can connect to backend API
✅ No CORS errors in browser console
✅ Orders page loads data successfully
