import { Product } from './product.types';

// Order Types matching backend enums
// Backend expects STRING values for enums!
export type OrderStatus = 'Pending' | 'Processing' | 'Completed' | 'Cancelled' | 'Refunded' | 'PartiallyRefunded' | 'OnHold';
export type OrderType = 'DineIn' | 'TakeAway' | 'Delivery' | 'Pickup';
export type OrderSource = 'mobile' | 'pos';
export type PaymentMethod = 'Cash' | 'CreditCard' | 'DebitCard' | 'MobilePayment' | 'GiftCard' | 'LoyaltyPoints' | 'Other';
export type PaymentStatus = 'Pending' | 'Completed' | 'Failed' | 'Refunded' | 'PartiallyRefunded' | 'Cancelled';

// Enum constants for easier use - these are the STRING values
export const OrderTypeEnum = {
  DineIn: 'DineIn' as OrderType,
  TakeAway: 'TakeAway' as OrderType,
  Delivery: 'Delivery' as OrderType,
  Pickup: 'Pickup' as OrderType,
} as const;

export const PaymentMethodEnum = {
  Cash: 'Cash' as PaymentMethod,
  CreditCard: 'CreditCard' as PaymentMethod,
  DebitCard: 'DebitCard' as PaymentMethod,
  MobilePayment: 'MobilePayment' as PaymentMethod,
  GiftCard: 'GiftCard' as PaymentMethod,
  LoyaltyPoints: 'LoyaltyPoints' as PaymentMethod,
  Other: 'Other' as PaymentMethod,
} as const;

export const OrderStatusEnum = {
  Pending: 'Pending' as OrderStatus,
  Processing: 'Processing' as OrderStatus,
  Completed: 'Completed' as OrderStatus,
  Cancelled: 'Cancelled' as OrderStatus,
  Refunded: 'Refunded' as OrderStatus,
  PartiallyRefunded: 'PartiallyRefunded' as OrderStatus,
  OnHold: 'OnHold' as OrderStatus,
} as const;

export const PaymentStatusEnum = {
  Pending: 'Pending' as PaymentStatus,
  Completed: 'Completed' as PaymentStatus,
  Failed: 'Failed' as PaymentStatus,
  Refunded: 'Refunded' as PaymentStatus,
  PartiallyRefunded: 'PartiallyRefunded' as PaymentStatus,
  Cancelled: 'Cancelled' as PaymentStatus,
} as const;

export interface OrderItem {
  id?: number;
  productId: number;
  product?: Product;
  productName?: string;
  productSKU?: string;
  quantity: number;
  unitPriceIncGst: number;
  discountAmount: number;
  totalAmount: number;
  notes?: string;
  isVoided?: boolean;
}

export interface Order {
  id: number;
  orderNumber: string;
  orderDate: string;
  status: OrderStatus;
  orderType: OrderType;
  customerId?: number;
  customerName?: string;
  storeId: number;
  storeName?: string;
  cashierName: string;
  tableNumber?: string;
  notes?: string;
  items: OrderItem[];
  subTotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  changeAmount: number;
  completedAt?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface Payment {
  id: number;
  amount: number;
  paymentMethod: PaymentMethod;
  status: PaymentStatus;
  referenceNumber?: string;
  cardLastFourDigits?: string;
  cardType?: string;
  paymentDate: string;
}

// Create Order DTOs - matching backend structure
export interface CreateOrderItemDto {
  productId: number;
  quantity: number;
  discountAmount?: number;
  notes?: string;
}

export interface CreateOrderDto {
  orderType: OrderType;
  storeId?: number;  // Added for mobile app orders
  tableNumber?: string;
  customerId?: number;
  notes?: string;
  items: CreateOrderItemDto[];
  discountAmount?: number;
}

export interface CreateOrderResponse {
  orderId: number;
  orderNumber: string;
}

// Payment DTOs
export interface ProcessPaymentDto {
  orderId: number;
  amount: number;
  paymentMethod: PaymentMethod;
  referenceNumber?: string;
  cardLastFourDigits?: string;
  cardType?: string;
  notes?: string;
}

// Legacy types for backward compatibility (will be removed)
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
