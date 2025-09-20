@echo off
echo Building POS Backend with Reports...
cd /d "D:\pos-app\backend"

echo.
echo Restoring NuGet packages...
dotnet restore

echo.
echo Building solution...
dotnet build --configuration Debug

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build successful!
    echo Reports API endpoints are now available at:
    echo   - GET /api/reports/sales
    echo   - GET /api/reports/products
    echo   - GET /api/reports/shifts/current
    echo   - GET /api/reports/shifts/{id}
    echo   - GET /api/reports/export/{type}
    echo   - GET /api/reports/dashboard
) else (
    echo.
    echo Build failed! Check the errors above.
)

pause
