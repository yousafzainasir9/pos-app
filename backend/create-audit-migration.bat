@echo off
echo ========================================
echo   Creating Security & Audit Migration
echo ========================================
echo.

cd src\POS.Infrastructure

echo Creating EF Core migration...
dotnet ef migrations add AddAuditAndSecurityLogs -s ..\POS.WebAPI

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo   Migration Created Successfully!
    echo ========================================
    echo.
    echo Next steps:
    echo 1. Review the migration file in Migrations folder
    echo 2. Run 'update-database.bat' to apply migration
    echo 3. Or run POS.Migrator to rebuild database
    echo.
) else (
    echo.
    echo ========================================
    echo   Migration Creation Failed!
    echo ========================================
    echo.
    echo Please check the error messages above.
    echo.
)

pause
