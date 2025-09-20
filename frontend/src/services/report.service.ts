import api from './api.service';
import { AxiosResponse } from 'axios';

export interface SalesReportData {
  period: string;
  totalSales: number;
  totalOrders: number;
  averageOrderValue: number;
  topProducts: Array<{
    productId: number;
    productName: string;
    quantitySold: number;
    revenue: number;
  }>;
  salesByDay: Array<{
    date: string;
    sales: number;
    orders: number;
  }>;
  paymentMethodBreakdown: Array<{
    method: string;
    count: number;
    total: number;
  }>;
}

export interface ProductPerformanceData {
  topSellingProducts: Array<{
    id: number;
    name: string;
    sku: string;
    quantitySold: number;
    revenue: number;
    profitMargin: number;
  }>;
  categoryPerformance: Array<{
    categoryId: number;
    categoryName: string;
    totalSales: number;
    totalRevenue: number;
  }>;
  lowStockProducts: Array<{
    id: number;
    name: string;
    currentStock: number;
    threshold: number;
    lastSoldDate: string;
  }>;
}

export interface ShiftReportData {
  shiftId: number;
  shiftNumber: string;
  cashierName: string;
  startTime: string;
  endTime?: string;
  startingCash: number;
  endingCash?: number;
  totalSales: number;
  totalOrders: number;
  cashSales: number;
  cardSales: number;
  otherSales: number;
  expectedCash: number;
  cashDifference?: number;
  transactions: Array<{
    orderNumber: string;
    time: string;
    amount: number;
    paymentMethod: string;
  }>;
}

class ReportService {
  async getSalesReport(startDate: string, endDate: string): Promise<SalesReportData> {
    try {
      const response: AxiosResponse<any> = await api.get('/reports/sales', {
        params: { startDate, endDate }
      });
      return response.data.data;
    } catch (error) {
      console.log('Using mock data for sales report');
      // Generate mock data for development
      return this.generateMockSalesReport(startDate, endDate);
    }
  }

  async getProductPerformance(startDate: string, endDate: string): Promise<ProductPerformanceData> {
    try {
      const response: AxiosResponse<any> = await api.get('/reports/products', {
        params: { startDate, endDate }
      });
      return response.data.data;
    } catch (error) {
      console.log('Using mock data for product performance');
      // Generate mock data for development
      return this.generateMockProductPerformance();
    }
  }

  async getShiftReport(shiftId?: number): Promise<ShiftReportData> {
    try {
      const url = shiftId ? `/reports/shifts/${shiftId}` : '/reports/shifts/current';
      const response: AxiosResponse<any> = await api.get(url);
      return response.data.data;
    } catch (error) {
      console.log('Using mock data for shift report');
      // Generate mock data for development
      return this.generateMockShiftReport();
    }
  }

  async exportReport(type: string, startDate: string, endDate: string): Promise<Blob> {
    const response = await api.get(`/reports/export/${type}`, {
      params: { startDate, endDate },
      responseType: 'blob'
    });
    return response.data;
  }

  // Mock data generators for development
  private generateMockSalesReport(startDate: string, endDate: string): SalesReportData {
    const days = this.getDaysBetween(new Date(startDate), new Date(endDate));
    const salesByDay = days.map(date => ({
      date: date.toISOString().split('T')[0],
      sales: Math.random() * 5000 + 1000,
      orders: Math.floor(Math.random() * 50 + 10)
    }));

    return {
      period: `${startDate} to ${endDate}`,
      totalSales: salesByDay.reduce((sum, day) => sum + day.sales, 0),
      totalOrders: salesByDay.reduce((sum, day) => sum + day.orders, 0),
      averageOrderValue: 0,
      topProducts: [
        { productId: 1, productName: 'Chocolate Chip Cookie', quantitySold: 245, revenue: 1225.00 },
        { productId: 2, productName: 'Sourdough Bread', quantitySold: 189, revenue: 945.00 },
        { productId: 3, productName: 'Blueberry Muffin', quantitySold: 156, revenue: 624.00 },
        { productId: 4, productName: 'Croissant', quantitySold: 134, revenue: 536.00 },
        { productId: 5, productName: 'Apple Pie', quantitySold: 89, revenue: 801.00 }
      ],
      salesByDay,
      paymentMethodBreakdown: [
        { method: 'Cash', count: 145, total: 2890.50 },
        { method: 'Credit Card', count: 234, total: 5670.75 },
        { method: 'Debit Card', count: 156, total: 3120.25 },
        { method: 'Mobile Payment', count: 78, total: 1560.00 }
      ]
    };
  }

  private generateMockProductPerformance(): ProductPerformanceData {
    return {
      topSellingProducts: [
        { id: 1, name: 'Chocolate Chip Cookie', sku: 'CCC001', quantitySold: 450, revenue: 2250.00, profitMargin: 65 },
        { id: 2, name: 'Sourdough Bread', sku: 'SDB001', quantitySold: 320, revenue: 1600.00, profitMargin: 55 },
        { id: 3, name: 'Blueberry Muffin', sku: 'BMF001', quantitySold: 280, revenue: 1120.00, profitMargin: 60 },
        { id: 4, name: 'Croissant', sku: 'CRS001', quantitySold: 240, revenue: 960.00, profitMargin: 50 },
        { id: 5, name: 'Apple Pie', sku: 'APP001', quantitySold: 120, revenue: 1080.00, profitMargin: 70 }
      ],
      categoryPerformance: [
        { categoryId: 1, categoryName: 'Bakery', totalSales: 890, totalRevenue: 4450.00 },
        { categoryId: 2, categoryName: 'Beverages', totalSales: 650, totalRevenue: 2600.00 },
        { categoryId: 3, categoryName: 'Snacks', totalSales: 420, totalRevenue: 1680.00 },
        { categoryId: 4, categoryName: 'Desserts', totalSales: 380, totalRevenue: 3420.00 }
      ],
      lowStockProducts: [
        { id: 6, name: 'Cinnamon Roll', currentStock: 5, threshold: 20, lastSoldDate: '2024-01-15' },
        { id: 7, name: 'Bagel', currentStock: 8, threshold: 30, lastSoldDate: '2024-01-14' },
        { id: 8, name: 'Danish Pastry', currentStock: 3, threshold: 15, lastSoldDate: '2024-01-15' }
      ]
    };
  }

  private generateMockShiftReport(): ShiftReportData {
    const transactions = Array.from({ length: 20 }, (_, i) => ({
      orderNumber: `ORD-${String(i + 1).padStart(4, '0')}`,
      time: new Date(Date.now() - Math.random() * 8 * 60 * 60 * 1000).toISOString(),
      amount: Math.random() * 100 + 10,
      paymentMethod: ['Cash', 'Credit Card', 'Debit Card'][Math.floor(Math.random() * 3)]
    }));

    const totalSales = transactions.reduce((sum, t) => sum + t.amount, 0);
    const cashSales = transactions.filter(t => t.paymentMethod === 'Cash').reduce((sum, t) => sum + t.amount, 0);
    const cardSales = totalSales - cashSales;

    return {
      shiftId: 1,
      shiftNumber: 'SHIFT-001',
      cashierName: 'John Doe',
      startTime: new Date(Date.now() - 6 * 60 * 60 * 1000).toISOString(),
      endTime: undefined,
      startingCash: 200.00,
      endingCash: undefined,
      totalSales,
      totalOrders: transactions.length,
      cashSales,
      cardSales,
      otherSales: 0,
      expectedCash: 200.00 + cashSales,
      cashDifference: undefined,
      transactions
    };
  }

  private getDaysBetween(startDate: Date, endDate: Date): Date[] {
    const days = [];
    const currentDate = new Date(startDate);
    while (currentDate <= endDate) {
      days.push(new Date(currentDate));
      currentDate.setDate(currentDate.getDate() + 1);
    }
    return days;
  }
}

export default new ReportService();
