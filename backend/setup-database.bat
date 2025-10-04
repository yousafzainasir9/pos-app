@echo off
echo ========================================
echo  POS Database Setup
echo ========================================
echo.
echo This will:
echo   1. Create the database (if not exists)
echo   2. Apply all migrations
echo   3. Seed initial data
echo.
echo NOTE: RefreshDatabase is set to TRUE in appsettings.json
echo       This will DROP and recreate the database!
echo.
pause

cd /d "%~dp0"
cd src\POS.Migrator

echo.
echo Running migrator...
echo.

dotnet run --configuration Debug

if errorlevel 1 (
    echo.
    echo ========================================
    echo  ERROR: Database setup failed!
    echo ========================================
    echo.
    echo Please check the error messages above.
    pause
    exit /b 1
)

echo.
echo ========================================
echo  Database setup completed successfully!
echo ========================================
echo.
echo You can now:
echo   1. Run 'run.bat' to start the API
echo   2. Test at https://localhost:7xxx/swagger
echo.
echo Default users:
echo   admin / Admin123!
echo   manager / Manager123!
echo   cashier1 / Cashier123!
echo.
pause
