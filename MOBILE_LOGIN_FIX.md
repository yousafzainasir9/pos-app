# Mobile Login Fix - HTTP 307 Redirect Issue

## Problem
Mobile app was unable to login due to HTTP 307 redirect. The app was making HTTP requests to `http://10.0.2.2:5021/api/auth/pin-login` but the server was redirecting to HTTPS.

## Root Cause
The ASP.NET Core backend was using `app.UseHttpsRedirection()` in all environments, forcing HTTP requests to redirect to HTTPS. Since the mobile app was configured to use HTTP (which is appropriate for development with Android emulator), the 307 redirect was preventing successful authentication.

## Changes Made

### 1. Backend - Program.cs
**File:** `D:\pos-app\backend\src\POS.WebAPI\Program.cs`

**Change:** Disabled HTTPS redirection in development environment

```csharp
// OLD CODE:
app.UseHttpsRedirection();

// NEW CODE:
// Only use HTTPS redirection in production (allows mobile app to connect via HTTP in development)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
```

**Line:** 219-225

This allows the mobile app to make HTTP requests during development without being redirected.

### 2. Mobile App - build.gradle
**File:** `D:\pos-app\mobileApp\android\app\build.gradle`

**Change:** Added manifest placeholder to enable cleartext HTTP traffic

```gradle
defaultConfig {
    applicationId "com.mobileapp"
    minSdkVersion rootProject.ext.minSdkVersion
    targetSdkVersion rootProject.ext.targetSdkVersion
    versionCode 1
    versionName "1.0"
    // Allow cleartext HTTP traffic for development
    manifestPlaceholders = [usesCleartextTraffic: "true"]
}
```

This ensures Android allows HTTP traffic to the local development server.

## Configuration Summary

### Backend Configuration
- **HTTP Port:** 5021 (used by mobile app)
- **HTTPS Port:** 7021 (used by Swagger/browser)
- **Environment:** Development
- **HTTPS Redirection:** Disabled in development

### Mobile App Configuration
- **API Base URL:** `http://10.0.2.2:5021/api`
- **Emulator:** Android (10.0.2.2 is the special IP for Android emulator to access host machine's localhost)
- **Cleartext Traffic:** Enabled for development
- **Timeout:** 30 seconds

## Testing the Fix

1. **Restart the Backend:**
   ```bash
   # Stop the current running instance
   # Start it again from Visual Studio or CLI
   cd D:\pos-app\backend\src\POS.WebAPI
   dotnet run --launch-profile https
   ```

2. **Rebuild the Mobile App:**
   ```bash
   cd D:\pos-app\mobileApp
   # Clean the build
   cd android
   ./gradlew clean
   cd ..
   # Reinstall on emulator
   npx react-native run-android
   ```

3. **Test Login:**
   - Username: `customer`
   - Password: `Customer123!`
   - Store: Select from dropdown

## Expected Result
- PIN login request should return **200 OK** instead of **307 Redirect**
- Authentication token should be received and stored
- User should be logged in successfully

## Production Considerations

### When Moving to Production:

1. **Enable HTTPS on Backend:**
   - The conditional check ensures HTTPS redirection is enabled in production
   - Obtain proper SSL certificates
   - Update mobile app to use HTTPS endpoint

2. **Update Mobile App:**
   ```typescript
   // For production, use your actual domain
   const API_BASE_URL = 'https://api.yourcompany.com/api';
   ```

3. **Remove Cleartext Traffic:**
   ```gradle
   // In production build variant
   manifestPlaceholders = [usesCleartextTraffic: "false"]
   ```

4. **Update AndroidManifest for Production:**
   Create a network security config that only allows HTTPS:
   ```xml
   <!-- res/xml/network_security_config.xml -->
   <network-security-config>
       <base-config cleartextTrafficPermitted="false" />
   </network-security-config>
   ```

## Troubleshooting

### If login still fails:

1. **Check backend is running:**
   - Open browser to `http://localhost:5021/swagger`
   - Should see Swagger UI

2. **Check network connectivity:**
   ```bash
   # From command prompt
   adb shell ping 10.0.2.2
   ```

3. **Check firewall:**
   - Ensure Windows Firewall allows connections on port 5021
   - Add exception if needed

4. **Check Android logs:**
   ```bash
   npx react-native log-android
   ```
   Look for API request logs starting with 🚀

5. **Test with physical device:**
   - Find your computer's local IP address
   - Update API_BASE_URL to use actual IP (e.g., `http://192.168.1.100:5021/api`)

## Files Modified
1. `D:\pos-app\backend\src\POS.WebAPI\Program.cs` - Line 219-225
2. `D:\pos-app\mobileApp\android\app\build.gradle` - Added manifestPlaceholders

## Additional Notes
- The mobile app already had proper HTTP configuration in `client.ts`
- AndroidManifest.xml already had the placeholder for cleartext traffic
- Only needed to enable it via build.gradle and disable HTTPS redirect on backend
