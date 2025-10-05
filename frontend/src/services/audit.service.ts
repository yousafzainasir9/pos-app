import api from './api.service';

export interface AuditLog {
  id: number;
  timestamp: string;
  userId?: number;
  userName?: string;
  action: string;
  entityName: string;
  entityId?: string;
  oldValues?: string;
  newValues?: string;
  details?: string;
  ipAddress?: string;
  userAgent?: string;
  storeId?: number;
  storeName?: string;
}

export interface SecurityLog {
  id: number;
  timestamp: string;
  eventType: number;
  eventTypeName: string;
  severity: number;
  severityName: string;
  userId?: number;
  userName?: string;
  description: string;
  ipAddress?: string;
  userAgent?: string;
  success: boolean;
  metadata?: string;
  storeId?: number;
  storeName?: string;
}

export interface AuditLogSearchRequest {
  startDate?: string;
  endDate?: string;
  userId?: number;
  action?: string;
  entityName?: string;
  storeId?: number;
  page?: number;
  pageSize?: number;
}

export interface SecurityLogSearchRequest {
  startDate?: string;
  endDate?: string;
  userId?: number;
  eventType?: number;
  severity?: number;
  success?: boolean;
  storeId?: number;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface AuditStatistics {
  totalAuditLogs: number;
  totalSecurityLogs: number;
  failedLoginAttempts24h: number;
  unauthorizedAccess24h: number;
  criticalEvents7d: number;
  topUsers: TopUserActivity[];
  activityByDay: ActivityByDay[];
  securityEventCounts: SecurityEventCount[];
}

export interface TopUserActivity {
  userId: number;
  userName: string;
  activityCount: number;
}

export interface ActivityByDay {
  date: string;
  count: number;
}

export interface SecurityEventCount {
  eventType: number;
  eventTypeName: string;
  count: number;
}

const auditService = {
  // Audit Logs
  getAuditLogs: async (request: AuditLogSearchRequest): Promise<PagedResult<AuditLog>> => {
    const params = new URLSearchParams();
    if (request.startDate) params.append('startDate', request.startDate);
    if (request.endDate) params.append('endDate', request.endDate);
    if (request.userId) params.append('userId', request.userId.toString());
    if (request.action) params.append('action', request.action);
    if (request.entityName) params.append('entityName', request.entityName);
    if (request.storeId) params.append('storeId', request.storeId.toString());
    if (request.page) params.append('page', request.page.toString());
    if (request.pageSize) params.append('pageSize', request.pageSize.toString());

    const response = await api.get(`/audit/audit-logs?${params.toString()}`);
    return response.data.data;
  },

  getAuditLogById: async (id: number): Promise<AuditLog> => {
    const response = await api.get(`/audit/audit-logs/${id}`);
    return response.data.data;
  },

  // Security Logs
  getSecurityLogs: async (request: SecurityLogSearchRequest): Promise<PagedResult<SecurityLog>> => {
    const params = new URLSearchParams();
    if (request.startDate) params.append('startDate', request.startDate);
    if (request.endDate) params.append('endDate', request.endDate);
    if (request.userId) params.append('userId', request.userId.toString());
    if (request.eventType !== undefined) params.append('eventType', request.eventType.toString());
    if (request.severity !== undefined) params.append('severity', request.severity.toString());
    if (request.success !== undefined) params.append('success', request.success.toString());
    if (request.storeId) params.append('storeId', request.storeId.toString());
    if (request.page) params.append('page', request.page.toString());
    if (request.pageSize) params.append('pageSize', request.pageSize.toString());

    const response = await api.get(`/audit/security-logs?${params.toString()}`);
    return response.data.data;
  },

  getSecurityLogById: async (id: number): Promise<SecurityLog> => {
    const response = await api.get(`/audit/security-logs/${id}`);
    return response.data.data;
  },

  // Statistics
  getStatistics: async (days: number = 30): Promise<AuditStatistics> => {
    const response = await api.get(`/audit/statistics?days=${days}`);
    return response.data.data;
  },

  getRecentActivity: async (count: number = 20): Promise<AuditLog[]> => {
    const response = await api.get(`/audit/recent-activity?count=${count}`);
    return response.data.data;
  },

  getRecentSecurityEvents: async (count: number = 20): Promise<SecurityLog[]> => {
    const response = await api.get(`/audit/recent-security-events?count=${count}`);
    return response.data.data;
  },

  // Export
  exportAuditLogs: async (request: AuditLogSearchRequest): Promise<Blob> => {
    const response = await api.post('/audit/audit-logs/export', request, {
      responseType: 'blob'
    });
    return response.data;
  },

  exportSecurityLogs: async (request: SecurityLogSearchRequest): Promise<Blob> => {
    const response = await api.post('/audit/security-logs/export', request, {
      responseType: 'blob'
    });
    return response.data;
  },

  // Enums
  getSecurityEventTypes: async (): Promise<Record<number, string>> => {
    const response = await api.get('/audit/security-event-types');
    return response.data.data;
  },

  getSecuritySeverityLevels: async (): Promise<Record<number, string>> => {
    const response = await api.get('/audit/security-severity-levels');
    return response.data.data;
  }
};

export default auditService;
