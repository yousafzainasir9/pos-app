import axios from 'axios';
import { 
  LoginRequest, 
  LoginResponse, 
  PinLoginRequest,
  User 
} from '@/types';

const API_BASE_URL = 'https://localhost:7021/api';

class AuthService {
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    try {
      // Use axios directly for login to avoid token interceptor issues
      const response = await axios.post<LoginResponse>(`${API_BASE_URL}/auth/login`, credentials, {
        headers: {
          'Content-Type': 'application/json'
        }
      });
      
      const data = response.data;
      
      // Store auth data
      if (data.token) {
        localStorage.setItem('token', data.token);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('user', JSON.stringify(data.user));
      }
      
      return data;
    } catch (error: any) {
      console.error('Login error:', error);
      throw error;
    }
  }

  async pinLogin(credentials: PinLoginRequest): Promise<LoginResponse> {
    try {
      // Use axios directly for PIN login to avoid token interceptor issues
      const response = await axios.post<LoginResponse>(`${API_BASE_URL}/auth/pin-login`, credentials, {
        headers: {
          'Content-Type': 'application/json'
        }
      });
      
      const data = response.data;
      
      // Store auth data
      if (data.token) {
        localStorage.setItem('token', data.token);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('user', JSON.stringify(data.user));
      }
      
      return data;
    } catch (error: any) {
      console.error('PIN login error:', error);
      throw error;
    }
  }

  async logout(): Promise<void> {
    try {
      // Try to call logout endpoint but don't fail if it errors
      const token = localStorage.getItem('token');
      if (token) {
        await axios.post(`${API_BASE_URL}/auth/logout`, {}, {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });
      }
    } catch (error) {
      console.error('Logout API error:', error);
    } finally {
      // Always clear local storage
      this.clearAuth();
    }
  }

  clearAuth(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getUser(): User | null {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      try {
        return JSON.parse(userStr);
      } catch {
        return null;
      }
    }
    return null;
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  hasRole(role: string): boolean {
    const user = this.getUser();
    return user?.role === role;
  }

  isManager(): boolean {
    const user = this.getUser();
    return user?.role === 'Admin' || user?.role === 'Manager';
  }

  isCashier(): boolean {
    const user = this.getUser();
    return user?.role === 'Cashier' || this.isManager();
  }
}

export default new AuthService();
