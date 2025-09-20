// API Response types
export interface ApiResponse<T> {
  data?: T;
  message?: string;
  errors?: string[];
}

// Enums
export enum OrderStatus {
  Pending = 1,
  Processing = 2,
  Completed = 3,
  Cancelled = 4,
  Refunded = 5,
  PartiallyRefunded = 6,
  OnHold = 7
}

export enum OrderType {
  DineIn = 1,
  TakeAway = 2,
  Delivery = 3,
  Pickup = 4
}

export enum PaymentMethod {
  Cash = 1,
  CreditCard = 2,
  DebitCard = 3,
  MobilePayment = 4,
  GiftCard = 5,
  LoyaltyPoints = 6,
  Other = 7
}

export enum PaymentStatus {
  Pending = 1,
  Completed = 2,
  Failed = 3,
  Refunded = 4,
  PartiallyRefunded = 5,
  Cancelled = 6
}

export enum UserRole {
  Admin = 1,
  Manager = 2,
  Cashier = 3,
  Staff = 4,
  ReadOnly = 5
}

// Entity types
export interface Category {
  id: number;
  name: string;
  slug: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  subcategories?: Subcategory[];
}

export interface Subcategory {
  id: number;
  name: string;
  slug: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  categoryId: number;
  categoryName: string;
}

export interface Product {
  id: number;
  name: string;
  slug: string;
  sku?: string;
  barcode?: string;
  description?: string;
  priceExGst: number;
  gstAmount: number;
  priceIncGst: number;
  cost?: number;
  packNotes?: string;
  packSize?: number;
  imageUrl?: string;
  isActive: boolean;
  trackInventory: boolean;
  stockQuantity: number;
  lowStockThreshold: number;
  subcategoryId: number;
  subcategoryName: string;
  categoryId: number;
  categoryName: string;
}

export interface ProductListItem {
  id: number;
  name: string;
  sku?: string;
  priceIncGst: number;
  stockQuantity: number;
  isActive: boolean;
  imageUrl?: string;
  categoryName: string;
  subcategoryName: string;
}

export interface CartItem {
  productId: number;
  productName: string;
  sku?: string;
  quantity: number;
  unitPrice: number;
  discountAmount: number;
  subtotal: number;
  notes?: string;
}

export interface Order {
  id: number;
  orderNumber: string;
  orderDate: Date;
  status: OrderStatus;
  orderType: OrderType;
  subTotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  changeAmount: number;
  notes?: string;
  tableNumber?: string;
  completedAt?: Date;
  customerId?: number;
  customerName?: string;
  cashierName: string;
  storeName: string;
  items: OrderItem[];
  payments: Payment[];
}

export interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  productSKU?: string;
  quantity: number;
  unitPriceIncGst: number;
  discountAmount: number;
  totalAmount: number;
  notes?: string;
  isVoided: boolean;
}

export interface Payment {
  id: number;
  amount: number;
  paymentMethod: PaymentMethod;
  status: PaymentStatus;
  referenceNumber?: string;
  cardLastFourDigits?: string;
  cardType?: string;
  paymentDate: Date;
}

export interface Customer {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  loyaltyCardNumber?: string;
  loyaltyPoints: number;
  totalPurchases: number;
  totalOrders: number;
  lastOrderDate?: Date;
  isActive: boolean;
}

export interface User {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  storeId?: number;
  storeName?: string;
  hasActiveShift?: boolean;
  activeShiftId?: number;
}

export interface Shift {
  id: number;
  shiftNumber: string;
  startTime: Date;
  endTime?: Date;
  startingCash: number;
  endingCash?: number;
  status: string;
  totalOrders: number;
  totalSales: number;
  cashSales?: number;
  cardSales?: number;
  otherSales?: number;
}

// Auth types
export interface LoginRequest {
  username: string;
  password: string;
}

export interface PinLoginRequest {
  pin: string;
  storeId: number;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresIn: number;
  user: User;
}

// Create types
export interface CreateOrderRequest {
  orderType: OrderType;
  tableNumber?: string;
  customerId?: number;
  notes?: string;
  items: CreateOrderItem[];
  discountAmount?: number;
}

export interface CreateOrderItem {
  productId: number;
  quantity: number;
  discountAmount?: number;
  notes?: string;
}

export interface ProcessPaymentRequest {
  orderId: number;
  amount: number;
  paymentMethod: PaymentMethod;
  referenceNumber?: string;
  cardLastFourDigits?: string;
  cardType?: string;
  notes?: string;
}

export interface OpenShiftRequest {
  startingCash: number;
  notes?: string;
}

export interface CloseShiftRequest {
  endingCash: number;
  notes?: string;
}
