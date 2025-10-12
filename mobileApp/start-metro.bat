@echo off
echo ========================================
echo  QUICK START SCRIPT
echo ========================================
echo.
echo This will start Metro bundler with clean cache
echo.

echo [1/2] Stopping any running Metro processes...
taskkill /F /IM node.exe 2>nul
timeout /t 2 >nul

echo [2/2] Starting Metro bundler with fresh cache...
echo.
echo ========================================
call npx react-native start --reset-cache

pause
