@echo off
echo ================================================
echo POS Backend - Reports & Analytics Build Test
echo ================================================
cd /d "D:\pos-app\backend"

echo.
echo Cleaning solution...
dotnet clean --verbosity quiet

echo.
echo Building solution...
echo.
dotnet build --configuration Debug

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ================================================
    echo        BUILD SUCCESSFUL!
    echo ================================================
    echo.
    echo The Reports & Analytics feature is ready!
    echo.
    echo Backend API Endpoints Available:
    echo   - /api/reports/sales
    echo   - /api/reports/products
    echo   - /api/reports/shifts/current
    echo   - /api/reports/shifts/{id}
    echo   - /api/reports/export/{type}
    echo   - /api/reports/dashboard
    echo.
    echo To start the backend API:
    echo   dotnet run --project src\POS.WebAPI
    echo.
    echo To start the frontend:
    echo   cd ..\frontend
    echo   npm run dev
    echo.
    echo Access the application at:
    echo   Frontend: http://localhost:3000
    echo   Backend API: https://localhost:7139
    echo   Swagger Docs: https://localhost:7139/swagger
    echo.
) else (
    echo.
    echo ================================================
    echo        BUILD FAILED
    echo ================================================
    echo Please check the errors above.
)

pause
