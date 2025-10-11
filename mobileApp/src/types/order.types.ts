import { Product } from './product.types';

// Order Types
export type OrderStatus = 'pending' | 'preparing' | 'ready' | 'completed' | 'cancelled';
export type OrderSource = 'mobile' | 'pos';

export interface OrderItem {
  id?: number;
  productId: number;
  product?: Product;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface Order {
  id: number;
  orderNumber: string;
  customerId?: number;
  customerName: string;
  customerPhone: string;
  storeId: number;
  storeName?: string;
  orderSource: OrderSource;
  status: OrderStatus;
  specialInstructions?: string;
  items: OrderItem[];
  subtotal: number;
  gstAmount: number;
  totalAmount: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrderRequest {
  customerName: string;
  customerPhone: string;
  storeId: number;
  orderSource: OrderSource;
  specialInstructions?: string;
  items: {
    productId: number;
    quantity: number;
    unitPrice: number;
  }[];
  subtotal: number;
  gstAmount: number;
  totalAmount: number;
}

export interface CreateOrderResponse {
  success: boolean;
  data: {
    orderId: number;
    orderNumber: string;
    status: OrderStatus;
    estimatedTime?: string;
  };
  message?: string;
}
