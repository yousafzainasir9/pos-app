import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Customer } from '../../types/auth.types';

interface AuthState {
  customer: Customer | null;
  isAuthenticated: boolean;
  isGuest: boolean;
}

const initialState: AuthState = {
  customer: null,
  isAuthenticated: false,
  isGuest: false,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setCustomer: (state, action: PayloadAction<Customer>) => {
      state.customer = action.payload;
      state.isAuthenticated = true;
      state.isGuest = false;
    },
    setGuestMode: (state, action: PayloadAction<{ name: string; phone: string }>) => {
      state.customer = action.payload;
      state.isAuthenticated = false;
      state.isGuest = true;
    },
    logout: (state) => {
      state.customer = null;
      state.isAuthenticated = false;
      state.isGuest = false;
    },
  },
});

export const { setCustomer, setGuestMode, logout } = authSlice.actions;
export default authSlice.reducer;
