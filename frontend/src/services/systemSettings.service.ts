import api from './api.service';

export interface GeneralSettingsDto {
  companyName: string;
  companyAddress: string;
  companyPhone: string;
  companyEmail: string;
  companyWebsite: string;
  companyLogo: string;
  timeZone: string;
  dateFormat: string;
  timeFormat: string;
  currency: string;
  currencySymbol: string;
}

export interface ReceiptSettingsDto {
  headerText: string;
  footerText: string;
  showLogo: boolean;
  showTaxDetails: boolean;
  showItemDetails: boolean;
  showBarcode?: boolean;
  showQRCode?: boolean;
  showCustomerInfo?: boolean;
  paperSize: string;
  fontSize: number;
  receiptTemplate: string;
  customTemplate?: string;
  printMarginTop?: number;
  printMarginBottom?: number;
  printMarginLeft?: number;
  printMarginRight?: number;
}

export interface EmailSettingsDto {
  smtpHost: string;
  smtpPort: number;
  smtpUsername: string;
  smtpPassword: string;
  smtpUseSsl: boolean;
  fromEmail: string;
  fromName: string;
  enableEmailReceipts: boolean;
  enableEmailNotifications: boolean;
  enableLowStockAlerts?: boolean;
  enableDailySalesReport?: boolean;
  emailProvider?: string;
}

export interface DefaultValuesDto {
  defaultTaxRate: number;
  defaultLowStockThreshold: number;
  defaultOrderStatus: string;
  defaultPaymentMethod: string;
  sessionTimeoutMinutes: number;
  receiptPrintCopies: number;
  autoPrintReceipt: boolean;
  requireCustomerForOrder: boolean;
  autoOpenCashDrawer?: boolean;
  enableBarcodeLookup?: boolean;
  enableQuickSale?: boolean;
  passwordMinLength?: number;
  requireStrongPassword?: boolean;
}

export interface AllSettingsDto {
  general: GeneralSettingsDto;
  receipt: ReceiptSettingsDto;
  email: EmailSettingsDto;
  defaults: DefaultValuesDto;
}

export interface SettingsExportDto {
  receipt: ReceiptSettingsDto;
  email: EmailSettingsDto;
  defaults: DefaultValuesDto;
  exportedAt: string;
  exportedBy: string;
  version: string;
}

const systemSettingsService = {
  getAllSettings: async (): Promise<AllSettingsDto> => {
    const response = await api.get('/systemsettings');
    return response.data.data;
  },

  // General Settings (Legacy - kept for backward compatibility)
  getGeneralSettings: async (): Promise<GeneralSettingsDto> => {
    const response = await api.get('/systemsettings/general');
    return response.data.data;
  },

  updateGeneralSettings: async (settings: GeneralSettingsDto): Promise<void> => {
    await api.put('/systemsettings/general', settings);
  },

  // Receipt Settings
  getReceiptSettings: async (): Promise<ReceiptSettingsDto> => {
    const response = await api.get('/systemsettings/receipt', {
      headers: {
        'Cache-Control': 'no-cache, no-store, must-revalidate',
        'Pragma': 'no-cache',
        'Expires': '0'
      }
    });
    
    console.log('🌐 [API] Receipt settings response:', {
      receiptTemplate: response.data.data?.receiptTemplate,
      paperSize: response.data.data?.paperSize,
      fontSize: response.data.data?.fontSize
    });
    
    return response.data.data;
  },

  updateReceiptSettings: async (settings: ReceiptSettingsDto): Promise<void> => {
    await api.put('/systemsettings/receipt', settings);
  },

  // Email Settings
  getEmailSettings: async (): Promise<EmailSettingsDto> => {
    const response = await api.get('/systemsettings/email');
    return response.data.data;
  },

  updateEmailSettings: async (settings: EmailSettingsDto): Promise<void> => {
    await api.put('/systemsettings/email', settings);
  },

  testEmailSettings: async (settings: EmailSettingsDto, testEmail: string): Promise<boolean> => {
    const response = await api.post(`/systemsettings/email/test?testEmail=${encodeURIComponent(testEmail)}`, settings);
    return response.data.data;
  },

  // Default Values
  getDefaultValues: async (): Promise<DefaultValuesDto> => {
    const response = await api.get('/systemsettings/defaults');
    return response.data.data;
  },

  updateDefaultValues: async (settings: DefaultValuesDto): Promise<void> => {
    await api.put('/systemsettings/defaults', settings);
  },

  // Reset to Defaults
  resetToDefaults: async (): Promise<void> => {
    await api.post('/systemsettings/reset');
  },

  // Export/Import (client-side only for now)
  exportSettings: async (): Promise<SettingsExportDto> => {
    const [receipt, email, defaults] = await Promise.all([
      systemSettingsService.getReceiptSettings(),
      systemSettingsService.getEmailSettings(),
      systemSettingsService.getDefaultValues()
    ]);

    return {
      receipt,
      email: { ...email, smtpPassword: '***ENCRYPTED***' }, // Don't export password
      defaults,
      exportedAt: new Date().toISOString(),
      exportedBy: 'System', // TODO: Get current user
      version: '1.0'
    };
  }
};

export default systemSettingsService;
