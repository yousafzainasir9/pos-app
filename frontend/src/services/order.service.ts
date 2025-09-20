import apiService from './api.service';
import { 
  Order,
  CreateOrderRequest,
  ProcessPaymentRequest,
  OrderStatus
} from '@/types';

class OrderService {
  async getOrders(params?: {
    fromDate?: Date;
    toDate?: Date;
    status?: OrderStatus;
    customerId?: number;
  }): Promise<Order[]> {
    const response = await apiService.get<Order[]>('/orders', { params });
    return response.data;
  }

  async getOrder(id: number): Promise<Order> {
    const response = await apiService.get<Order>(`/orders/${id}`);
    return response.data;
  }

  async createOrder(order: CreateOrderRequest): Promise<{ orderId: number; orderNumber: string }> {
    const response = await apiService.post<{ orderId: number; orderNumber: string }>('/orders', order);
    return response.data;
  }

  async processPayment(payment: ProcessPaymentRequest): Promise<{
    message: string;
    orderStatus: string;
    paidAmount: number;
    remainingAmount: number;
    changeAmount: number;
  }> {
    const response = await apiService.post<{
      message: string;
      orderStatus: string;
      paidAmount: number;
      remainingAmount: number;
      changeAmount: number;
    }>(`/orders/${payment.orderId}/payments`, payment);
    return response.data;
  }

  async voidOrder(id: number, reason: string): Promise<{ message: string }> {
    const response = await apiService.post<{ message: string }>(`/orders/${id}/void`, { reason });
    return response.data;
  }

  async getCurrentShiftOrders(): Promise<{
    shiftId?: number;
    shiftNumber?: string;
    startTime?: Date;
    totalOrders: number;
    totalSales: number;
    orders: Array<{
      id: number;
      orderNumber: string;
      orderDate: Date;
      status: OrderStatus;
      totalAmount: number;
      paidAmount: number;
      customerName: string;
    }>;
  }> {
    const response = await apiService.get<any>('/orders/current-shift');
    return response.data;
  }
}

export default new OrderService();
