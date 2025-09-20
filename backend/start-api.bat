@echo off
echo ========================================
echo    Starting Cookie Barrel POS API
echo ========================================
echo.

cd src\POS.WebAPI

echo Starting API on http://localhost:5000
echo Swagger UI will be available at: http://localhost:5000/swagger
echo.
echo Press Ctrl+C to stop the API
echo.

dotnet run --launch-profile http

pause
