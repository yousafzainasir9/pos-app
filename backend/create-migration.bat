@echo off
echo ========================================
echo   POS Migration Generator
echo ========================================
echo.

if "%1"=="" (
    echo Usage: create-migration.bat [MigrationName]
    echo Example: create-migration.bat AddImageUrlToCategories
    echo.
    exit /b 1
)

echo Creating migration: %1
echo.

cd src\POS.Infrastructure
dotnet ef migrations add %1 -s ..\POS.WebAPI

if %errorlevel% neq 0 (
    echo.
    echo ❌ Migration creation failed!
    echo.
    pause
    exit /b 1
)

echo.
echo ✅ Migration created successfully!
echo.
echo Now running migrator to apply the migration...
echo.
cd ..\..\
cd src\POS.Migrator
dotnet run

pause
