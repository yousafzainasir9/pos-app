import axios from 'axios';
import { 
  LoginRequest, 
  LoginResponse, 
  PinLoginRequest,
  User,
  ApiResponse
} from '@/types';

const API_BASE_URL = 'https://localhost:7021/api';

class AuthService {
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    try {
      // Use axios directly for login to avoid token interceptor issues
      const response = await axios.post<ApiResponse<LoginResponse>>(
        `${API_BASE_URL}/auth/login`, 
        credentials, 
        {
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );
      
      // Handle new API response format
      if (!response.data.success || !response.data.data) {
        throw new Error(response.data.error?.message || 'Login failed');
      }

      const loginData = response.data.data;
      
      // Store auth data
      if (loginData.token) {
        localStorage.setItem('token', loginData.token);
        localStorage.setItem('refreshToken', loginData.refreshToken);
        localStorage.setItem('user', JSON.stringify(loginData.user));
      }
      
      return loginData;
    } catch (error: any) {
      console.error('Login error:', error);
      
      // Handle new error format
      if (error.response?.data?.error) {
        const apiError = error.response.data.error;
        throw {
          message: apiError.message,
          errorCode: apiError.errorCode,
          errors: apiError.errors
        };
      }
      
      throw error;
    }
  }

  async pinLogin(credentials: PinLoginRequest): Promise<LoginResponse> {
    try {
      // Use axios directly for PIN login to avoid token interceptor issues
      const response = await axios.post<ApiResponse<LoginResponse>>(
        `${API_BASE_URL}/auth/pin-login`, 
        credentials, 
        {
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );
      
      // Handle new API response format
      if (!response.data.success || !response.data.data) {
        throw new Error(response.data.error?.message || 'PIN login failed');
      }

      const loginData = response.data.data;
      
      // Store auth data
      if (loginData.token) {
        localStorage.setItem('token', loginData.token);
        localStorage.setItem('refreshToken', loginData.refreshToken);
        localStorage.setItem('user', JSON.stringify(loginData.user));
      }
      
      return loginData;
    } catch (error: any) {
      console.error('PIN login error:', error);
      
      // Handle new error format
      if (error.response?.data?.error) {
        const apiError = error.response.data.error;
        throw {
          message: apiError.message,
          errorCode: apiError.errorCode,
          errors: apiError.errors
        };
      }
      
      throw error;
    }
  }

  async logout(): Promise<void> {
    try {
      // Try to call logout endpoint but don't fail if it errors
      const token = localStorage.getItem('token');
      if (token) {
        await axios.post<ApiResponse<void>>(
          `${API_BASE_URL}/auth/logout`, 
          {}, 
          {
            headers: {
              'Authorization': `Bearer ${token}`,
              'Content-Type': 'application/json'
            }
          }
        );
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
