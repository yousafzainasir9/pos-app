# POS System Database Configuration

## SQL Server Connection Details
- **Server Name**: Zai
- **Authentication**: SQL Server Authentication
- **Username**: sa
- **Password**: 1234
- **Database Name**: POSDatabase

## Connection String
```
Server=Zai;Database=POSDatabase;User Id=sa;Password=1234;TrustServerCertificate=True;MultipleActiveResultSets=true
```

## Configuration Files Updated

### Backend Applications
1. **POS.WebAPI**
   - `src\POS.WebAPI\appsettings.json`
   - `src\POS.WebAPI\appsettings.Development.json`

2. **POS.Migrator**
   - `src\POS.Migrator\appsettings.json`
   - `src\POS.Migrator\appsettings.Development.json`

## Quick Commands

### Test Connection
```bash
test-connection.bat
```

### Create/Refresh Database
```bash
refresh-db.bat
```

### Build Solution
```bash
restore-and-build.bat
```

### Run API
```bash
cd src\POS.WebAPI
dotnet run
```

## Default Users After Seeding

| Username | Password | PIN | Role |
|----------|----------|-----|------|
| admin | Admin123! | 9999 | Admin |
| manager | Manager123! | 1234 | Manager |
| cashier1 | Cashier123! | 1111 | Cashier |

## Troubleshooting

If database is not created:

1. **Check SQL Server Service**
   - Open Services (services.msc)
   - Look for "SQL Server (ZAI)"
   - Ensure it's running

2. **Check SQL Server Authentication**
   - Open SSMS
   - Right-click server "Zai" → Properties → Security
   - Ensure "SQL Server and Windows Authentication mode" is selected

3. **Check sa Account**
   - In SSMS, expand Security → Logins
   - Right-click "sa" → Properties
   - Ensure "Login is enabled" is checked
   - Verify password is set to "1234"

4. **Check TCP/IP Protocol**
   - Open SQL Server Configuration Manager
   - SQL Server Network Configuration → Protocols for ZAI
   - Ensure TCP/IP is Enabled

5. **Check Firewall**
   - Ensure Windows Firewall allows SQL Server connections
   - Default port is 1433

## API Endpoints

Once the database is created and API is running:

- **Base URL**: http://localhost:5000 or https://localhost:5001
- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

## Frontend Configuration

The frontend will connect to the API at:
- Development: http://localhost:5000
- Production: Configure as needed
