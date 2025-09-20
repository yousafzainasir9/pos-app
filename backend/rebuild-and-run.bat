@echo off
echo ========================================
echo    Rebuilding POS API
echo ========================================
echo.

cd src\POS.WebAPI

echo Cleaning...
dotnet clean --nologo --verbosity quiet

echo.
echo Restoring packages...
dotnet restore --nologo --verbosity quiet

echo.
echo Building...
dotnet build --nologo

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ Build failed!
    cd ..\..
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ✅ Build successful!
echo.
echo Starting API on http://localhost:5000...
echo.
dotnet run --urls http://localhost:5000

cd ..\..
