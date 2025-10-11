// Auth Types
export interface Customer {
  id?: number;
  name: string;
  phone: string;
}

export interface LoginRequest {
  phone: string;
  name: string;
}

export interface AuthState {
  customer: Customer | null;
  isAuthenticated: boolean;
  isGuest: boolean;
}
