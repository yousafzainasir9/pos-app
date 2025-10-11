@echo off
echo ===================================
echo Cookie Barrel Mobile App Setup
echo ===================================
echo.

echo Step 1: Installing Dependencies...
echo.
call npm install

echo.
echo Step 2: Clearing cache...
echo.
call npm start -- --reset-cache &

echo.
echo ===================================
echo Setup Complete!
echo ===================================
echo.
echo Metro bundler is starting...
echo Open another terminal and run: npm run android
echo.
echo Press Ctrl+C to stop Metro bundler
echo ===================================
pause
