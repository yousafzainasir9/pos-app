@echo off
echo WARNING: This will delete all migrations and recreate them from scratch!
echo This should only be used in development environments.
echo.
echo Press Ctrl+C to cancel, or any other key to continue...
pause

cd src\POS.Infrastructure

echo.
echo Removing existing migrations...
if exist Data\Migrations (
    rmdir /s /q Data\Migrations
    echo Migrations folder deleted.
) else (
    echo No migrations folder found.
)

echo.
echo Creating fresh initial migration...
dotnet ef migrations add InitialCreate --startup-project ..\POS.WebAPI --context POSDbContext

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Failed to create migration!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Initial migration created successfully!
echo.
echo Now you can run the migrator to apply the changes.
pause
