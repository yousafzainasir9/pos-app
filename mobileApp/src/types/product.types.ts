// Product Types
export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  categoryId: number;
  category?: Category;
  imageUrl?: string;
  isActive: boolean;
  stockQuantity: number;
  createdAt: string;
  updatedAt: string;
}

export interface Category {
  id: number;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
}

// Cart Types
export interface CartItem {
  product: Product;
  quantity: number;
}
