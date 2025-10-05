import api from './api.service';

export interface BackupDto {
  fileName: string;
  createdAt: string;
  sizeInBytes: number;
  sizeFormatted: string;
  backupType: string;
}

export interface BackupRequestDto {
  backupName: string;
  includeAuditLogs: boolean;
  includeSecurityLogs: boolean;
}

export interface ImportResultDto {
  totalRecords: number;
  successfulImports: number;
  failedImports: number;
  updatedRecords: number;
  errors: string[];
  warnings: string[];
}

export interface ExportDataRequestDto {
  entityType: string;
  startDate?: string;
  endDate?: string;
  format: string;
  storeId?: number;
}

export interface DataStatisticsDto {
  totalProducts: number;
  totalCustomers: number;
  totalOrders: number;
  totalUsers: number;
  databaseSizeInBytes: number;
  databaseSizeFormatted: string;
  lastBackupDate: string;
  backupCount: number;
}

export interface ArchiveDataRequestDto {
  archiveBeforeDate: string;
  archiveOrders: boolean;
  archiveAuditLogs: boolean;
  archiveSecurityLogs: boolean;
  deleteAfterArchive: boolean;
}

export interface ArchiveResultDto {
  ordersArchived: number;
  auditLogsArchived: number;
  securityLogsArchived: number;
  archiveFileName: string;
  archiveSizeInBytes: number;
}

const dataManagementService = {
  // Statistics
  getStatistics: async (): Promise<DataStatisticsDto> => {
    const response = await api.get('/datamanagement/statistics');
    return response.data.data;
  },

  // Backup
  createBackup: async (request: BackupRequestDto): Promise<BackupDto> => {
    const response = await api.post('/datamanagement/backup', request);
    return response.data.data;
  },

  getBackups: async (): Promise<BackupDto[]> => {
    const response = await api.get('/datamanagement/backups');
    return response.data.data;
  },

  deleteBackup: async (fileName: string): Promise<void> => {
    await api.delete(`/datamanagement/backups/${encodeURIComponent(fileName)}`);
  },

  // Import
  importProducts: async (fileContent: string, fileType: string, updateExisting: boolean): Promise<ImportResultDto> => {
    const response = await api.post('/datamanagement/import/products', {
      fileContent,
      fileType,
      updateExisting
    });
    return response.data.data;
  },

  importCustomers: async (fileContent: string, fileType: string, updateExisting: boolean): Promise<ImportResultDto> => {
    const response = await api.post('/datamanagement/import/customers', {
      fileContent,
      fileType,
      updateExisting
    });
    return response.data.data;
  },

  // Export
  exportData: async (request: ExportDataRequestDto): Promise<Blob> => {
    const response = await api.post('/datamanagement/export', request, {
      responseType: 'blob'
    });
    return response.data;
  },

  // Archive
  archiveData: async (request: ArchiveDataRequestDto): Promise<ArchiveResultDto> => {
    const response = await api.post('/datamanagement/archive', request);
    return response.data.data;
  },

  getArchives: async (): Promise<BackupDto[]> => {
    const response = await api.get('/datamanagement/archives');
    return response.data.data;
  }
};

export default dataManagementService;
