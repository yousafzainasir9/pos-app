@echo off
echo Installing Cookie Barrel POS Frontend Dependencies...
echo.

echo Installing npm packages...
call npm install

echo.
echo Starting development server...
echo The application will be available at:
echo - http://localhost:3000
echo.
echo Make sure the backend API is running on http://localhost:5001
echo.
echo Press Ctrl+C to stop the server.
echo.

npm run dev
