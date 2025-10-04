# 🔧 Complete Error Fix Script

## Run these commands in order:

### Step 1: Delete Duplicate Files
```powershell
cd D:\pos-app\backend\src\POS.Application\DTOs\Auth

# Remove duplicate individual DTO files (we're keeping AuthDtos.cs)
if (Test-Path "LoginRequestDto.cs") { Remove-Item "LoginRequestDto.cs" -Force }
if (Test-Path "PinLoginRequestDto.cs") { Remove-Item "PinLoginRequestDto.cs" -Force }
if (Test-Path "RefreshTokenRequestDto.cs") { Remove-Item "RefreshTokenRequestDto.cs" -Force }
if (Test-Path "LoginResponseDto.cs") { Remove-Item "LoginResponseDto.cs" -Force }
if (Test-Path "UserDto.cs") { Remove-Item "UserDto.cs" -Force }

Write-Host "✅ Duplicate files deleted" -ForegroundColor Green
```

### Step 2: Clean Solution
```powershell
cd D:\pos-app\backend

Write-Host "Cleaning solution..." -ForegroundColor Yellow
dotnet clean

Write-Host "Deleting bin and obj folders..." -ForegroundColor Yellow
Get-ChildItem -Include bin,obj -Recurse | Remove-Item -Force -Recurse

Write-Host "✅ Solution cleaned" -ForegroundColor Green
```

### Step 3: Restore and Build
```powershell
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore

Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build

Write-Host "✅ Build complete!" -ForegroundColor Green
```

---

## OR Run This Single Command:

Copy and paste this entire block into PowerShell:

```powershell
# Complete fix in one go
cd D:\pos-app\backend

# Step 1: Remove duplicates
cd src\POS.Application\DTOs\Auth
Remove-Item LoginRequestDto.cs,PinLoginRequestDto.cs,RefreshTokenRequestDto.cs,LoginResponseDto.cs,UserDto.cs -Force -ErrorAction SilentlyContinue

# Step 2: Clean
cd D:\pos-app\backend
dotnet clean
Get-ChildItem -Include bin,obj -Recurse | Remove-Item -Force -Recurse

# Step 3: Rebuild
dotnet restore
dotnet build

Write-Host "✅ All done! Check for errors." -ForegroundColor Green
```

---

## Expected Result:

After running these commands, you should see:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## If You Still Get Errors:

### Check for these common issues:

1. **Visual Studio locks files**: Close Visual Studio before running cleanup
2. **Multiple solutions open**: Close all VS instances
3. **File permissions**: Run PowerShell as Administrator

### Manual cleanup if script fails:

1. Close Visual Studio completely
2. Manually delete these folders:
   - `D:\pos-app\backend\src\POS.Application\bin`
   - `D:\pos-app\backend\src\POS.Application\obj`
   - `D:\pos-app\backend\src\POS.Infrastructure\bin`
   - `D:\pos-app\backend\src\POS.Infrastructure\obj`
   - `D:\pos-app\backend\src\POS.WebAPI\bin`
   - `D:\pos-app\backend\src\POS.WebAPI\obj`
   - `D:\pos-app\backend\src\POS.Domain\bin`
   - `D:\pos-app\backend\src\POS.Domain\obj`
   - `D:\pos-app\backend\src\POS.Migrator\bin`
   - `D:\pos-app\backend\src\POS.Migrator\obj`

3. Manually delete these duplicate files in `DTOs\Auth\`:
   - `LoginRequestDto.cs`
   - `PinLoginRequestDto.cs`
   - `RefreshTokenRequestDto.cs`
   - `LoginResponseDto.cs`
   - `UserDto.cs`

4. Reopen Visual Studio
5. Build → Rebuild Solution

---

## ✅ After Successful Build:

Once you have 0 errors, you can proceed with:
1. Updating Program.cs
2. Replacing AuthController
3. Testing the implementation

See: **IMPLEMENTATION_CHECKLIST.md** for next steps.
