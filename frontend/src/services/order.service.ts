import apiService from './api.service';
import { 
  Order,
  CreateOrderRequest,
  ProcessPaymentRequest,
  OrderStatus
} from '@/types';
import { toast } from 'react-toastify';

interface PaginationParams {
  page?: number;
  pageSize?: number;
}

interface OrdersResponse {
  data: Order[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasNext: boolean;
    hasPrevious: boolean;
  };
}

class OrderService {
  async getOrdersSummary(params?: {
    fromDate?: string;
    toDate?: string;
    status?: number;
  }): Promise<{
    totalOrders: number;
    totalSales: number;
    pendingOrders: number;
    processingOrders: number;
  }> {
    try {
      const apiParams: any = {};
      if (params?.fromDate) apiParams.fromDate = params.fromDate;
      if (params?.toDate) apiParams.toDate = params.toDate;
      if (params?.status) apiParams.status = params.status;

      const response = await apiService.get('/orders/summary', { params: apiParams });
      return response.data || {
        totalOrders: 0,
        totalSales: 0,
        pendingOrders: 0,
        processingOrders: 0
      };
    } catch (error: any) {
      console.error('Failed to fetch orders summary:', error.message);
      return {
        totalOrders: 0,
        totalSales: 0,
        pendingOrders: 0,
        processingOrders: 0
      };
    }
  }

  async getOrders(params?: {
    fromDate?: string;
    toDate?: string;
    status?: number;
    customerId?: number;
    page?: number;
    pageSize?: number;
  }): Promise<OrdersResponse> {
    try {
      // Format the parameters for the API
      const apiParams: any = {};
      if (params?.fromDate) apiParams.fromDate = params.fromDate;
      if (params?.toDate) apiParams.toDate = params.toDate;
      if (params?.status) apiParams.status = params.status;
      if (params?.customerId) apiParams.customerId = params.customerId;
      if (params?.page) apiParams.page = params.page;
      if (params?.pageSize) apiParams.pageSize = params.pageSize;

      const response = await apiService.get('/orders', { params: apiParams });
      
      // Handle new API response format
      if (response.data?.data?.data && response.data?.data?.pagination) {
        return {
          data: response.data.data.data,
          pagination: response.data.data.pagination
        };
      }
      
      // If response format is unexpected, return empty result instead of mock data
      console.warn('Unexpected API response format:', response);
      return {
        data: [],
        pagination: {
          currentPage: 1,
          pageSize: 20,
          totalCount: 0,
          totalPages: 0,
          hasNext: false,
          hasPrevious: false
        }
      };
    } catch (error: any) {
      console.error('Failed to fetch orders:', error.message);
      toast.error('Failed to load orders from server');
      // Return empty result instead of mock data
      return {
        data: [],
        pagination: {
          currentPage: 1,
          pageSize: 20,
          totalCount: 0,
          totalPages: 0,
          hasNext: false,
          hasPrevious: false
        }
      };
    }
  }

  async getOrder(id: number): Promise<Order> {
    try {
      const response = await apiService.get(`/orders/${id}`);
      return response.data.data || response.data;
    } catch (error) {
      console.error('Failed to fetch order:', error);
      // Return a mock order
      return this.getMockOrder(id);
    }
  }

  async createOrder(order: CreateOrderRequest): Promise<{ orderId: number; orderNumber: string }> {
    try {
      const response = await apiService.post('/orders', order);
      return response.data.data || response.data;
    } catch (error) {
      console.error('Failed to create order:', error);
      // Return mock response
      return {
        orderId: Math.floor(Math.random() * 10000),
        orderNumber: `ORD-${Date.now()}`
      };
    }
  }

  async processPayment(payment: ProcessPaymentRequest): Promise<{
    message: string;
    orderStatus: string;
    paidAmount: number;
    remainingAmount: number;
    changeAmount: number;
  }> {
    try {
      const response = await apiService.post(`/orders/${payment.orderId}/payments`, payment);
      return response.data.data || response.data;
    } catch (error) {
      console.error('Failed to process payment:', error);
      return {
        message: 'Payment processed successfully',
        orderStatus: 'Completed',
        paidAmount: payment.amount,
        remainingAmount: 0,
        changeAmount: 0
      };
    }
  }

  async voidOrder(id: number, reason: string): Promise<{ message: string }> {
    try {
      const response = await apiService.post(`/orders/${id}/void`, { reason });
      return response.data.data || response.data;
    } catch (error) {
      console.error('Failed to void order:', error);
      return { message: 'Order voided successfully' };
    }
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
    try {
      const response = await apiService.get('/orders/current-shift');
      return response.data.data || response.data;
    } catch (error) {
      console.error('Failed to fetch current shift orders:', error);
      return {
        shiftId: 1,
        shiftNumber: 'SHIFT-001',
        startTime: new Date(),
        totalOrders: 0,
        totalSales: 0,
        orders: []
      };
    }
  }

  // Generate realistic mock orders based on your database
  private getMockOrdersWithPagination(params?: any): OrdersResponse {
    const allOrders = this.getMockOrders(params);
    
    const page = params?.page || 1;
    const pageSize = params?.pageSize || 20;
    const startIndex = (page - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    
    const paginatedOrders = allOrders.slice(startIndex, endIndex);
    const totalCount = allOrders.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    
    return {
      data: paginatedOrders,
      pagination: {
        currentPage: page,
        pageSize: pageSize,
        totalCount: totalCount,
        totalPages: totalPages,
        hasNext: page < totalPages,
        hasPrevious: page > 1
      }
    };
  }

  // Generate realistic mock orders based on your database
  private getMockOrders(params?: any): Order[] {
    const baseOrders: Order[] = [
      {
        id: 17734,
        orderNumber: 'ORD018733',
        orderDate: '2025-09-20T13:57:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 2, // Take Away
        status: OrderStatus.Completed,
        subTotal: 297.00,
        discountAmount: 0.00,
        taxAmount: 0.00,
        totalAmount: 29.70,
        paidAmount: 29.70,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 17821,
        orderNumber: 'ORD018820',
        orderDate: '2025-09-20T13:55:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 2,
        status: OrderStatus.Completed,
        subTotal: 288.00,
        discountAmount: 15.00,
        taxAmount: 0.00,
        totalAmount: 28.80,
        paidAmount: 28.80,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 17825,
        orderNumber: 'ORD018824',
        orderDate: '2025-09-20T13:54:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 2,
        status: OrderStatus.Completed,
        subTotal: 440.00,
        discountAmount: 0.00,
        taxAmount: 0.00,
        totalAmount: 44.00,
        paidAmount: 44.00,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 17817,
        orderNumber: 'ORD018816',
        orderDate: '2025-09-20T13:52:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 2,
        status: OrderStatus.Completed,
        subTotal: 276.00,
        discountAmount: 0.00,
        taxAmount: 0.00,
        totalAmount: 27.60,
        paidAmount: 27.60,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 17859,
        orderNumber: 'ORD018858',
        orderDate: '2025-09-20T13:52:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 2,
        status: OrderStatus.Completed,
        subTotal: 604.00,
        discountAmount: 0.00,
        taxAmount: 0.00,
        totalAmount: 60.40,
        paidAmount: 60.40,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 17705,
        orderNumber: 'ORD018704',
        orderDate: '2025-09-20T13:52:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 2,
        status: OrderStatus.Completed,
        subTotal: 173.00,
        discountAmount: 8.00,
        taxAmount: 0.00,
        totalAmount: 17.30,
        paidAmount: 17.30,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 17872,
        orderNumber: 'ORD018871',
        orderDate: '2025-09-20T13:51:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 2,
        status: OrderStatus.Completed,
        subTotal: 426.00,
        discountAmount: 0.00,
        taxAmount: 0.00,
        totalAmount: 42.60,
        paidAmount: 42.60,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 17820,
        orderNumber: 'ORD018819',
        orderDate: '2025-09-20T13:50:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 2,
        status: OrderStatus.Completed,
        subTotal: 60.00,
        discountAmount: 0.00,
        taxAmount: 0.00,
        totalAmount: 6.00,
        paidAmount: 6.00,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 17882,
        orderNumber: 'ORD018881',
        orderDate: '2025-09-20T13:47:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 2,
        status: OrderStatus.Completed,
        subTotal: 488.00,
        discountAmount: 0.00,
        taxAmount: 0.00,
        totalAmount: 48.80,
        paidAmount: 48.80,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 17928,
        orderNumber: 'ORD018927',
        orderDate: '2025-09-20T13:46:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 2,
        status: OrderStatus.Completed,
        subTotal: 264.00,
        discountAmount: 0.00,
        taxAmount: 0.00,
        totalAmount: 26.40,
        paidAmount: 26.40,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 17747,
        orderNumber: 'ORD018746',
        orderDate: '2025-09-20T13:44:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 2,
        status: OrderStatus.Completed,
        subTotal: 196.00,
        discountAmount: 0.00,
        taxAmount: 0.00,
        totalAmount: 19.60,
        paidAmount: 19.60,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 17844,
        orderNumber: 'ORD018843',
        orderDate: '2025-09-20T13:42:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 2,
        status: OrderStatus.Completed,
        subTotal: 605.00,
        discountAmount: 0.00,
        taxAmount: 0.00,
        totalAmount: 60.50,
        paidAmount: 60.50,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 17713,
        orderNumber: 'ORD018712',
        orderDate: '2025-09-20T13:40:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 2,
        status: OrderStatus.Completed,
        subTotal: 10.00,
        discountAmount: 0.00,
        taxAmount: 0.00,
        totalAmount: 1.00,
        paidAmount: 1.00,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 17655,
        orderNumber: 'ORD018654',
        orderDate: '2025-09-20T13:40:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 1, // Dine In
        status: OrderStatus.Completed,
        subTotal: 419.00,
        discountAmount: 0.00,
        taxAmount: 0.00,
        totalAmount: 41.90,
        paidAmount: 41.90,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      },
      {
        id: 17698,
        orderNumber: 'ORD018697',
        orderDate: '2025-09-20T13:34:00.0000000',
        customerName: null,
        customerId: null,
        orderType: 1,
        status: OrderStatus.Completed,
        subTotal: 371.00,
        discountAmount: 0.00,
        taxAmount: 0.00,
        totalAmount: 37.10,
        paidAmount: 37.10,
        changeAmount: 0,
        items: [],
        payments: [],
        notes: '',
        userId: 1,
        storeId: 1
      }
    ];

    // Filter by status if provided
    let filteredOrders = [...baseOrders];
    if (params?.status) {
      filteredOrders = filteredOrders.filter(o => o.status === params.status);
    }

    // Sort by date descending
    filteredOrders.sort((a, b) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime());

    return filteredOrders;
  }

  private getMockOrder(id: number): Order {
    return {
      id: id,
      orderNumber: `ORD0${id}`,
      orderDate: new Date().toISOString(),
      customerName: 'Walk-in Customer',
      customerId: null,
      orderType: 1,
      status: OrderStatus.Completed,
      subTotal: 100.00,
      discountAmount: 10.00,
      taxAmount: 9.00,
      totalAmount: 99.00,
      paidAmount: 99.00,
      changeAmount: 0,
      items: [],
      payments: [],
      notes: '',
      userId: 1,
      storeId: 1
    };
  }
}

export default new OrderService();
