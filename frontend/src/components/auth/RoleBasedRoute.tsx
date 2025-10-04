import React, { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { Alert, Container } from 'react-bootstrap';

interface RoleBasedRouteProps {
  children: ReactNode;
  allowedRoles: string[];
  redirectTo?: string;
}

const RoleBasedRoute: React.FC<RoleBasedRouteProps> = ({ 
  children, 
  allowedRoles,
  redirectTo = '/admin'
}) => {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (!user || !allowedRoles.includes(user.role)) {
    return (
      <Container className="mt-5">
        <Alert variant="danger">
          <Alert.Heading>Access Denied</Alert.Heading>
          <p>
            You do not have permission to access this page. This page is only available to {allowedRoles.join(', ')} users.
          </p>
          <hr />
          <p className="mb-0">
            Your current role: <strong>{user?.role}</strong>
          </p>
        </Alert>
      </Container>
    );
  }

  return <>{children}</>;
};

export default RoleBasedRoute;
