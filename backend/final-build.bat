@echo off
echo ========================================
echo Building POS Backend - Final Test
echo ========================================
cd /d "D:\pos-app\backend"

echo.
echo [1/3] Cleaning previous build...
dotnet clean > nul 2>&1

echo [2/3] Restoring packages...
dotnet restore

echo [3/3] Building solution...
dotnet build --configuration Debug

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo BUILD SUCCESSFUL!
    echo ========================================
    echo.
    echo Reports & Analytics Backend Ready:
    echo.
    echo API Endpoints:
    echo   /api/reports/sales          - Sales analytics
    echo   /api/reports/products       - Product performance  
    echo   /api/reports/shifts/current - Current shift report
    echo   /api/reports/shifts/{id}    - Specific shift report
    echo   /api/reports/export/{type}  - CSV exports
    echo   /api/reports/dashboard      - Dashboard stats
    echo.
    echo To start the API server:
    echo   dotnet run --project src\POS.WebAPI
    echo.
    echo Then access:
    echo   https://localhost:7139/swagger
    echo.
) else (
    echo.
    echo ========================================
    echo BUILD FAILED - See errors above
    echo ========================================
)

pause
