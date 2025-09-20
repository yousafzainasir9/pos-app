import apiService from './api.service';
import { 
  Product, 
  ProductListItem,
  Category,
  Subcategory 
} from '@/types';

class ProductService {
  async getProducts(params?: {
    search?: string;
    categoryId?: number;
    subcategoryId?: number;
    isActive?: boolean;
  }): Promise<ProductListItem[]> {
    const response = await apiService.get<ProductListItem[]>('/products', { params });
    return response.data;
  }

  async getProduct(id: number): Promise<Product> {
    const response = await apiService.get<Product>(`/products/${id}`);
    return response.data;
  }

  async getProductByBarcode(barcode: string): Promise<Product> {
    const response = await apiService.get<Product>(`/products/by-barcode/${barcode}`);
    return response.data;
  }

  async createProduct(product: Omit<Product, 'id'>): Promise<Product> {
    const response = await apiService.post<Product>('/products', product);
    return response.data;
  }

  async updateProduct(id: number, product: Product): Promise<void> {
    await apiService.put(`/products/${id}`, product);
  }

  async deleteProduct(id: number): Promise<void> {
    await apiService.delete(`/products/${id}`);
  }

  async getCategories(includeSubcategories = false): Promise<Category[]> {
    const response = await apiService.get<Category[]>('/categories', {
      params: { includeSubcategories }
    });
    return response.data;
  }

  async getCategory(id: number): Promise<Category> {
    const response = await apiService.get<Category>(`/categories/${id}`);
    return response.data;
  }

  async getSubcategories(categoryId: number): Promise<Subcategory[]> {
    const response = await apiService.get<Subcategory[]>(`/categories/${categoryId}/subcategories`);
    return response.data;
  }

  async getSubcategoryProducts(subcategoryId: number): Promise<ProductListItem[]> {
    const response = await apiService.get<ProductListItem[]>(`/categories/subcategories/${subcategoryId}/products`);
    return response.data;
  }
}

export default new ProductService();
