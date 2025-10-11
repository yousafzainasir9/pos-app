@echo off
echo Creating Cookie Barrel App Icons...
echo.
echo This will generate simple app icons with "CB" text.
echo.

REM We need to use ImageMagick or a similar tool to create PNGs
REM For now, let's use the online tool

echo Please follow these steps:
echo.
echo 1. Go to: https://icon.kitchen/
echo 2. Click "Icon" and choose a simple design
echo 3. Set background color to: #D97706 (orange)
echo 4. Add text "CB" in white
echo 5. Download for Android
echo 6. Extract the ZIP
echo 7. Copy all 'mipmap-*' folders to:
echo    D:\pos-app\mobileApp\android\app\src\main\res\
echo 8. Replace all existing folders
echo.
echo Then rebuild the app:
echo    cd D:\pos-app\mobileApp
echo    npm run android
echo.
pause
