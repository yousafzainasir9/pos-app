@echo off
echo ========================================
echo  Fixing React Native Build Issues
echo ========================================
echo.

echo [1/5] Stopping Metro bundler...
taskkill /F /IM node.exe 2>nul
timeout /t 2 >nul

echo [2/5] Cleaning Metro cache...
if exist %TEMP%\metro-* (
    rmdir /s /q %TEMP%\metro-* 2>nul
)
if exist %TEMP%\react-* (
    rmdir /s /q %TEMP%\react-* 2>nul
)

echo [3/5] Cleaning build folders...
if exist android\app\build (
    rmdir /s /q android\app\build
)
if exist android\.gradle (
    rmdir /s /q android\.gradle
)

echo [4/5] Cleaning node_modules cache...
call npm cache clean --force

echo [5/5] Starting fresh Metro bundler with reset cache...
echo.
echo ========================================
echo  Cache cleared! Starting Metro bundler...
echo ========================================
echo.
call npx react-native start --reset-cache

pause
