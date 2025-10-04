import api from './api.service';

export interface Store {
  id: number;
  name: string;
  code: string;
  address?: string;
  city?: string;
  phone?: string;
  email?: string;
  taxRate: number;
  currency: string;
  isActive: boolean;
}

export interface StoreDetail extends Store {
  state?: string;
  postalCode?: string;
  country?: string;
  taxNumber?: string;
  openingTime?: string;
  closingTime?: string;
  activeUserCount: number;
}

export interface UpdateStoreDto {
  name?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  phone?: string;
  email?: string;
  taxNumber?: string;
  taxRate?: number;
  currency?: string;
  openingTime?: string;
  closingTime?: string;
}

class StoreService {
  async getStores() {
    const response = await api.get('/stores');
    return response.data;
  }

  async getStore(id: number) {
    const response = await api.get(`/stores/${id}`);
    return response.data;
  }

  async getCurrentStore() {
    const response = await api.get('/stores/current');
    return response.data;
  }

  async updateStore(id: number, data: UpdateStoreDto) {
    const response = await api.put(`/stores/${id}`, data);
    return response.data;
  }
}

export default new StoreService();
