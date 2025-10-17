@echo off
echo ============================================
echo Unit Test Setup Verification
echo ============================================
echo.

echo Step 1: Building Infrastructure Tests...
cd tests\POS.Infrastructure.Tests
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Infrastructure Tests build failed!
    pause
    exit /b 1
)
echo ✓ Infrastructure Tests built successfully
echo.

echo Step 2: Building WebAPI Tests...
cd ..\POS.WebAPI.Tests
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: WebAPI Tests build failed!
    pause
    exit /b 1
)
echo ✓ WebAPI Tests built successfully
echo.

cd ..\..

echo Step 3: Running All Tests...
dotnet test --no-build
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: Some tests may have failed
) else (
    echo ✓ All tests passed successfully
)
echo.

echo ============================================
echo Setup Verification Complete!
echo ============================================
echo.
echo Next Steps:
echo 1. Review tests\DAY1_SETUP_COMPLETE.md
echo 2. Proceed with Day 2 implementation
echo.
pause
