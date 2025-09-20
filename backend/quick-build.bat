@echo off
echo Building POS Backend...
cd /d "D:\pos-app\backend"

dotnet build --configuration Debug --verbosity quiet

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ===== BUILD SUCCESSFUL =====
    echo Reports API is ready!
    echo.
) else (
    echo.
    echo ===== BUILD FAILED =====
    echo Fix the errors above.
)

pause
