import apiService from './api.service';

class ProductService {
  async getProductByBarcode(barcode: string) {
    const response = await apiService.get(`/products/by-barcode/${barcode}`);
    return response.data.data;
  }

  async getProducts(params?: any) {
    const response = await apiService.get('/products', { params });
    return response.data.data || [];
  }

  async getProduct(id: number) {
    const response = await apiService.get(`/products/${id}`);
    return response.data.data;
  }

  async createProduct(product: any) {
    const response = await apiService.post('/products', product);
    return response.data.data;
  }

  async updateProduct(id: number, product: any) {
    const response = await apiService.put(`/products/${id}`, product);
    return response.data.data;
  }

  async deleteProduct(id: number) {
    const response = await apiService.delete(`/products/${id}`);
    return response.data.data;
  }

  async getCategories(includeSubcategories: boolean = false) {
    const response = await apiService.get('/categories');
    return response.data.data || [];
  }

  async getSubcategoriesByCategory(categoryId: number) {
    const response = await apiService.get(`/subcategories/category/${categoryId}`);
    return response.data.data || [];
  }

  async getSuppliers() {
    const response = await apiService.get('/suppliers');
    return response.data.data || [];
  }
}

export default new ProductService();
