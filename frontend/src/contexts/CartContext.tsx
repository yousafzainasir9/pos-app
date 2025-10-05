import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { Product, CartItem } from '@/types';
import { toast } from 'react-toastify';

interface CartContextType {
  items: CartItem[];
  totalItems: number;
  totalAmount: number;
  taxAmount: number;
  subTotal: number;
  addItem: (product: Product, quantity?: number, notes?: string) => void;
  removeItem: (productId: number) => void;
  updateQuantity: (productId: number, quantity: number) => void;
  clearCart: () => void;
  getItem: (productId: number) => CartItem | undefined;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

export const CartProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [items, setItems] = useState<CartItem[]>([]);

  // Load cart from localStorage on mount
  useEffect(() => {
    const savedCart = localStorage.getItem('cart');
    if (savedCart) {
      try {
        setItems(JSON.parse(savedCart));
      } catch (error) {
        console.error('Failed to load cart:', error);
      }
    }
  }, []);

  // Save cart to localStorage whenever it changes
  useEffect(() => {
    localStorage.setItem('cart', JSON.stringify(items));
  }, [items]);

  const addItem = (product: Product, quantity: number = 1, notes?: string) => {
    setItems(prevItems => {
      const existingItem = prevItems.find(item => item.productId === product.id);
      
      if (existingItem) {
        // Update quantity if item already exists
        return prevItems.map(item =>
          item.productId === product.id
            ? {
                ...item,
                quantity: item.quantity + quantity,
                subtotal: (item.quantity + quantity) * item.unitPrice,
                notes: notes || item.notes
              }
            : item
        );
      } else {
        // Add new item
        const newItem: CartItem = {
          productId: product.id,
          productName: product.name,
          sku: product.sku,
          quantity,
          unitPrice: product.priceIncGst,
          discountAmount: 0,
          subtotal: quantity * product.priceIncGst,
          notes: notes
        };
        toast.success(`${product.name} added to cart`);
        return [...prevItems, newItem];
      }
    });
  };

  const removeItem = (productId: number) => {
    setItems(prevItems => {
      const item = prevItems.find(i => i.productId === productId);
      if (item) {
        toast.info(`${item.productName} removed from cart`);
      }
      return prevItems.filter(item => item.productId !== productId);
    });
  };

  const updateQuantity = (productId: number, quantity: number) => {
    if (quantity <= 0) {
      removeItem(productId);
      return;
    }

    setItems(prevItems =>
      prevItems.map(item =>
        item.productId === productId
          ? {
              ...item,
              quantity,
              subtotal: quantity * item.unitPrice - item.discountAmount
            }
          : item
      )
    );
  };

  const clearCart = () => {
    setItems([]);
    localStorage.removeItem('cart');
    toast.info('Cart cleared');
  };

  const getItem = (productId: number) => {
    return items.find(item => item.productId === productId);
  };

  // Calculate totals
  const totalItems = items.reduce((sum, item) => sum + item.quantity, 0);
  const subTotal = items.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0);
  const totalDiscounts = items.reduce((sum, item) => sum + item.discountAmount, 0);
  const afterDiscounts = subTotal - totalDiscounts;
  
  // GST is already included in the price, so we calculate the GST component
  const taxAmount = afterDiscounts * (0.1 / 1.1); // Extract 10% GST from GST-inclusive price
  const totalAmount = afterDiscounts;

  const value: CartContextType = {
    items,
    totalItems,
    totalAmount,
    taxAmount,
    subTotal: afterDiscounts - taxAmount, // Excluding GST
    addItem,
    removeItem,
    updateQuantity,
    clearCart,
    getItem
  };

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
};

export const useCart = () => {
  const context = useContext(CartContext);
  if (context === undefined) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
};
