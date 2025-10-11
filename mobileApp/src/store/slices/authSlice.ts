import { createSlice, PayloadAction, createAsyncThunk } from '@reduxjs/toolkit';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { authApi, LoginRequest, PinLoginRequest } from '../../api/auth.api';
import { User, Customer } from '../../types/auth.types';

interface AuthState {
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isGuest: boolean;
  isLoading: boolean;
  error: string | null;
  // Legacy support
  customer: Customer | null;
}

const initialState: AuthState = {
  user: null,
  token: null,
  refreshToken: null,
  isAuthenticated: false,
  isGuest: false,
  isLoading: false,
  error: null,
  customer: null,
};

// Async thunk for login
export const loginUser = createAsyncThunk(
  'auth/loginUser',
  async (credentials: LoginRequest, { rejectWithValue }) => {
    try {
      const response = await authApi.login(credentials);
      
      // Save tokens to AsyncStorage
      await AsyncStorage.setItem('authToken', response.data.token);
      await AsyncStorage.setItem('refreshToken', response.data.refreshToken);
      await AsyncStorage.setItem('user', JSON.stringify(response.data.user));
      
      return response.data;
    } catch (error: any) {
      return rejectWithValue(
        error.response?.data?.message || 'Login failed. Please check your credentials.'
      );
    }
  }
);

// Async thunk for PIN login
export const pinLoginUser = createAsyncThunk(
  'auth/pinLoginUser',
  async (credentials: PinLoginRequest, { rejectWithValue }) => {
    try {
      const response = await authApi.pinLogin(credentials);
      
      // Save tokens to AsyncStorage
      await AsyncStorage.setItem('authToken', response.data.token);
      await AsyncStorage.setItem('refreshToken', response.data.refreshToken);
      await AsyncStorage.setItem('user', JSON.stringify(response.data.user));
      
      return response.data;
    } catch (error: any) {
      return rejectWithValue(
        error.response?.data?.message || 'PIN login failed. Please check your PIN and store selection.'
      );
    }
  }
);

// Async thunk for logout
export const logoutUser = createAsyncThunk(
  'auth/logoutUser',
  async (_, { getState }) => {
    const state = getState() as { auth: AuthState };
    const token = state.auth.token;
    
    try {
      if (token) {
        await authApi.logout(token);
      }
    } catch (error) {
      console.log('Logout error:', error);
    } finally {
      // Clear AsyncStorage regardless of API call success
      await AsyncStorage.removeItem('authToken');
      await AsyncStorage.removeItem('refreshToken');
      await AsyncStorage.removeItem('user');
    }
  }
);

// Async thunk to restore session from AsyncStorage
export const restoreSession = createAsyncThunk(
  'auth/restoreSession',
  async () => {
    const token = await AsyncStorage.getItem('authToken');
    const refreshToken = await AsyncStorage.getItem('refreshToken');
    const userStr = await AsyncStorage.getItem('user');
    
    if (token && userStr) {
      const user = JSON.parse(userStr);
      return { token, refreshToken, user };
    }
    
    return null;
  }
);

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
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Login
      .addCase(loginUser.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(loginUser.fulfilled, (state, action) => {
        state.isLoading = false;
        state.user = action.payload.user;
        state.token = action.payload.token;
        state.refreshToken = action.payload.refreshToken;
        state.isAuthenticated = true;
        state.isGuest = false;
        state.error = null;
        
        // Set customer for backward compatibility
        state.customer = {
          id: action.payload.user.customerId,
          name: `${action.payload.user.firstName} ${action.payload.user.lastName}`,
          phone: action.payload.user.phone || '',
        };
      })
      .addCase(loginUser.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // PIN Login
      .addCase(pinLoginUser.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(pinLoginUser.fulfilled, (state, action) => {
        state.isLoading = false;
        state.user = action.payload.user;
        state.token = action.payload.token;
        state.refreshToken = action.payload.refreshToken;
        state.isAuthenticated = true;
        state.isGuest = false;
        state.error = null;
        
        // Set customer for backward compatibility
        state.customer = {
          id: action.payload.user.customerId,
          name: `${action.payload.user.firstName} ${action.payload.user.lastName}`,
          phone: action.payload.user.phone || '',
        };
      })
      .addCase(pinLoginUser.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Logout
      .addCase(logoutUser.fulfilled, (state) => {
        state.user = null;
        state.token = null;
        state.refreshToken = null;
        state.isAuthenticated = false;
        state.isGuest = false;
        state.customer = null;
        state.error = null;
      })
      // Restore session
      .addCase(restoreSession.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(restoreSession.fulfilled, (state, action) => {
        state.isLoading = false;
        if (action.payload) {
          state.user = action.payload.user;
          state.token = action.payload.token;
          state.refreshToken = action.payload.refreshToken || null;
          state.isAuthenticated = true;
          
          // Set customer for backward compatibility
          state.customer = {
            id: action.payload.user.customerId,
            name: `${action.payload.user.firstName} ${action.payload.user.lastName}`,
            phone: action.payload.user.phone || '',
          };
        }
      })
      .addCase(restoreSession.rejected, (state) => {
        state.isLoading = false;
      });
  },
});

export const { setCustomer, setGuestMode, clearError } = authSlice.actions;
export default authSlice.reducer;
