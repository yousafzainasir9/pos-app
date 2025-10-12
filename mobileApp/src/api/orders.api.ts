import apiClient from './client';
import {
  Order,
  CreateOrderDto,
  CreateOrderResponse,
  OrderStatus,
  ProcessPaymentDto,
} from '../types/order.types';

export const ordersApi = {
  // Create new order
  create: async (orderData: CreateOrderDto): Promise<CreateOrderResponse> => {
    const response = await apiClient.post<CreateOrderResponse>(
      '/orders',
      orderData
    );
    return response.data;
  },

  // Get orders with pagination
  getOrders: async (params?: {
    fromDate?: string;
    toDate?: string;
    status?: OrderStatus;
    customerId?: number;
    page?: number;
    pageSize?: number;
  }): Promise<{ data: Order[]; pagination: any }> => {
    const response = await apiClient.get('/orders', { params });
    return response.data.data;
  },

  // Get order by ID
  getById: async (id: number): Promise<Order> => {
    const response = await apiClient.get<Order>(`/orders/${id}`);
    return response.data;
  },

  // Process payment for order
  processPayment: async (orderId: number, paymentData: ProcessPaymentDto): Promise<any> => {
    const response = await apiClient.post(
      `/orders/${orderId}/payments`,
      paymentData
    );
    return response.data;
  },

  // Void/cancel order
  voidOrder: async (orderId: number, reason: string): Promise<void> => {
    await apiClient.post(`/orders/${orderId}/void`, { reason });
  },

  // Get current shift orders
  getCurrentShiftOrders: async (): Promise<any> => {
    const response = await apiClient.get('/orders/current-shift');
    return response.data;
  },
};
