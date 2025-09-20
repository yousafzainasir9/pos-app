@echo off
echo Creating new migration for pending model changes...
echo.

cd src\POS.Infrastructure

echo Adding migration...
dotnet ef migrations add UpdateModelChanges --startup-project ..\POS.WebAPI --context POSDbContext

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Failed to create migration!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Migration created successfully!
echo.
echo Now you can run the migrator to apply the changes.
pause
