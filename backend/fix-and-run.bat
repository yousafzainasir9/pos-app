@echo off
echo ========================================
echo POS Backend - Quick Fix and Rebuild
echo ========================================
echo.
echo Fixing ISecurityService registration...
echo.

cd src\POS.WebAPI

echo [1/3] Cleaning previous build...
dotnet clean
echo.

echo [2/3] Building project...
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)
echo Build successful!
echo.

echo [3/3] Starting API...
echo.
echo ========================================
echo API will start at: https://localhost:7021
echo Swagger docs at: https://localhost:7021/swagger
echo ========================================
echo.
echo Press Ctrl+C to stop the server
echo.

dotnet run

pause
