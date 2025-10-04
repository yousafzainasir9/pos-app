import api from './api.service';

export interface User {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phone?: string;
  role: string;
  isActive: boolean;
  lastLoginAt?: string;
  storeId?: number;
  storeName?: string;
}

export interface UserDetail extends User {
  createdAt: string;
  updatedAt?: string;
  hasPin: boolean;
}

export interface CreateUserDto {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone?: string;
  pin?: string;
  role: string;
  storeId?: number;
}

export interface UpdateUserDto {
  email?: string;
  firstName?: string;
  lastName?: string;
  phone?: string;
  role?: string;
  isActive?: boolean;
  storeId?: number;
}

class UserService {
  async getUsers(params: {
    page?: number;
    pageSize?: number;
    search?: string;
    role?: string;
    isActive?: boolean;
  }) {
    const response = await api.get('/users', { params });
    return response.data;
  }

  async getUser(id: number) {
    const response = await api.get(`/users/${id}`);
    return response.data;
  }

  async createUser(data: CreateUserDto) {
    const response = await api.post('/users', data);
    return response.data;
  }

  async updateUser(id: number, data: UpdateUserDto) {
    const response = await api.put(`/users/${id}`, data);
    return response.data;
  }

  async deactivateUser(id: number) {
    const response = await api.delete(`/users/${id}`);
    return response.data;
  }

  async resetPassword(id: number, newPassword: string) {
    const response = await api.post(`/users/${id}/reset-password`, { newPassword });
    return response.data;
  }

  async resetPin(id: number, newPin: string) {
    const response = await api.post(`/users/${id}/reset-pin`, { newPin });
    return response.data;
  }

  async getUserActivity(id: number, from?: string, to?: string) {
    const response = await api.get(`/users/${id}/activity`, {
      params: { from, to }
    });
    return response.data;
  }
}

export default new UserService();
