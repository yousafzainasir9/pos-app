import axios, { AxiosInstance, AxiosError, AxiosRequestConfig } from 'axios';
import { toast } from 'react-toastify';
import { ApiResponse } from '@/types';

const API_BASE_URL = 'https://localhost:7021/api';

class ApiService {
  private axiosInstance: AxiosInstance;
  private isRefreshing = false;
  private failedQueue: any[] = [];

  constructor() {
    this.axiosInstance = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.setupInterceptors();
  }

  private processQueue(error: any, token: string | null = null) {
    this.failedQueue.forEach(prom => {
      if (error) {
        prom.reject(error);
      } else {
        prom.resolve(token);
      }
    });
    
    this.failedQueue = [];
  }

  private setupInterceptors() {
    // Request interceptor
    this.axiosInstance.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('token');
        if (token && config.headers) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    // Response interceptor
    this.axiosInstance.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };

        // Don't retry on login or refresh endpoints
        if (originalRequest?.url?.includes('/auth/login') || 
            originalRequest?.url?.includes('/auth/refresh') ||
            originalRequest?.url?.includes('/auth/pin-login')) {
          return Promise.reject(error);
        }

        if (error.response?.status === 401 && !originalRequest._retry) {
          if (this.isRefreshing) {
            // If already refreshing, queue this request
            return new Promise((resolve, reject) => {
              this.failedQueue.push({ resolve, reject });
            }).then(token => {
              if (originalRequest.headers) {
                originalRequest.headers.Authorization = `Bearer ${token}`;
              }
              return this.axiosInstance(originalRequest);
            }).catch(err => {
              return Promise.reject(err);
            });
          }

          originalRequest._retry = true;
          this.isRefreshing = true;
          
          const refreshToken = localStorage.getItem('refreshToken');
          
          if (!refreshToken) {
            this.isRefreshing = false;
            this.clearAuth();
            window.location.href = '/login';
            return Promise.reject(error);
          }

          try {
            const response = await axios.post<ApiResponse<any>>(
              `${API_BASE_URL}/auth/refresh`, 
              { refreshToken }
            );
            
            // Handle new API response format
            if (!response.data.success || !response.data.data) {
              throw new Error('Token refresh failed');
            }

            const { token, refreshToken: newRefreshToken } = response.data.data;
            
            localStorage.setItem('token', token);
            localStorage.setItem('refreshToken', newRefreshToken);
            
            this.isRefreshing = false;
            this.processQueue(null, token);
            
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${token}`;
            }
            
            return this.axiosInstance(originalRequest);
          } catch (refreshError) {
            this.processQueue(refreshError, null);
            this.isRefreshing = false;
            this.clearAuth();
            window.location.href = '/login';
            return Promise.reject(refreshError);
          }
        }

        // Handle new error response format
        if (error.response?.data) {
          const apiError = error.response.data as ApiResponse<any>;
          
          if (apiError.error) {
            // Extract error message and details
            const errorMessage = apiError.error.message;
            const errorCode = apiError.error.errorCode;
            const fieldErrors = apiError.error.errors;

            // Handle specific error codes
            if (errorCode === 'AUTH_001' || errorCode === 'AUTH_002') {
              toast.error(errorMessage);
            } else if (error.response.status === 429) {
              toast.error('Too many requests. Please try again later.');
            } else if (error.response.status === 500) {
              toast.error('Server error occurred');
            } else if (error.response.status === 400) {
              // Show validation errors if present
              if (fieldErrors && Object.keys(fieldErrors).length > 0) {
                Object.entries(fieldErrors).forEach(([field, messages]) => {
                  messages.forEach(msg => toast.error(`${field}: ${msg}`));
                });
              } else {
                toast.error(errorMessage || 'Bad request');
              }
            } else if (error.response.status !== 404) {
              // Don't show 404 errors, but show others
              toast.error(errorMessage || 'An error occurred');
            }
          }
        }

        return Promise.reject(error);
      }
    );
  }

  private clearAuth(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  }

  public async get<T>(url: string, config?: AxiosRequestConfig): Promise<{ data: T }> {
    return this.axiosInstance.get<T>(url, config);
  }

  public async post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<{ data: T }> {
    return this.axiosInstance.post<T>(url, data, config);
  }

  public async put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<{ data: T }> {
    return this.axiosInstance.put<T>(url, data, config);
  }

  public async delete<T>(url: string, config?: AxiosRequestConfig): Promise<{ data: T }> {
    return this.axiosInstance.delete<T>(url, config);
  }

  public async patch<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<{ data: T }> {
    return this.axiosInstance.patch<T>(url, data, config);
  }
}

export default new ApiService();
