import apiClient from './client';
import { Product, Category } from '../types/product.types';

export const productsApi = {
  // Get all products
  getAll: async (): Promise<Product[]> => {
    const response = await apiClient.get<{ success: boolean; data: Product[] }>(
      '/products'
    );
    return response.data.data;
  },

  // Search products
  search: async (query: string): Promise<Product[]> => {
    const response = await apiClient.get<{ success: boolean; data: Product[] }>(
      `/products?search=${encodeURIComponent(query)}`
    );
    return response.data.data;
  },

  // Get products by category
  getByCategory: async (categoryId: number): Promise<Product[]> => {
    const response = await apiClient.get<{ success: boolean; data: Product[] }>(
      `/products?categoryId=${categoryId}`
    );
    return response.data.data;
  },

  // Get all categories
  getCategories: async (): Promise<Category[]> => {
    const response = await apiClient.get<{ success: boolean; data: Category[] }>(
      '/categories'
    );
    return response.data.data;
  },

  // Get product by ID
  getById: async (id: number): Promise<Product> => {
    const response = await apiClient.get<{ success: boolean; data: Product }>(
      `/products/${id}`
    );
    return response.data.data;
  },
};
