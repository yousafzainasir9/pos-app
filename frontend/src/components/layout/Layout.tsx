import React, { ReactNode } from 'react';
import { Container } from 'react-bootstrap';
import Header from './Header';
import { useAuth } from '@/contexts/AuthContext';
import { Navigate } from 'react-router-dom';

interface LayoutProps {
  children: ReactNode;
  requireAuth?: boolean;
  requireShift?: boolean;
}

const Layout: React.FC<LayoutProps> = ({ 
  children, 
  requireAuth = true,
  requireShift = false 
}) => {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center vh-100">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  if (requireAuth && !isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return (
    <div className="min-vh-100 d-flex flex-column">
      {isAuthenticated && <Header />}
      <main className="flex-grow-1">
        <Container fluid className="py-3">
          {children}
        </Container>
      </main>
    </div>
  );
};

export default Layout;
