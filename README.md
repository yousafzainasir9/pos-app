# Cookie Barrel Point of Sale System

A comprehensive Point of Sale (POS) system designed for Cookie Barrel bakery chain, built with modern web technologies.

## 🚀 Features

- **Product Management**: Category-based product organization with barcode support
- **Order Processing**: Complete order workflow with multiple payment methods
- **Inventory Tracking**: Real-time stock management with low stock alerts
- **Shift Management**: Cash register shifts with reconciliation
- **User Management**: Role-based access control (Admin, Manager, Cashier)
- **GST Calculation**: Automatic 10% Australian GST handling
- **Reporting**: Sales reports and analytics
- **PIN Login**: Quick access for cashiers using 4-digit PIN

## 🏗️ Architecture

### Backend (.NET 9)
- **Clean Architecture** pattern
- **Entity Framework Core** with Code First approach
- **Repository & Unit of Work** patterns
- **JWT Authentication**
- **SQL Server** database
- **RESTful API** design

### Frontend (React 18)
- **TypeScript** for type safety
- **Context API** for state management
- **React Hook Form** with **Zod** validation
- **Bootstrap 5** for UI
- **Axios** for API communication
- **Vite** for fast development

## 📁 Project Structure

```
pos-app/
├── backend/           # .NET Web API
│   ├── src/
│   │   ├── POS.Domain/        # Core entities
│   │   ├── POS.Application/   # Business logic
│   │   ├── POS.Infrastructure/# Data access
│   │   └── POS.WebAPI/        # API endpoints
│   └── README.md
├── frontend/          # React application
│   ├── src/
│   │   ├── components/   # UI components
│   │   ├── contexts/     # State management
│   │   ├── pages/        # Page components
│   │   ├── services/     # API services
│   │   └── schemas/      # Validation schemas
│   └── README.md
└── documentation/     # System documentation
```

## 🚦 Quick Start

### Prerequisites
- .NET 9.0 SDK
- Node.js 18+ and npm
- SQL Server (LocalDB or full instance)
- Git

### Backend Setup

1. Navigate to backend directory:
```bash
cd backend
```

2. Run the setup script (Windows):
```bash
run.bat
```

This will:
- Restore NuGet packages
- Create database migrations
- Apply migrations to database
- Seed initial data (categories, products, users)
- Start API server on http://localhost:5001

### Frontend Setup

1. Navigate to frontend directory:
```bash
cd frontend
```

2. Run the setup script (Windows):
```bash
run.bat
```

This will:
- Install npm dependencies
- Start development server on http://localhost:3000

## 👤 Default Users

| Username | Password | PIN | Role |
|----------|----------|-----|------|
| admin | Admin123! | 9999 | Admin |
| manager | Manager123! | 1234 | Manager |
| cashier1 | Cashier123! | 1111 | Cashier |

## 🔑 Key Features

### Point of Sale
- Browse products by category/subcategory
- Search products by name or barcode
- Add to cart with quantity management
- Apply discounts
- Process multiple payment types

### Shift Management
- Open shift with starting cash count
- Track sales throughout the shift
- Close shift with cash reconciliation
- Generate shift reports

### Inventory Management
- Track stock levels in real-time
- Low stock alerts
- Stock adjustment capabilities
- Purchase order management (coming soon)

### Reporting
- Daily sales reports
- Product performance analytics
- Shift summaries
- Customer purchase history

## 🛠️ Technology Stack

### Backend
- .NET 9.0
- Entity Framework Core 9.0
- SQL Server
- JWT Authentication
- Serilog for logging
- Swagger/OpenAPI

### Frontend
- React 18
- TypeScript
- Bootstrap 5
- React Router v6
- React Hook Form
- Zod validation
- Axios
- Vite

## 📊 Database Schema

Key entities:
- **Products**: Items for sale with pricing and inventory
- **Categories/Subcategories**: Product organization
- **Orders**: Sales transactions
- **OrderItems**: Line items in orders
- **Payments**: Payment records
- **Users**: System users and authentication
- **Shifts**: Cash register shifts
- **Customers**: Customer information and loyalty

## 🔒 Security Features

- JWT token-based authentication
- Role-based access control (RBAC)
- Password hashing with BCrypt
- Automatic token refresh
- Audit trails for all transactions
- Soft delete for data integrity

## 📈 Performance

- Optimized database queries with EF Core
- Client-side caching for frequently accessed data
- Lazy loading for improved initial load times
- Pagination for large datasets
- Real-time inventory updates

## 🧪 Testing

### Run Backend Tests
```bash
cd backend
dotnet test
```

### Run Frontend Tests
```bash
cd frontend
npm test
```

## 📦 Deployment

### Backend Deployment
1. Build for production:
```bash
dotnet publish -c Release
```

2. Deploy to IIS or Azure App Service
3. Update connection strings for production database

### Frontend Deployment
1. Build for production:
```bash
npm run build
```

2. Deploy `dist` folder to web server or CDN
3. Configure API endpoint for production

## 🐛 Troubleshooting

### Common Issues

1. **Database Connection Failed**
   - Check SQL Server is running
   - Verify connection string in appsettings.json
   - Ensure database permissions are correct

2. **Port Already in Use**
   - Backend: Change port in launchSettings.json
   - Frontend: Change port in vite.config.ts

3. **CORS Issues**
   - Verify CORS configuration in Program.cs
   - Check API URL in frontend configuration

## 📝 License

Proprietary - Cookie Barrel Pty Ltd. All rights reserved.

## 👥 Support

For technical support or questions:
- Email: support@cookiebarrel.com.au
- Phone: +61 2 XXXX XXXX
- Documentation: [Internal Wiki]

## 🔄 Updates

Check for updates regularly:
```bash
git pull origin main
```

## 🏆 Credits

Developed by Cookie Barrel IT Team

---

© 2025 Cookie Barrel Pty Ltd. All rights reserved.
