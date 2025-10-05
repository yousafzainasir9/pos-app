import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import './App.css';
import './theme/theme.css';

// Contexts
import { AuthProvider } from './contexts/AuthContext';
import { CartProvider } from './contexts/CartContext';
import { ShiftProvider } from './contexts/ShiftContext';
import { ThemeProvider } from './theme/ThemeContext';

// Layout
import Layout from './components/layout/Layout';
import RoleBasedRoute from './components/auth/RoleBasedRoute';
import HomeRedirect from './components/auth/HomeRedirect';

// Pages
import LoginPage from './pages/LoginPage';
import POSPage from './pages/POSPage';
import CartPage from './pages/CartPage';
import OrdersPage from './pages/OrdersPage';
import ProductsPage from './pages/ProductsPage';
import ShiftPage from './pages/ShiftPage';
import ReportsPage from './pages/ReportsPage';
import AdminPage from './pages/AdminPage';
import ThemeSettingsPage from './pages/ThemeSettingsPage';
import UserManagementPage from './pages/UserManagementPage';
import StoreSettingsPage from './pages/StoreSettingsPage';
import SecurityAuditPage from './pages/SecurityAuditPage';

function App() {
  return (
    <Router>
      <ThemeProvider>
        <AuthProvider>
          <CartProvider>
            <ShiftProvider>
            <Routes>
              {/* Public Routes */}
              <Route path="/login" element={<LoginPage />} />
              
              {/* Protected Routes */}
              <Route path="/" element={<Layout><HomeRedirect /></Layout>} />
              <Route path="/pos" element={
                <Layout>
                  <RoleBasedRoute allowedRoles={['Manager', 'Cashier', 'Staff']}>
                    <POSPage />
                  </RoleBasedRoute>
                </Layout>
              } />
              <Route path="/cart" element={
                <Layout>
                  <RoleBasedRoute allowedRoles={['Manager', 'Cashier', 'Staff']}>
                    <CartPage />
                  </RoleBasedRoute>
                </Layout>
              } />
              <Route path="/orders" element={<Layout><OrdersPage /></Layout>} />
              <Route path="/products" element={<Layout><ProductsPage /></Layout>} />
              <Route path="/shift" element={<Layout><ShiftPage /></Layout>} />
              <Route path="/reports" element={<Layout><ReportsPage /></Layout>} />
              
              {/* Admin Routes */}
              <Route path="/admin" element={
                <Layout>
                  <RoleBasedRoute allowedRoles={['Admin', 'Manager']}>
                    <AdminPage />
                  </RoleBasedRoute>
                </Layout>
              } />
              <Route path="/admin/users" element={
                <Layout>
                  <RoleBasedRoute allowedRoles={['Admin', 'Manager']}>
                    <UserManagementPage />
                  </RoleBasedRoute>
                </Layout>
              } />
              <Route path="/admin/store-settings" element={
                <Layout>
                  <RoleBasedRoute allowedRoles={['Admin', 'Manager']}>
                    <StoreSettingsPage />
                  </RoleBasedRoute>
                </Layout>
              } />
              <Route path="/admin/security" element={
                <Layout>
                  <RoleBasedRoute allowedRoles={['Admin']}>
                    <SecurityAuditPage />
                  </RoleBasedRoute>
                </Layout>
              } />
              
              <Route path="/theme-settings" element={<Layout><ThemeSettingsPage /></Layout>} />
              
              {/* Catch all */}
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
            
            <ToastContainer
              position="top-right"
              autoClose={3000}
              hideProgressBar={false}
              newestOnTop
              closeOnClick
              rtl={false}
              pauseOnFocusLoss
              draggable
              pauseOnHover
              theme="light"
            />
          </ShiftProvider>
        </CartProvider>
      </AuthProvider>
    </ThemeProvider>
    </Router>
  );
}

export default App;
