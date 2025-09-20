import apiService from './api.service';
import { 
  LoginRequest, 
  LoginResponse, 
  PinLoginRequest,
  User 
} from '@/types';

class AuthService {
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await apiService.post<LoginResponse>('/auth/login', credentials);
    const data = response.data;
    
    // Store auth data
    localStorage.setItem('token', data.token);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('user', JSON.stringify(data.user));
    
    return data;
  }

  async pinLogin(credentials: PinLoginRequest): Promise<LoginResponse> {
    const response = await apiService.post<LoginResponse>('/auth/pin-login', credentials);
    const data = response.data;
    
    // Store auth data
    localStorage.setItem('token', data.token);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('user', JSON.stringify(data.user));
    
    return data;
  }

  async logout(): Promise<void> {
    try {
      await apiService.post('/auth/logout');
    } finally {
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
