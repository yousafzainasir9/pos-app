@echo off
echo ===================================
echo Running Cookie Barrel Mobile App
echo ===================================
echo.
echo Starting Metro Bundler...
echo.
start cmd /k "npm start"

timeout /t 5 /nobreak

echo.
echo Starting Android App...
echo.
call npx react-native run-android

echo.
echo App should be running on your device/emulator
echo.
pause
