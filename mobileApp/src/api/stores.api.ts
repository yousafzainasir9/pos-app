import apiClient from './client';

export interface Store {
  id: number;
  name: string;
  address: string;
  phone: string;
  isActive: boolean;
}

export const storesApi = {
  // Get all active stores
  getAll: async (): Promise<Store[]> => {
    const response = await apiClient.get<{ success: boolean; data: Store[] }>(
      '/stores'
    );
    return response.data.data;
  },

  // Get store by ID
  getById: async (id: number): Promise<Store> => {
    const response = await apiClient.get<{ success: boolean; data: Store }>(
      `/stores/${id}`
    );
    return response.data.data;
  },
};
