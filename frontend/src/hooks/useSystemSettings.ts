import { useState, useEffect } from 'react';
import systemSettingsService, { 
  ReceiptSettingsDto, 
  EmailSettingsDto, 
  DefaultValuesDto 
} from '@/services/systemSettings.service';

// Hook to load and use receipt settings
export const useReceiptSettings = () => {
  const [settings, setSettings] = useState<ReceiptSettingsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    try {
      setLoading(true);
      const data = await systemSettingsService.getReceiptSettings();
      setSettings(data);
      setError(null);
    } catch (err: any) {
      console.error('Failed to load receipt settings:', err);
      setError(err.message || 'Failed to load receipt settings');
      // Use default settings if loading fails
      setSettings({
        headerText: 'Thank you for your purchase!',
        footerText: 'Please visit us again',
        showLogo: true,
        showTaxDetails: true,
        showItemDetails: true,
        showBarcode: false,
        showQRCode: false,
        showCustomerInfo: true,
        paperSize: '80mm',
        fontSize: 12,
        receiptTemplate: 'standard',
        printMarginTop: 0,
        printMarginBottom: 10,
        printMarginLeft: 5,
        printMarginRight: 5
      });
    } finally {
      setLoading(false);
    }
  };

  const refreshSettings = () => {
    loadSettings();
  };

  return { settings, loading, error, refreshSettings };
};

// Hook to load and use email settings
export const useEmailSettings = () => {
  const [settings, setSettings] = useState<EmailSettingsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    try {
      setLoading(true);
      const data = await systemSettingsService.getEmailSettings();
      setSettings(data);
      setError(null);
    } catch (err: any) {
      console.error('Failed to load email settings:', err);
      setError(err.message || 'Failed to load email settings');
      // Use default settings if loading fails
      setSettings({
        smtpHost: '',
        smtpPort: 587,
        smtpUsername: '',
        smtpPassword: '',
        smtpUseSsl: true,
        fromEmail: '',
        fromName: '',
        enableEmailReceipts: false,
        enableEmailNotifications: false,
        enableLowStockAlerts: false,
        enableDailySalesReport: false
      });
    } finally {
      setLoading(false);
    }
  };

  return { settings, loading, error };
};

// Hook to load and use default values
export const useDefaultValues = () => {
  const [settings, setSettings] = useState<DefaultValuesDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    try {
      setLoading(true);
      const data = await systemSettingsService.getDefaultValues();
      setSettings(data);
      setError(null);
    } catch (err: any) {
      console.error('Failed to load default values:', err);
      setError(err.message || 'Failed to load default values');
      // Use default settings if loading fails
      setSettings({
        defaultTaxRate: 10.0,
        defaultLowStockThreshold: 10,
        defaultOrderStatus: 'Pending',
        defaultPaymentMethod: 'Cash',
        sessionTimeoutMinutes: 60,
        receiptPrintCopies: 1,
        autoPrintReceipt: true,
        requireCustomerForOrder: false,
        autoOpenCashDrawer: true,
        enableBarcodeLookup: true,
        enableQuickSale: true,
        passwordMinLength: 6,
        requireStrongPassword: false
      });
    } finally {
      setLoading(false);
    }
  };

  const refreshSettings = () => {
    loadSettings();
  };

  return { settings, loading, error, refreshSettings };
};

// Singleton pattern to cache settings across components
let cachedReceiptSettings: ReceiptSettingsDto | null = null;
let cachedDefaultValues: DefaultValuesDto | null = null;

export const getCachedReceiptSettings = async (): Promise<ReceiptSettingsDto> => {
  if (!cachedReceiptSettings) {
    try {
      cachedReceiptSettings = await systemSettingsService.getReceiptSettings();
    } catch (error) {
      console.error('Failed to load receipt settings, using defaults:', error);
      cachedReceiptSettings = {
        headerText: 'Thank you for your purchase!',
        footerText: 'Please visit us again',
        showLogo: true,
        showTaxDetails: true,
        showItemDetails: true,
        showBarcode: false,
        showQRCode: false,
        showCustomerInfo: true,
        paperSize: '80mm',
        fontSize: 12,
        receiptTemplate: 'standard',
        printMarginTop: 0,
        printMarginBottom: 10,
        printMarginLeft: 5,
        printMarginRight: 5
      };
    }
  }
  return cachedReceiptSettings;
};

export const getCachedDefaultValues = async (): Promise<DefaultValuesDto> => {
  if (!cachedDefaultValues) {
    try {
      cachedDefaultValues = await systemSettingsService.getDefaultValues();
    } catch (error) {
      console.error('Failed to load default values, using defaults:', error);
      cachedDefaultValues = {
        defaultTaxRate: 10.0,
        defaultLowStockThreshold: 10,
        defaultOrderStatus: 'Pending',
        defaultPaymentMethod: 'Cash',
        sessionTimeoutMinutes: 60,
        receiptPrintCopies: 1,
        autoPrintReceipt: true,
        requireCustomerForOrder: false,
        autoOpenCashDrawer: true,
        enableBarcodeLookup: true,
        enableQuickSale: true,
        passwordMinLength: 6,
        requireStrongPassword: false
      };
    }
  }
  return cachedDefaultValues;
};

// Clear cache when settings are updated
export const clearSettingsCache = () => {
  cachedReceiptSettings = null;
  cachedDefaultValues = null;
};
