@echo off
echo Building POS Backend - Testing Reports Integration...
cd /d "D:\pos-app\backend"

echo.
echo Cleaning previous build...
dotnet clean

echo.
echo Restoring NuGet packages...
dotnet restore

echo.
echo Building solution...
dotnet build --configuration Debug --verbosity minimal

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo BUILD SUCCESSFUL!
    echo ========================================
    echo.
    echo Reports API is ready with endpoints:
    echo   - GET /api/reports/sales
    echo   - GET /api/reports/products  
    echo   - GET /api/reports/shifts/current
    echo   - GET /api/reports/shifts/{id}
    echo   - GET /api/reports/export/{type}
    echo   - GET /api/reports/dashboard
    echo.
    echo To run the API:
    echo   dotnet run --project src\POS.WebAPI
    echo.
) else (
    echo.
    echo ========================================
    echo BUILD FAILED!
    echo ========================================
    echo Check the errors above for details.
)

pause
