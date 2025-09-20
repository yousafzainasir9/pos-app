@echo off
echo ========================================
echo    Rebuilding with Data Import Support
echo ========================================
echo.

echo [1/3] Restoring packages (including EPPlus for Excel support)...
dotnet restore --nologo

echo.
echo [2/3] Building solution...
dotnet build --nologo

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ Build failed!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo [3/3] Running database setup with data import...
cd src\POS.Migrator
dotnet run

cd ..\..
echo.
pause
