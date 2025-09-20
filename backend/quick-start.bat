@echo off
cls
echo ========================================
echo    Cookie Barrel POS API - Quick Start
echo ========================================
echo.

echo Checking SQL Server connection...
sqlcmd -S Zai -U sa -P 1234 -Q "SELECT 'Connected to SQL Server' as Status" -h -1
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Cannot connect to SQL Server
    echo Please check:
    echo   1. SQL Server 'Zai' is running
    echo   2. sa password is '1234'
    echo   3. SQL Server Authentication is enabled
    pause
    exit /b 1
)

echo.
echo Starting API...
echo ----------------------------------------

cd src\POS.WebAPI
start "POS API" dotnet run --urls "http://localhost:5000"

echo.
echo Waiting for API to start...
timeout /t 8 /nobreak > nul

echo.
echo ========================================
echo    API Started Successfully!
echo ========================================
echo.
echo Access Points:
echo   Swagger UI: http://localhost:5000/swagger
echo   Health:     http://localhost:5000/api/health
echo   API Base:   http://localhost:5000/api
echo.
echo Opening Swagger UI...
start http://localhost:5000/swagger

echo.
pause
