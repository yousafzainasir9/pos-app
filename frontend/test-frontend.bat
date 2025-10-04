@echo off
echo ========================================
echo POS Frontend - Quick Test
echo ========================================
echo.

cd frontend

echo [1/4] Checking Node modules...
if not exist "node_modules\" (
    echo Installing dependencies...
    call npm install
) else (
    echo Dependencies already installed.
)
echo.

echo [2/4] Running TypeScript check...
call npm run build
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: TypeScript compilation failed!
    pause
    exit /b 1
)
echo TypeScript check PASSED!
echo.

echo [3/4] Build successful!
echo.

echo [4/4] Starting development server...
echo.
echo ========================================
echo Frontend will start at: http://localhost:5173
echo Backend should be at: https://localhost:7021
echo ========================================
echo.
echo Test Login Credentials:
echo   Username: admin
echo   Password: Admin123!
echo.
echo Or PIN Login:
echo   PIN: 9999
echo   Store: Main Store
echo ========================================
echo.

call npm run dev

pause
