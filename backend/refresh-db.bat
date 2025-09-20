@echo off
echo ========================================
echo    DEVELOPMENT DATABASE REFRESH
echo ========================================
echo.
echo Using SQL Server: Zai
echo Database: POSDatabase
echo.
echo This will DROP and RECREATE the database with fresh seed data.
echo.

cd src\POS.Migrator

echo Running database refresh...
echo.

dotnet run

cd ..\..

echo.
echo ========================================
echo Press any key to exit...
pause > nul
