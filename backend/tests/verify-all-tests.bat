@echo off
COLOR 0A
echo.
echo ========================================================
echo    POS BACKEND - UNIT TEST SUITE VERIFICATION
echo ========================================================
echo.
echo Status: Checking test implementation...
echo.

REM Check if test projects exist
if not exist "tests\POS.Infrastructure.Tests\POS.Infrastructure.Tests.csproj" (
    COLOR 0C
    echo [ERROR] POS.Infrastructure.Tests project not found!
    pause
    exit /b 1
)

if not exist "tests\POS.WebAPI.Tests\POS.WebAPI.Tests.csproj" (
    COLOR 0C
    echo [ERROR] POS.WebAPI.Tests project not found!
    pause
    exit /b 1
)

echo [OK] Test projects found
echo.

REM Build the solution
echo Building solution...
dotnet build --nologo --verbosity quiet
if %ERRORLEVEL% NEQ 0 (
    COLOR 0C
    echo [ERROR] Build failed!
    pause
    exit /b 1
)
echo [OK] Build successful
echo.

REM Count test files
echo Counting test files...
set infraTests=0
set apiTests=0

for /f %%i in ('dir /b /s "tests\POS.Infrastructure.Tests\*Tests.cs" 2^>nul ^| find /c /v ""') do set infraTests=%%i
for /f %%i in ('dir /b /s "tests\POS.WebAPI.Tests\*Tests.cs" 2^>nul ^| find /c /v ""') do set apiTests=%%i

echo [OK] Infrastructure test files: %infraTests%
echo [OK] API test files: %apiTests%
echo.

REM Run tests and capture results
echo Running all tests...
echo.
dotnet test --nologo --verbosity normal --no-build

if %ERRORLEVEL% EQU 0 (
    COLOR 0A
    echo.
    echo ========================================================
    echo    ALL TESTS PASSED! 
    echo ========================================================
    echo.
    echo Test Suite Summary:
    echo - Infrastructure Tests: %infraTests% files
    echo - API Tests: %apiTests% files
    echo - Status: PASSING
    echo - Ready for: PRODUCTION
    echo.
    echo Next steps:
    echo 1. Review coverage report: .\build-and-test.bat
    echo 2. Commit test suite to version control
    echo 3. Configure CI/CD pipeline
    echo.
    echo Documentation: tests\FINAL_COMPLETE_SUMMARY.md
    echo.
) else (
    COLOR 0C
    echo.
    echo ========================================================
    echo    SOME TESTS FAILED
    echo ========================================================
    echo.
    echo Please check the test output above for details.
    echo.
)

pause
