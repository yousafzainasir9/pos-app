# Running POS Backend in Visual Studio

## Step-by-Step Setup

### 1. Open Solution
- Open Visual Studio 2022
- File → Open → Project/Solution
- Select: `D:\pos-app\backend\POS.sln`

### 2. Set Startup Project
- Right-click **POS.WebAPI** in Solution Explorer
- Click **"Set as Startup Project"**
- Project name should now be **bold**

### 3. Select Launch Profile
Click the dropdown next to the green Play button (▶)

**Choose one of:**
- **http** - Runs on `http://localhost:5000` (recommended for mobile)
- **https** - Runs on `http://localhost:5021` and `https://localhost:7021`
- **IIS Express** - Not recommended

**Recommended: Use "http" profile**

### 4. Run Backend
- Press **F5** (Debug)
- Or press **Ctrl+F5** (Run without debugging)
- Or click green **Play** button

### 5. Verify It's Running

**Check Output Window:**
- View → Output
- Select "Show output from: Debug" or "ASP.NET Core Web Server"
- Look for: `Now listening on: http://localhost:XXXX`

**Or open browser:**
- Navigate to: `http://localhost:5000/swagger`
- You should see Swagger UI

### 6. Set Breakpoints

**In AuthController.cs:**
```csharp
[HttpPost("login")]
public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
{
    // SET BREAKPOINT HERE ⭐
    var user = await _unitOfWork.Repository<User>().Query()
        .Include(u => u.Store)
        .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);
    
    // And here
    if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    {
        // ...
    }
}
```

**To set breakpoint:**
- Click in the left gray margin
- Or place cursor on line and press **F9**
- Red dot appears = breakpoint set ✅

### 7. Update Mobile App

Edit `mobileApp/src/api/client.ts`:

```typescript
// Update to match Visual Studio output
// If VS shows: http://localhost:5000
const API_BASE_URL = 'http://10.0.2.2:5000/api';

// If VS shows: http://localhost:5021
const API_BASE_URL = 'http://10.0.2.2:5021/api';
```

### 8. Test

1. ✅ Backend running in Visual Studio (F5)
2. ✅ Mobile app running (npm run android)
3. ✅ Open mobile app, switch to PIN tab
4. ✅ Enter PIN: `1234`
5. ✅ Click "Login with PIN"
6. ✅ Visual Studio should pause at breakpoint!

## Debugging Tips

### View Variables
When paused at breakpoint:
- Hover over variables to see values
- Use **Locals** window (Debug → Windows → Locals)
- Use **Watch** window to monitor specific values

### Step Through Code
- **F10** - Step Over (execute current line)
- **F11** - Step Into (go into method)
- **Shift+F11** - Step Out (exit current method)
- **F5** - Continue (run until next breakpoint)

### View Request Data
When paused in Login method:
```csharp
// Inspect these:
request.Username  // Should be "1234"
request.Password  // Should be "1234"
user              // Should be the customer user
```

## Common Issues

### Issue: Breakpoint says "The breakpoint will not currently be hit"
**Solution:** 
- Make sure you're running in Debug mode (F5), not Release
- Check you're running the correct project (POS.WebAPI should be bold)

### Issue: Port already in use
**Solution:**
- Stop all running instances (Debug → Stop Debugging)
- Close any browser tabs with swagger
- Press F5 again

### Issue: Can't connect from mobile
**Solution:**
1. Check Windows Firewall:
   ```powershell
   netsh advfirewall firewall add rule name="POS API" dir=in action=allow protocol=TCP localport=5000
   ```
2. Verify port in VS Output window
3. Update mobile app API_BASE_URL to match

### Issue: Swagger opens but mobile can't connect
**Solution:**
- Swagger works = backend is running ✅
- Mobile can't connect = port mismatch or firewall
- Double-check mobile app uses `10.0.2.2:XXXX` not `localhost:XXXX`

## Running Multiple Projects

### Backend + Migrator
1. Right-click solution
2. Properties → Common Properties → Startup Project
3. Select "Multiple startup projects"
4. Set:
   - POS.WebAPI: Start
   - POS.Migrator: None (run separately when needed)

### Running Migrator
1. Right-click **POS.Migrator** project
2. Select **"Set as Startup Project"**
3. Press **Ctrl+F5** (run without debugging)
4. It will seed the database and close
5. Switch back to **POS.WebAPI** as startup project

## Hot Reload

Visual Studio 2022 supports Hot Reload:
- Make code changes while debugging
- Changes apply automatically (for most changes)
- Look for 🔥 icon in toolbar

## Output Windows to Watch

### Debug Output
- View → Output → Show output from: **Debug**
- Shows application logs (Console.WriteLine, ILogger)

### ASP.NET Core Web Server
- View → Output → Show output from: **ASP.NET Core Web Server**
- Shows HTTP requests and server startup messages

### Immediate Window
- Debug → Windows → Immediate
- Execute code while paused at breakpoint
- Example: `? user.Username` to see username

---

**That's it!** Now you can debug your backend properly in Visual Studio while testing the mobile app! 🎉
