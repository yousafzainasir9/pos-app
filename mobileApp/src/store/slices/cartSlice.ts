import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { CartItem, Product } from '../../types/product.types';

interface CartState {
  items: CartItem[];
  subtotal: number;
  gstAmount: number;
  totalAmount: number;
}

const GST_RATE = 0.1; // 10% GST

const calculateTotals = (items: CartItem[]) => {
  const subtotal = items.reduce(
    (sum, item) => sum + item.product.price * item.quantity,
    0
  );
  const gstAmount = subtotal * GST_RATE;
  const totalAmount = subtotal + gstAmount;

  return { subtotal, gstAmount, totalAmount };
};

const initialState: CartState = {
  items: [],
  subtotal: 0,
  gstAmount: 0,
  totalAmount: 0,
};

const cartSlice = createSlice({
  name: 'cart',
  initialState,
  reducers: {
    addToCart: (state, action: PayloadAction<Product>) => {
      const existingItem = state.items.find(
        item => item.product.id === action.payload.id
      );

      if (existingItem) {
        existingItem.quantity += 1;
      } else {
        state.items.push({
          product: action.payload,
          quantity: 1,
        });
      }

      const totals = calculateTotals(state.items);
      state.subtotal = totals.subtotal;
      state.gstAmount = totals.gstAmount;
      state.totalAmount = totals.totalAmount;
    },

    removeFromCart: (state, action: PayloadAction<number>) => {
      state.items = state.items.filter(
        item => item.product.id !== action.payload
      );

      const totals = calculateTotals(state.items);
      state.subtotal = totals.subtotal;
      state.gstAmount = totals.gstAmount;
      state.totalAmount = totals.totalAmount;
    },

    updateQuantity: (
      state,
      action: PayloadAction<{ productId: number; quantity: number }>
    ) => {
      const item = state.items.find(
        item => item.product.id === action.payload.productId
      );

      if (item) {
        if (action.payload.quantity <= 0) {
          state.items = state.items.filter(
            item => item.product.id !== action.payload.productId
          );
        } else {
          item.quantity = action.payload.quantity;
        }

        const totals = calculateTotals(state.items);
        state.subtotal = totals.subtotal;
        state.gstAmount = totals.gstAmount;
        state.totalAmount = totals.totalAmount;
      }
    },

    clearCart: (state) => {
      state.items = [];
      state.subtotal = 0;
      state.gstAmount = 0;
      state.totalAmount = 0;
    },
  },
});

export const { addToCart, removeFromCart, updateQuantity, clearCart } =
  cartSlice.actions;
export default cartSlice.reducer;
