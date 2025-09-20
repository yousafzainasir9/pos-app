@echo off
echo ========================================
echo    Cookie Barrel POS - Full Startup
echo ========================================
echo.

echo Step 1: Building the solution...
echo ----------------------------------------
call restore-and-build.bat
if %ERRORLEVEL% NEQ 0 (
    echo Build failed! Please fix errors before continuing.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Step 2: Checking database...
echo ----------------------------------------
powershell -ExecutionPolicy Bypass -Command "& {
    $connectionString = 'Server=Zai;Database=POSDatabase;User Id=sa;Password=1234;TrustServerCertificate=True'
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = 'SELECT DB_ID(''POSDatabase'')'
        $result = $command.ExecuteScalar()
        if ($result -eq [System.DBNull]::Value) {
            Write-Host 'Database does not exist. Creating...' -ForegroundColor Yellow
            exit 1
        } else {
            Write-Host 'Database exists!' -ForegroundColor Green
            exit 0
        }
    } catch {
        Write-Host 'Cannot connect to SQL Server' -ForegroundColor Red
        exit 2
    }
}"

if %ERRORLEVEL% EQU 1 (
    echo.
    echo Creating and seeding database...
    call refresh-db.bat
)

if %ERRORLEVEL% EQU 2 (
    echo.
    echo ERROR: Cannot connect to SQL Server 'Zai'
    echo Please ensure SQL Server is running and sa password is correct.
    pause
    exit /b 1
)

echo.
echo Step 3: Starting the API...
echo ----------------------------------------
echo.
echo The API will start in a new window.
echo.
echo URLs:
echo   - API:     http://localhost:5000
echo   - Swagger: http://localhost:5000/swagger
echo   - Health:  http://localhost:5000/health
echo.
echo Default Login Credentials:
echo   Admin:    admin / Admin123!
echo   Manager:  manager / Manager123!
echo   Cashier:  cashier1 / Cashier123!
echo.

start "Cookie Barrel POS API" cmd /k "cd src\POS.WebAPI && dotnet run --launch-profile http"

timeout /t 5 /nobreak > nul

echo Opening Swagger UI in browser...
start http://localhost:5000/swagger

echo.
echo ========================================
echo    System Started Successfully!
echo ========================================
echo.
echo Press any key to exit (API will continue running)...
pause > nul
