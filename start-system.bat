@echo off
echo ========================================
echo    Cookie Barrel POS - Full System
echo ========================================
echo.
echo Starting Backend and Frontend...
echo.

echo [1/3] Starting Backend API (Port 5000)...
start "POS Backend API" cmd /k "cd backend\src\POS.WebAPI && dotnet run --urls http://localhost:5000"

timeout /t 5 /nobreak > nul

echo [2/3] Starting Frontend (Port 3000)...
start "POS Frontend" cmd /k "cd frontend && npm run dev"

timeout /t 5 /nobreak > nul

echo [3/3] Opening applications in browser...
echo.

echo ========================================
echo    System Started Successfully!
echo ========================================
echo.
echo Access Points:
echo   Frontend:    http://localhost:3000
echo   Backend API: http://localhost:5000/api
echo   Swagger UI:  http://localhost:5000/swagger
echo   Health:      http://localhost:5000/api/health
echo.
echo Default Login Credentials:
echo   Admin:    admin / Admin123!
echo   Manager:  manager / Manager123!
echo   Cashier:  cashier1 / Cashier123!
echo.
echo Opening Frontend in browser...
start http://localhost:3000
echo.
echo Press any key to stop all services...
pause > nul

echo.
echo Stopping services...
taskkill /FI "WindowTitle eq POS Backend API*" /T /F
taskkill /FI "WindowTitle eq POS Frontend*" /T /F
echo Services stopped.
pause
