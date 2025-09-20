@echo off
echo Building Cookie Barrel POS Backend...
echo.

cd src\POS.WebAPI

echo Restoring packages...
dotnet restore

echo.
echo Building project...
dotnet build

echo.
echo Creating database migrations...
dotnet ef migrations add InitialCreate -p ..\POS.Infrastructure -s . --no-build

echo.
echo Applying database migrations...
dotnet ef database update --no-build

echo.
echo Starting API...
echo API will be available at:
echo - https://localhost:7001 (HTTPS)
echo - http://localhost:5001 (HTTP)
echo - Swagger UI: https://localhost:7001/swagger
echo.
echo Press Ctrl+C to stop the server.
echo.

set ASPNETCORE_URLS=https://localhost:7001;http://localhost:5001
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --no-build
