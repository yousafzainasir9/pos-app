@echo off
echo ========================================
echo    Cookie Barrel POS Database Setup
echo ========================================
echo.
echo Select an option:
echo 1. Refresh Database (Drop and recreate with fresh data)
echo 2. Update Database (Apply migrations only, keep existing data)
echo 3. Exit
echo.
set /p choice=Enter your choice (1-3): 

if "%choice%"=="1" goto refresh
if "%choice%"=="2" goto update
if "%choice%"=="3" goto exit
goto invalid

:refresh
echo.
echo WARNING: This will DELETE all existing data and recreate the database!
echo.
set /p confirm=Are you sure? (Y/N): 
if /i "%confirm%" neq "Y" goto exit

echo.
echo Refreshing database...
cd src\POS.Migrator

REM Set RefreshDatabase to true in appsettings
echo { > appsettings.json
echo   "ConnectionStrings": { >> appsettings.json
echo     "DefaultConnection": "Server=Zai;Database=POSDatabase;User Id=sa;Password=1234;TrustServerCertificate=True;MultipleActiveResultSets=true" >> appsettings.json
echo   }, >> appsettings.json
echo   "RefreshDatabase": true, >> appsettings.json
echo   "Logging": { >> appsettings.json
echo     "LogLevel": { >> appsettings.json
echo       "Default": "Information", >> appsettings.json
echo       "Microsoft": "Warning", >> appsettings.json
echo       "Microsoft.Hosting.Lifetime": "Information", >> appsettings.json
echo       "Microsoft.EntityFrameworkCore": "Warning" >> appsettings.json
echo     } >> appsettings.json
echo   } >> appsettings.json
echo } >> appsettings.json

dotnet run
goto end

:update
echo.
echo Updating database (keeping existing data)...
cd src\POS.Migrator

REM Set RefreshDatabase to false in appsettings
echo { > appsettings.json
echo   "ConnectionStrings": { >> appsettings.json
echo     "DefaultConnection": "Server=Zai;Database=POSDatabase;User Id=sa;Password=1234;TrustServerCertificate=True;MultipleActiveResultSets=true" >> appsettings.json
echo   }, >> appsettings.json
echo   "RefreshDatabase": false, >> appsettings.json
echo   "Logging": { >> appsettings.json
echo     "LogLevel": { >> appsettings.json
echo       "Default": "Information", >> appsettings.json
echo       "Microsoft": "Warning", >> appsettings.json
echo       "Microsoft.Hosting.Lifetime": "Information", >> appsettings.json
echo       "Microsoft.EntityFrameworkCore": "Warning" >> appsettings.json
echo     } >> appsettings.json
echo   } >> appsettings.json
echo } >> appsettings.json

dotnet run
goto end

:invalid
echo.
echo Invalid choice! Please select 1, 2, or 3.
pause
goto start

:exit
echo.
echo Exiting...
goto end

:end
cd ..\..
echo.
pause
