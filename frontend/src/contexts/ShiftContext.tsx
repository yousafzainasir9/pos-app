import React, { createContext, useContext, useState, ReactNode, useEffect } from 'react';
import { Shift, OpenShiftRequest, CloseShiftRequest } from '@/types';
import apiService from '@/services/api.service';
import authService from '@/services/auth.service';
import { toast } from 'react-toastify';

interface ShiftContextType {
  currentShift: Shift | null;
  isShiftOpen: boolean;
  isLoading: boolean;
  openShift: (request: OpenShiftRequest) => Promise<void>;
  closeShift: (request: CloseShiftRequest) => Promise<void>;
  loadCurrentShift: () => Promise<void>;
}

const ShiftContext = createContext<ShiftContextType | undefined>(undefined);

export const ShiftProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [currentShift, setCurrentShift] = useState<Shift | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    // Only load shift if user is authenticated
    if (authService.isAuthenticated()) {
      loadCurrentShift();
    }
  }, []);

  const loadCurrentShift = async () => {
    // Check if user is authenticated before making API call
    if (!authService.isAuthenticated()) {
      setCurrentShift(null);
      return;
    }

    try {
      setIsLoading(true);
      const response = await apiService.get<Shift>('/shifts/current');
      setCurrentShift(response.data);
    } catch (error: any) {
      if (error.response?.status !== 404 && error.response?.status !== 401) {
        console.error('Failed to load current shift:', error);
      }
      setCurrentShift(null);
    } finally {
      setIsLoading(false);
    }
  };

  const openShift = async (request: OpenShiftRequest) => {
    try {
      setIsLoading(true);
      const response = await apiService.post<Shift>('/shifts/open', request);
      setCurrentShift(response.data);
      toast.success('Shift opened successfully');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to open shift');
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const closeShift = async (request: CloseShiftRequest) => {
    if (!currentShift) {
      toast.error('No active shift to close');
      return;
    }

    try {
      setIsLoading(true);
      const response = await apiService.post<any>(`/shifts/${currentShift.id}/close`, request);
      
      // Show shift summary
      const { cashDifference, totalSales, totalOrders } = response.data;
      
      toast.success(
        `Shift closed successfully!\n` +
        `Total Sales: $${totalSales.toFixed(2)}\n` +
        `Total Orders: ${totalOrders}\n` +
        `Cash Difference: $${cashDifference.toFixed(2)}`,
        { autoClose: 5000 }
      );
      
      setCurrentShift(null);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to close shift');
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const value: ShiftContextType = {
    currentShift,
    isShiftOpen: !!currentShift,
    isLoading,
    openShift,
    closeShift,
    loadCurrentShift
  };

  return <ShiftContext.Provider value={value}>{children}</ShiftContext.Provider>;
};

export const useShift = () => {
  const context = useContext(ShiftContext);
  if (context === undefined) {
    throw new Error('useShift must be used within a ShiftProvider');
  }
  return context;
};