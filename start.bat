@echo off
cls
echo ========================================
echo    Cookie Barrel POS - System Check
echo ========================================
echo.

echo [1/5] Checking SQL Server connection...
sqlcmd -S Zai -U sa -P 1234 -Q "SELECT DB_ID('POSDatabase') as DatabaseExists" -h -1 > nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo    X SQL Server connection FAILED
    echo    Please ensure SQL Server 'Zai' is running with sa/1234
    pause
    exit /b 1
) else (
    echo    ✓ SQL Server connected
)

echo.
echo [2/5] Checking if database exists...
sqlcmd -S Zai -U sa -P 1234 -Q "IF DB_ID('POSDatabase') IS NOT NULL SELECT 'EXISTS' ELSE SELECT 'NOT_EXISTS'" -h -1 > temp.txt 2>&1
set /p DB_STATUS=<temp.txt
del temp.txt

if "%DB_STATUS%"=="NOT_EXISTS" (
    echo    X Database does not exist
    echo    Running database setup...
    call backend\refresh-db.bat
) else (
    echo    ✓ Database exists
)

echo.
echo [3/5] Building backend...
cd backend
dotnet build --nologo --verbosity quiet > nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo    X Backend build FAILED
    cd ..
    pause
    exit /b 1
) else (
    echo    ✓ Backend built successfully
)
cd ..

echo.
echo [4/5] Checking frontend dependencies...
cd frontend
if not exist "node_modules" (
    echo    Installing frontend dependencies...
    npm install --silent
) else (
    echo    ✓ Frontend dependencies installed
)
cd ..

echo.
echo [5/5] Starting services...
echo ----------------------------------------
echo.

echo Starting Backend API on http://localhost:5000...
start "POS Backend" cmd /c "cd backend\src\POS.WebAPI && dotnet run --urls http://localhost:5000"

echo Waiting for backend to start...
timeout /t 8 /nobreak > nul

echo.
echo Starting Frontend on http://localhost:3000...
start "POS Frontend" cmd /c "cd frontend && npm run dev"

echo Waiting for frontend to start...
timeout /t 5 /nobreak > nul

echo.
echo ========================================
echo    System Started Successfully!
echo ========================================
echo.
echo Access Points:
echo   Frontend App:  http://localhost:3000
echo   Backend API:   http://localhost:5000/api
echo   Swagger Docs:  http://localhost:5000/swagger
echo   Health Check:  http://localhost:5000/api/health
echo.
echo Login Credentials:
echo   Username: admin     Password: Admin123!
echo   Username: manager   Password: Manager123!  
echo   Username: cashier1  Password: Cashier123!
echo.
echo Opening application in browser...
timeout /t 2 /nobreak > nul
start http://localhost:3000

echo.
echo Press any key to stop all services...
pause > nul

echo Stopping services...
taskkill /FI "WindowTitle eq POS Backend*" /T /F > nul 2>&1
taskkill /FI "WindowTitle eq POS Frontend*" /T /F > nul 2>&1
echo Services stopped.
pause
