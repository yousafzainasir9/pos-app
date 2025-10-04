import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';

const HomeRedirect: React.FC = () => {
  const { user } = useAuth();

  // Redirect based on user role
  if (user?.role === 'Admin') {
    return <Navigate to="/admin" replace />;
  }

  // Default to POS for other roles
  return <Navigate to="/pos" replace />;
};

export default HomeRedirect;
