# Cookie Barrel POS System

A comprehensive Point of Sale (POS) system for Cookie Barrel bakery chain built with .NET 9 and React.

## 📋 Table of Contents
- [Overview](#overview)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Database Setup](#database-setup)
- [Installation](#installation)
- [Running the Application](#running-the-application)
- [Default Users](#default-users)
- [Features](#features)
- [Project Structure](#project-structure)

## Overview

Cookie Barrel POS is a full-featured point of sale system designed for bakery operations. It includes inventory management, customer loyalty programs, multi-store support, and comprehensive reporting.

## Architecture

The system follows Clean Architecture principles with the following layers:
- **Domain**: Core entities and business rules
- **Infrastructure**: Data access, external services
- **Application**: Business logic and use cases
- **WebAPI**: REST API endpoints
- **Frontend**: React-based user interface

## Prerequisites

- **Backend:**
  - .NET 9 SDK or later
  - SQL Server 2019 or later (LocalDB, Express, or Full)
  - Visual Studio 2022 or VS Code

- **Frontend:**
  - Node.js 18+ and npm

## Database Setup

The project includes a Database Migrator tool that automatically creates and seeds your database with comprehensive test data.

### Step 1: Configure Database Connection

1. Navigate to `backend/src/POS.WebAPI/`
2. Open `appsettings.Development.json`
3. Update the connection string to point to your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=POSDatabase;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
}
```

Replace `YOUR_SERVER_NAME` with:
- `(localdb)\\mssqllocaldb` for LocalDB
- `.\\SQLEXPRESS` for SQL Express
- Your server name for full SQL Server

### Step 2: Run the Database Migrator

The migrator will create the database, apply all migrations, and seed it with test data:

```bash
cd backend/src/POS.Migrator
dotnet run
```

#### What the Migrator Does:

1. **Creates the database** if it doesn't exist
2. **Applies all migrations** to create tables and relationships
3. **Seeds comprehensive test data** including:
   - 3 Stores (Main, Westfield, Airport)
   - 13 Users (Admin, Managers, Cashiers)
   - 100+ Customers with loyalty data
   - 200+ Products from Cookie Barrel catalog
   - 30 days of operational history
   - 3,000+ Orders with payment records
   - Complete inventory transactions
   - Shift records with sales data

#### Migrator Options:

The migrator behavior can be controlled via `appsettings.json`:

```json
{
  "RefreshDatabase": true  // Set to false to keep existing data
}
```

- `true` (default): Drops and recreates the database with fresh data
- `false`: Only applies new migrations, keeps existing data

### Step 3: Verify Database Creation

After running the migrator, you should see:

```
========================================
   DATABASE SETUP COMPLETED ✅
========================================
✅ Database dropped and recreated
✅ Migrations applied
✅ Seed data inserted

Database Statistics:
  • Stores:        3
  • Users:         13
  • Categories:    8
  • Subcategories: 35+
  • Products:      200+
  • Customers:     102
  • Suppliers:     4
  • Orders:        3000+
  • Payments:      3000+
  • Inventory:     1000+ transactions
```

## Installation

### Backend Setup

1. Clone the repository:
```bash
git clone https://github.com/yourusername/pos-app.git
cd pos-app
```

2. Restore NuGet packages:
```bash
cd backend
dotnet restore
```

3. Build the solution:
```bash
dotnet build
```

### Frontend Setup

1. Navigate to frontend directory:
```bash
cd frontend
```

2. Install npm packages:
```bash
npm install
```

## Running the Application

### Start Backend API

```bash
cd backend/src/POS.WebAPI
dotnet run
```

The API will be available at: `https://localhost:7124` or `http://localhost:5124`

### Start Frontend

In a new terminal:
```bash
cd frontend
npm run dev
```

The frontend will be available at: `http://localhost:5173`

## Default Users

After running the migrator, these users are available for testing:

| Role | Username | Password | PIN | Description |
|------|----------|----------|-----|-------------|
| Admin | admin | Admin123! | 9999 | Full system access |
| Manager | manager1 | Manager123! | 1001 | Store management |
| Cashier | cashier1 | Cashier123! | 2001 | POS operations |
| Cashier | cashier2 | Cashier123! | 2002 | POS operations |

Each store has its own manager and cashiers. PIN login provides quick access for cashiers.

## Features

### Core Functionality
- ✅ Multi-store support
- ✅ User role management (Admin, Manager, Cashier)
- ✅ Product catalog with categories
- ✅ Inventory tracking
- ✅ Customer loyalty program
- ✅ Order processing
- ✅ Payment handling (Cash, Card, Split payments)
- ✅ Shift management
- ✅ GST/Tax calculations

### Reporting
- Sales reports by date/store
- Inventory reports
- Shift reconciliation
- Customer purchase history

### Security
- JWT authentication
- Role-based authorization
- PIN-based quick login
- Audit trails

## Project Structure

```
pos-app/
├── backend/
│   ├── src/
│   │   ├── POS.Domain/          # Entities, Enums, Common
│   │   ├── POS.Application/     # Business logic, DTOs
│   │   ├── POS.Infrastructure/  # Data, Services, Seeders
│   │   ├── POS.WebAPI/         # API Controllers, Middleware
│   │   └── POS.Migrator/       # Database setup tool
│   └── tests/
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── services/
│   │   └── utils/
│   └── public/
└── documentation/
    └── cookie_barrel_catalog_devseed_2025-09-19.json  # Product catalog
```

## Development Tips

### Resetting the Database

To reset the database with fresh data:

```bash
cd backend/src/POS.Migrator
dotnet run
```

The migrator will drop the existing database and create a new one with fresh test data.

### Adding New Migrations

When you modify domain entities:

```bash
cd backend/src/POS.Infrastructure
dotnet ef migrations add YourMigrationName -s ../POS.WebAPI
```

### Viewing the Database

You can use:
- SQL Server Management Studio (SSMS)
- Azure Data Studio
- Visual Studio SQL Server Object Explorer

Connect using the connection string from `appsettings.Development.json`.

## Troubleshooting

### Database Connection Issues

1. Ensure SQL Server is running
2. Check the connection string in `appsettings.Development.json`
3. Verify SQL Server authentication settings

### Migrator Errors

If the migrator fails:
1. Check SQL Server permissions
2. Ensure the connection string is correct
3. Try running Visual Studio/terminal as Administrator
4. Check the logs for detailed error messages

### Port Conflicts

If ports are already in use, update:
- Backend: `launchSettings.json` in POS.WebAPI
- Frontend: `vite.config.ts`

## Support

For issues or questions, please check the documentation folder or create an issue in the repository.

## License

[Your License Here]
