@echo off
echo ========================================
echo    Restoring Packages and Building
echo ========================================
echo.

echo Cleaning solution...
dotnet clean

echo.
echo Restoring NuGet packages...
dotnet restore

echo.
echo Building solution...
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ Build failed! Please check the error messages above.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ✅ Build successful!
echo.
echo You can now run refresh-db.bat to setup the database.
echo.
pause
