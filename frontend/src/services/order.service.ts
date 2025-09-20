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
    const response = await apiService.get('/orders', { params });
    return response.data.data || [];
  }

  async getOrder(id: number): Promise<Order> {
    const response = await apiService.get(`/orders/${id}`);
    return response.data.data;
  }

  async createOrder(order: CreateOrderRequest): Promise<{ orderId: number; orderNumber: string }> {
    const response = await apiService.post('/orders', order);
    return response.data.data;
  }

  async processPayment(payment: ProcessPaymentRequest): Promise<{
    message: string;
    orderStatus: string;
    paidAmount: number;
    remainingAmount: number;
    changeAmount: number;
  }> {
    const response = await apiService.post(`/orders/${payment.orderId}/payments`, payment);
    return response.data.data;
  }

  async voidOrder(id: number, reason: string): Promise<{ message: string }> {
    const response = await apiService.post(`/orders/${id}/void`, { reason });
    return response.data.data;
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
    const response = await apiService.get('/orders/current-shift');
    return response.data.data;
  }
}

export default new OrderService();
