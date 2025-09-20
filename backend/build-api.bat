@echo off
echo ========================================
echo    Restoring Packages and Building
echo ========================================
echo.

cd src\POS.WebAPI

echo Cleaning solution...
dotnet clean

echo.
echo Restoring NuGet packages...
dotnet restore

echo.
echo Building solution...
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build failed! Please check the error messages above.
    cd ..\..
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Build successful!
cd ..\..
echo.
pause
