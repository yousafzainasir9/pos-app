@echo off
REM ====================================================================
REM Cookie Barrel POS - WhatsApp Integration Build & Run Script
REM ====================================================================

echo.
echo ========================================
echo Cookie Barrel POS - WhatsApp Integration
echo ========================================
echo.

REM Check if .NET SDK is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [1/5] Cleaning previous builds...
cd /d "%~dp0backend\src\POS.WebAPI"
dotnet clean --nologo >nul 2>&1

echo [2/5] Restoring NuGet packages...
dotnet restore --nologo

echo [3/5] Building project...
dotnet build --configuration Release --nologo
if %errorlevel% neq 0 (
    echo.
    echo ====================================
    echo BUILD FAILED!
    echo ====================================
    pause
    exit /b 1
)

echo [4/5] Checking configuration...
if not exist "appsettings.Development.json" (
    echo.
    echo WARNING: appsettings.Development.json not found!
    echo Please configure WhatsApp settings before running.
    pause
)

echo [5/5] Starting API server...
echo.
echo ========================================
echo API is starting...
echo ========================================
echo.
echo URLs:
echo   http://localhost:5124
echo   https://localhost:7021
echo.
echo Swagger UI:
echo   http://localhost:5124/swagger
echo.
echo WhatsApp Endpoints:
echo   http://localhost:5124/api/whatsapptest/config
echo   http://localhost:5124/api/whatsappwebhook/health
echo.
echo Press Ctrl+C to stop the server
echo ========================================
echo.

dotnet run --configuration Release --no-build

pause
