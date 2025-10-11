import apiClient from './client';
import {
  Order,
  CreateOrderRequest,
  CreateOrderResponse,
  OrderStatus,
} from '../types/order.types';

export const ordersApi = {
  // Create new order
  create: async (orderData: CreateOrderRequest): Promise<CreateOrderResponse> => {
    const response = await apiClient.post<CreateOrderResponse>(
      '/orders',
      orderData
    );
    return response.data;
  },

  // Get customer orders by phone
  getByCustomerPhone: async (phone: string): Promise<Order[]> => {
    const response = await apiClient.get<{ success: boolean; data: Order[] }>(
      `/orders/customer/${phone}`
    );
    return response.data.data;
  },

  // Get order by ID
  getById: async (id: number): Promise<Order> => {
    const response = await apiClient.get<{ success: boolean; data: Order }>(
      `/orders/${id}`
    );
    return response.data.data;
  },

  // Update order status
  updateStatus: async (id: number, status: OrderStatus): Promise<void> => {
    await apiClient.patch(`/orders/${id}/status`, { status });
  },

  // Cancel order
  cancel: async (id: number): Promise<void> => {
    await apiClient.patch(`/orders/${id}/cancel`);
  },
};
