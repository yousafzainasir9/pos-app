@echo off
echo Restoring NuGet packages for Cookie Barrel POS Backend...
echo.

cd src\POS.WebAPI

echo Cleaning previous builds...
dotnet clean

echo.
echo Restoring packages...
dotnet restore

echo.
echo Building solution...
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build failed! Please check the error messages above.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Build successful!
echo.
pause
