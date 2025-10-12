import { createSlice, PayloadAction, createAsyncThunk } from '@reduxjs/toolkit';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { storesApi } from '../../api/stores.api';

interface Store {
  id: number;
  name: string;
  address: string;
  phone: string;
  email: string;
  isActive: boolean;
}

interface StoreState {
  stores: Store[];
  selectedStore: Store | null;
  selectedStoreId: number | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: StoreState = {
  stores: [],
  selectedStore: null,
  selectedStoreId: null,
  isLoading: false,
  error: null,
};

// Async thunk to fetch stores
export const fetchStores = createAsyncThunk(
  'store/fetchStores',
  async (_, { rejectWithValue }) => {
    try {
      const response = await storesApi.getStores();
      return response.data;
    } catch (error: any) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch stores'
      );
    }
  }
);

// Async thunk to restore selected store from AsyncStorage
export const restoreSelectedStore = createAsyncThunk(
  'store/restoreSelectedStore',
  async () => {
    const storeIdStr = await AsyncStorage.getItem('selectedStoreId');
    if (storeIdStr) {
      return parseInt(storeIdStr, 10);
    }
    return null;
  }
);

const storeSlice = createSlice({
  name: 'store',
  initialState,
  reducers: {
    setSelectedStore: (state, action: PayloadAction<number>) => {
      state.selectedStoreId = action.payload;
      // Find and set the selected store object
      state.selectedStore = state.stores.find(s => s.id === action.payload) || null;
      // Save to AsyncStorage
      AsyncStorage.setItem('selectedStoreId', action.payload.toString());
    },
    clearSelectedStore: (state) => {
      state.selectedStoreId = null;
      state.selectedStore = null;
      AsyncStorage.removeItem('selectedStoreId');
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch stores
      .addCase(fetchStores.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchStores.fulfilled, (state, action) => {
        state.isLoading = false;
        state.stores = action.payload;
        state.error = null;
      })
      .addCase(fetchStores.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Restore selected store
      .addCase(restoreSelectedStore.fulfilled, (state, action) => {
        if (action.payload) {
          state.selectedStoreId = action.payload;
        }
      });
  },
});

export const { setSelectedStore, clearSelectedStore } = storeSlice.actions;
export default storeSlice.reducer;
