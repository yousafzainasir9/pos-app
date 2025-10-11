import apiClient from './client';
import { Product, Category } from '../types/product.types';

export const productsApi = {
  // Get all products
  getAll: async (): Promise<Product[]> => {
    const response = await apiClient.get('/products');
    return response.data.data; // Backend returns { success, data, message }
  },

  // Search products
  search: async (query: string): Promise<Product[]> => {
    const response = await apiClient.get(
      `/products?search=${encodeURIComponent(query)}`
    );
    return response.data.data;
  },

  // Get products by category
  getByCategory: async (categoryId: number): Promise<Product[]> => {
    const response = await apiClient.get(
      `/products?categoryId=${categoryId}`
    );
    return response.data.data;
  },

  // Get all categories
  getCategories: async (): Promise<Category[]> => {
    const response = await apiClient.get('/categories');
    return response.data.data;
  },

  // Get product by ID
  getById: async (id: number): Promise<Product> => {
    const response = await apiClient.get(`/products/${id}`);
    return response.data.data;
  },
};
