import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { User, LoginRequest, PinLoginRequest } from '@/types';
import authService from '@/services/auth.service';
import { toast } from 'react-toastify';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginRequest) => Promise<User>;
  pinLogin: (credentials: PinLoginRequest) => Promise<User>;
  logout: () => Promise<void>;
  checkAuth: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    checkAuth();
  }, []);

  const checkAuth = () => {
    const storedUser = authService.getUser();
    const token = authService.getToken();
    
    if (storedUser && token) {
      setUser(storedUser);
    }
    setIsLoading(false);
  };

  const login = async (credentials: LoginRequest): Promise<User> => {
    try {
      const response = await authService.login(credentials);
      setUser(response.user);
      toast.success('Login successful!');
      return response.user;
    } catch (error: any) {
      // Handle new error format
      const errorMessage = error.message || 
                          error.response?.data?.error?.message || 
                          'Login failed';
      
      // Show validation errors if present
      if (error.errors) {
        Object.entries(error.errors).forEach(([field, messages]: [string, any]) => {
          if (Array.isArray(messages)) {
            messages.forEach(msg => toast.error(`${field}: ${msg}`));
          }
        });
      } else {
        toast.error(errorMessage);
      }
      
      throw error;
    }
  };

  const pinLogin = async (credentials: PinLoginRequest): Promise<User> => {
    try {
      const response = await authService.pinLogin(credentials);
      setUser(response.user);
      toast.success('Login successful!');
      return response.user;
    } catch (error: any) {
      // Handle new error format
      const errorMessage = error.message || 
                          error.response?.data?.error?.message || 
                          'PIN login failed';
      
      // Show validation errors if present
      if (error.errors) {
        Object.entries(error.errors).forEach(([field, messages]: [string, any]) => {
          if (Array.isArray(messages)) {
            messages.forEach(msg => toast.error(`${field}: ${msg}`));
          }
        });
      } else {
        toast.error(errorMessage);
      }
      
      throw error;
    }
  };

  const logout = async () => {
    try {
      await authService.logout();
      setUser(null);
      toast.info('Logged out successfully');
    } catch (error) {
      console.error('Logout error:', error);
    }
  };

  const value: AuthContextType = {
    user,
    isAuthenticated: !!user,
    isLoading,
    login,
    pinLogin,
    logout,
    checkAuth
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
