@echo off
echo ========================================
echo POS System - Complete Start
echo ========================================
echo.
echo Starting both backend and frontend...
echo.

echo [1/2] Starting Backend...
start "POS Backend" cmd /k "cd backend && fix-and-run.bat"
timeout /t 5 /nobreak >nul

echo [2/2] Starting Frontend...
start "POS Frontend" cmd /k "cd frontend && npm run dev"

echo.
echo ========================================
echo Both services are starting!
echo ========================================
echo.
echo Backend:  https://localhost:7021
echo Frontend: http://localhost:5173
echo Swagger:  https://localhost:7021/swagger
echo.
echo Test Login:
echo   Username: admin
echo   Password: Admin123!
echo.
echo ========================================
echo.
echo Two new windows have opened:
echo 1. Backend API (port 7021)
echo 2. Frontend Dev Server (port 5173)
echo.
echo Close those windows to stop the services.
echo.
pause
