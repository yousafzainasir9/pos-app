@echo off
echo ========================================
echo  COMPREHENSIVE FIX FOR BUILD ERRORS
echo ========================================
echo.
echo This will:
echo  1. Stop all Node processes
echo  2. Clean all caches
echo  3. Clean build folders
echo  4. Reinstall dependencies
echo  5. Rebuild the app
echo.
pause

echo.
echo [Step 1/7] Killing Node processes...
taskkill /F /IM node.exe 2>nul
timeout /t 2 >nul

echo [Step 2/7] Cleaning Metro bundler cache...
if exist %TEMP%\metro-* (
    echo   Removing Metro cache...
    rmdir /s /q %TEMP%\metro-* 2>nul
)
if exist %TEMP%\react-* (
    echo   Removing React cache...
    rmdir /s /q %TEMP%\react-* 2>nul
)
if exist %TEMP%\haste-* (
    echo   Removing Haste cache...
    rmdir /s /q %TEMP%\haste-* 2>nul
)

echo [Step 3/7] Cleaning Android build folders...
if exist android\app\build (
    echo   Removing app build folder...
    rmdir /s /q android\app\build
)
if exist android\build (
    echo   Removing build folder...
    rmdir /s /q android\build
)
if exist android\.gradle (
    echo   Removing Gradle cache...
    rmdir /s /q android\.gradle
)

echo [Step 4/7] Cleaning npm cache...
call npm cache clean --force

echo [Step 5/7] Removing node_modules...
if exist node_modules (
    echo   This may take a minute...
    rmdir /s /q node_modules
)

echo [Step 6/7] Reinstalling dependencies...
echo   This will take a few minutes...
call npm install

echo [Step 7/7] Cleaning Gradle...
cd android
call gradlew clean
cd ..

echo.
echo ========================================
echo  Cleanup complete!
echo ========================================
echo.
echo Now run: npx react-native run-android
echo.
pause
