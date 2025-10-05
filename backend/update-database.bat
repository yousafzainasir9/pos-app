@echo off
echo ========================================
echo   Updating Database with Audit Tables
echo ========================================
echo.

cd src\POS.Infrastructure

echo Applying migration to database...
dotnet ef database update -s ..\POS.WebAPI

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo   Database Updated Successfully!
    echo ========================================
    echo.
    echo Tables created:
    echo - AuditLogs
    echo - SecurityLogs
    echo.
    echo You can now:
    echo 1. Start the API: run.bat
    echo 2. Start the frontend: cd frontend ^&^& npm run dev
    echo 3. Navigate to /admin/security
    echo.
) else (
    echo.
    echo ========================================
    echo   Database Update Failed!
    echo ========================================
    echo.
    echo Please check the error messages above.
    echo.
)

pause
