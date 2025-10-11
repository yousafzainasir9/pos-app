// Product Types
export interface Product {
  id: number;
  name: string;
  slug?: string;
  sku?: string;
  barcode?: string;
  description?: string;
  priceExGst: number;
  gstAmount: number;
  priceIncGst: number;
  cost?: number;
  packSize?: number;
  imageUrl?: string;
  isActive: boolean;
  trackInventory: boolean;
  stockQuantity: number;
  lowStockThreshold: number;
  displayOrder?: number;
  subcategoryId: number;
  subcategory?: Subcategory;
  category?: Category;
  supplierId?: number;
  supplier?: Supplier;
  createdAt?: string;
  updatedAt?: string;
}

export interface Category {
  id: number;
  name: string;
  slug?: string;
  description?: string;
  imageUrl?: string;
  displayOrder: number;
  isActive: boolean;
  subcategoryCount?: number;
  productCount?: number;
}

export interface Subcategory {
  id: number;
  name: string;
  slug?: string;
  description?: string;
  categoryId: number;
  displayOrder: number;
  isActive: boolean;
  productCount?: number;
}

export interface Supplier {
  id: number;
  name: string;
  isActive: boolean;
}

// Cart Types
export interface CartItem {
  product: Product;
  quantity: number;
}
