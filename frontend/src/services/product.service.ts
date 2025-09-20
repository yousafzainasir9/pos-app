import apiService from './api.service';

class ProductService {
  async getProducts() {
    const response = await apiService.get('/products');
    return response.data;
  }

  async getProduct(id: number) {
    const response = await apiService.get(`/products/${id}`);
    return response.data;
  }

  async createProduct(product: any) {
    const response = await apiService.post('/products', product);
    return response.data;
  }

  async updateProduct(id: number, product: any) {
    const response = await apiService.put(`/products/${id}`, product);
    return response.data;
  }

  async deleteProduct(id: number) {
    const response = await apiService.delete(`/products/${id}`);
    return response.data;
  }

  async getCategories() {
    const response = await apiService.get('/categories');
    return response.data;
  }

  async getSubcategoriesByCategory(categoryId: number) {
    const response = await apiService.get(`/subcategories/category/${categoryId}`);
    return response.data;
  }

  async getSuppliers() {
    const response = await apiService.get('/suppliers');
    return response.data;
  }
}

export default new ProductService();
