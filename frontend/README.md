# Cookie Barrel POS Frontend

## Overview
React-based frontend for the Cookie Barrel Point of Sale system, built with TypeScript, React Hook Form, Zod validation, and Bootstrap.

## Tech Stack
- **React 18** with TypeScript
- **Vite** for fast development and building
- **React Router** for navigation
- **Context API** for state management
- **React Hook Form** with Zod for form handling and validation
- **Bootstrap 5** for UI components
- **Axios** for API communication
- **React Toastify** for notifications

## Architecture

### Folder Structure
```
src/
├── components/       # Reusable components
│   └── layout/      # Layout components (Header, Layout)
├── contexts/        # React Context providers
│   ├── AuthContext.tsx
│   ├── CartContext.tsx
│   └── ShiftContext.tsx
├── pages/           # Page components
├── schemas/         # Zod validation schemas
├── services/        # API service layer
├── types/          # TypeScript type definitions
└── App.tsx         # Main application component
```

### Key Features
- **Authentication**: JWT-based auth with PIN login support
- **Shopping Cart**: Context-based cart management with localStorage persistence
- **Shift Management**: Open/close shifts with cash reconciliation
- **POS Interface**: Category-based product browsing with barcode support
- **Order Processing**: Complete order workflow with payment processing
- **Role-based Access**: Admin, Manager, Cashier roles with appropriate permissions

## Installation

1. Navigate to frontend directory:
```bash
cd D:\pos-app\frontend
```

2. Install dependencies:
```bash
npm install
```

3. Start development server:
```bash
npm run dev
```

The application will be available at http://localhost:3000

## Configuration

### API Configuration
The frontend is configured to proxy API requests to the backend running on port 5001. Update `vite.config.ts` if your backend runs on a different port.

### Environment Variables
Create a `.env` file for environment-specific configuration:
```env
VITE_API_URL=http://localhost:5001/api
```

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## Pages

### Public Pages
- **/login** - User authentication (username/password or PIN)

### Protected Pages
- **/pos** - Main POS interface for product selection
- **/cart** - Shopping cart and checkout
- **/orders** - Order management and history
- **/shift** - Shift management (open/close)
- **/products** - Product catalog management
- **/reports** - Sales reports and analytics
- **/admin** - System administration (Admin/Manager only)

## State Management

### AuthContext
Manages user authentication state and provides:
- `login()` - Username/password login
- `pinLogin()` - PIN-based quick login
- `logout()` - User logout
- `user` - Current user information
- `isAuthenticated` - Authentication status

### CartContext
Manages shopping cart state with:
- `items` - Cart items array
- `addItem()` - Add product to cart
- `removeItem()` - Remove from cart
- `updateQuantity()` - Update item quantity
- `clearCart()` - Clear all items
- `totalAmount` - Cart total with GST

### ShiftContext
Manages cash register shift with:
- `currentShift` - Active shift information
- `openShift()` - Start new shift
- `closeShift()` - End shift with reconciliation
- `isShiftOpen` - Shift status

## Form Validation

Forms use React Hook Form with Zod schemas for validation:

```typescript
const schema = z.object({
  username: z.string().min(1, 'Required'),
  password: z.string().min(1, 'Required')
});

const { register, handleSubmit, formState: { errors } } = useForm({
  resolver: zodResolver(schema)
});
```

## API Integration

Services layer handles all API communication:

```typescript
// Example: Product Service
productService.getProducts(params)
productService.getProductByBarcode(barcode)
productService.getCategories()
```

## Authentication Flow

1. User logs in via username/password or PIN
2. JWT token stored in localStorage
3. Token automatically included in API requests
4. Token refresh handled automatically
5. Logout clears token and redirects to login

## Development Guidelines

### Component Structure
```typescript
import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

const Component: React.FC = () => {
  // Hooks
  const { register, handleSubmit } = useForm({
    resolver: zodResolver(schema)
  });

  // Event handlers
  const onSubmit = (data) => {
    // Handle form submission
  };

  return (
    <Form onSubmit={handleSubmit(onSubmit)}>
      {/* Component JSX */}
    </Form>
  );
};
```

### Context Usage
```typescript
import { useAuth } from '@/contexts/AuthContext';

const Component = () => {
  const { user, login } = useAuth();
  // Use context values
};
```

## Testing Accounts

Default accounts for testing:

| Username | Password | PIN | Role |
|----------|----------|-----|------|
| admin | Admin123! | 9999 | Admin |
| manager | Manager123! | 1234 | Manager |
| cashier1 | Cashier123! | 1111 | Cashier |

## Build and Deployment

1. Build for production:
```bash
npm run build
```

2. Preview production build:
```bash
npm run preview
```

3. Deploy the `dist` folder to your hosting service

## License
Proprietary - Cookie Barrel Pty Ltd

## Support
For support, contact: support@cookiebarrel.com.au
