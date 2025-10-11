// Auth Types
export interface Customer {
  id?: number;
  name: string;
  phone: string;
}

export interface User {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  customerId?: number;
  phone?: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthState {
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isGuest: boolean;
  // Legacy support
  customer: Customer | null;
}
