# Cookie Barrel POS Backend

## Overview
This is the backend API for the Cookie Barrel Point of Sale (POS) system, built with .NET 9 following Clean Architecture principles and using Entity Framework Core with Code First approach.

## Architecture

### Project Structure
- **POS.Domain**: Core business entities and enums (no dependencies)
- **POS.Application**: Business logic, DTOs, interfaces, and application services
- **POS.Infrastructure**: Data access, EF Core DbContext, repositories, and external services
- **POS.WebAPI**: REST API endpoints, authentication, and dependency injection configuration

### Technologies Used
- .NET 9.0
- Entity Framework Core 9.0 (Code First)
- SQL Server
- JWT Authentication
- BCrypt for password hashing
- Serilog for logging
- Swagger/OpenAPI for API documentation
- AutoMapper for object mapping

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Installation

1. Navigate to the backend directory:
```bash
cd D:\pos-app\backend
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Update the connection string in `src\POS.WebAPI\appsettings.json`:
```json
"ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=POSDatabase;Trusted_Connection=True"
}
```

4. Create and apply database migrations:
```bash
cd src\POS.WebAPI
dotnet ef migrations add InitialCreate -p ../POS.Infrastructure -s .
dotnet ef database update
```

5. Run the application:
```bash
dotnet run
```

The API will be available at:
- https://localhost:7xxx (HTTPS)
- http://localhost:5xxx (HTTP)
- Swagger UI: https://localhost:7xxx/swagger

## Database Schema

### Core Entities
- **Category**: Product categories (e.g., Cookies, Cakes, Muffins)
- **Subcategory**: Category subdivisions (e.g., Cafe Cookies, Kids Cookies)
- **Product**: Individual products with pricing and inventory
- **Customer**: Customer information and loyalty tracking
- **Order**: Sales transactions
- **OrderItem**: Line items in orders
- **Payment**: Payment records for orders
- **User**: System users (admin, manager, cashier)
- **Store**: Store information and settings
- **Shift**: Cash register shifts
- **InventoryTransaction**: Stock movements

### Features
- Soft delete support (IsDeleted flag)
- Audit fields (CreatedOn, ModifiedOn, etc.)
- GST calculation (10% Australian GST)
- Inventory tracking
- Multi-payment support
- Shift management

## API Endpoints

### Authentication
- POST `/api/auth/login` - User login
- POST `/api/auth/refresh` - Refresh JWT token
- POST `/api/auth/logout` - User logout

### Products
- GET `/api/products` - Get all products (with filtering)
- GET `/api/products/{id}` - Get product by ID
- GET `/api/products/by-barcode/{barcode}` - Get product by barcode
- POST `/api/products` - Create new product
- PUT `/api/products/{id}` - Update product
- DELETE `/api/products/{id}` - Delete product (soft delete)

### Categories
- GET `/api/categories` - Get all categories
- GET `/api/categories/{id}` - Get category with subcategories
- POST `/api/categories` - Create category
- PUT `/api/categories/{id}` - Update category
- DELETE `/api/categories/{id}` - Delete category

### Orders
- GET `/api/orders` - Get orders (with filtering)
- GET `/api/orders/{id}` - Get order details
- POST `/api/orders` - Create new order
- PUT `/api/orders/{id}/status` - Update order status
- POST `/api/orders/{id}/payments` - Add payment to order
- POST `/api/orders/{id}/void` - Void order

### Customers
- GET `/api/customers` - Get all customers
- GET `/api/customers/{id}` - Get customer details
- POST `/api/customers` - Create customer
- PUT `/api/customers/{id}` - Update customer
- GET `/api/customers/{id}/orders` - Get customer order history

## Default Users

After seeding the database, the following users are available:

| Username | Password | Role | PIN |
|----------|----------|------|-----|
| admin | Admin123! | Admin | 9999 |
| manager | Manager123! | Manager | 1234 |
| cashier1 | Cashier123! | Cashier | 1111 |

## Development

### Adding a New Migration
```bash
cd src\POS.WebAPI
dotnet ef migrations add MigrationName -p ../POS.Infrastructure -s .
```

### Updating Database
```bash
dotnet ef database update
```

### Removing Last Migration
```bash
dotnet ef migrations remove -p ../POS.Infrastructure -s .
```

## Testing
Run tests from the solution root:
```bash
dotnet test
```

## Configuration

### JWT Settings
Configure JWT in `appsettings.json`:
```json
"JwtSettings": {
    "Secret": "your-256-bit-secret-key",
    "Issuer": "POS.API",
    "Audience": "POS.Client",
    "ExpirationInMinutes": 60
}
```

### Logging
Serilog configuration in `appsettings.json`:
```json
"Serilog": {
    "MinimumLevel": {
        "Default": "Information",
        "Override": {
            "Microsoft": "Warning"
        }
    }
}
```

## License
Proprietary - Cookie Barrel Pty Ltd

## Contact
For support, contact: support@cookiebarrel.com.au
