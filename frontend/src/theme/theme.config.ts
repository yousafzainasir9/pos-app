// Theme Configuration
// This file contains all customizable theme variables for the Cookie Barrel POS System

export interface ThemeColors {
  // Primary Colors
  primary: string;
  primaryHover: string;
  primaryActive: string;
  primaryLight: string;
  primaryDark: string;

  // Secondary Colors
  secondary: string;
  secondaryHover: string;
  secondaryActive: string;
  secondaryLight: string;
  secondaryDark: string;

  // Accent Colors
  accent: string;
  accentHover: string;
  accentLight: string;

  // Status Colors
  success: string;
  successLight: string;
  warning: string;
  warningLight: string;
  danger: string;
  dangerLight: string;
  info: string;
  infoLight: string;

  // Neutral Colors
  background: string;
  backgroundSecondary: string;
  surface: string;
  surfaceHover: string;
  border: string;
  borderLight: string;

  // Text Colors
  textPrimary: string;
  textSecondary: string;
  textMuted: string;
  textLight: string;
  textDark: string;

  // Logo Colors
  logoBackground: string;
  logoText: string;
  logoAccent: string;
  logoIcon: string;
}

export interface ThemeConfig {
  colors: ThemeColors;
  companyName: string;
  logoUrl?: string;
  favicon?: string;
}

// Default Cookie Barrel Theme
export const defaultTheme: ThemeConfig = {
  companyName: 'Cookie Barrel POS',
  colors: {
    // Primary - Brand Blue
    primary: '#0d6efd',
    primaryHover: '#0b5ed7',
    primaryActive: '#0a58ca',
    primaryLight: '#cfe2ff',
    primaryDark: '#084298',

    // Secondary - Dark Gray
    secondary: '#6c757d',
    secondaryHover: '#5c636a',
    secondaryActive: '#565e64',
    secondaryLight: '#e2e3e5',
    secondaryDark: '#41464b',

    // Accent - Cookie Brown/Orange
    accent: '#d97706',
    accentHover: '#b45309',
    accentLight: '#fef3c7',

    // Status Colors
    success: '#198754',
    successLight: '#d1e7dd',
    warning: '#ffc107',
    warningLight: '#fff3cd',
    danger: '#dc3545',
    dangerLight: '#f8d7da',
    info: '#0dcaf0',
    infoLight: '#cff4fc',

    // Neutral Colors
    background: '#f8f9fa',
    backgroundSecondary: '#e9ecef',
    surface: '#ffffff',
    surfaceHover: '#f8f9fa',
    border: '#dee2e6',
    borderLight: '#e9ecef',

    // Text Colors
    textPrimary: '#212529',
    textSecondary: '#6c757d',
    textMuted: '#adb5bd',
    textLight: '#ffffff',
    textDark: '#000000',

    // Logo Colors
    logoBackground: '#2c3e50',
    logoText: '#ffffff',
    logoAccent: '#d97706',
    logoIcon: '#d97706',
  },
};

// Alternative Theme: Dark Mode
export const darkTheme: ThemeConfig = {
  companyName: 'Cookie Barrel POS',
  colors: {
    primary: '#3b82f6',
    primaryHover: '#2563eb',
    primaryActive: '#1d4ed8',
    primaryLight: '#1e3a8a',
    primaryDark: '#1e40af',

    secondary: '#94a3b8',
    secondaryHover: '#cbd5e1',
    secondaryActive: '#e2e8f0',
    secondaryLight: '#475569',
    secondaryDark: '#1e293b',

    accent: '#f59e0b',
    accentHover: '#d97706',
    accentLight: '#451a03',

    success: '#10b981',
    successLight: '#064e3b',
    warning: '#f59e0b',
    warningLight: '#451a03',
    danger: '#ef4444',
    dangerLight: '#7f1d1d',
    info: '#06b6d4',
    infoLight: '#164e63',

    background: '#0f172a',
    backgroundSecondary: '#1e293b',
    surface: '#1e293b',
    surfaceHover: '#334155',
    border: '#334155',
    borderLight: '#475569',

    textPrimary: '#f1f5f9',
    textSecondary: '#cbd5e1',
    textMuted: '#94a3b8',
    textLight: '#ffffff',
    textDark: '#0f172a',

    logoBackground: '#1e293b',
    logoText: '#ffffff',
    logoAccent: '#f59e0b',
    logoIcon: '#f59e0b',
  },
};

// Alternative Theme: Green/Eco Theme
export const ecoTheme: ThemeConfig = {
  companyName: 'Cookie Barrel POS',
  colors: {
    primary: '#059669',
    primaryHover: '#047857',
    primaryActive: '#065f46',
    primaryLight: '#d1fae5',
    primaryDark: '#064e3b',

    secondary: '#6b7280',
    secondaryHover: '#4b5563',
    secondaryActive: '#374151',
    secondaryLight: '#e5e7eb',
    secondaryDark: '#1f2937',

    accent: '#10b981',
    accentHover: '#059669',
    accentLight: '#d1fae5',

    success: '#22c55e',
    successLight: '#dcfce7',
    warning: '#f59e0b',
    warningLight: '#fef3c7',
    danger: '#ef4444',
    dangerLight: '#fee2e2',
    info: '#3b82f6',
    infoLight: '#dbeafe',

    background: '#f9fafb',
    backgroundSecondary: '#f3f4f6',
    surface: '#ffffff',
    surfaceHover: '#f9fafb',
    border: '#e5e7eb',
    borderLight: '#f3f4f6',

    textPrimary: '#111827',
    textSecondary: '#6b7280',
    textMuted: '#9ca3af',
    textLight: '#ffffff',
    textDark: '#000000',

    logoBackground: '#065f46',
    logoText: '#ffffff',
    logoAccent: '#10b981',
    logoIcon: '#10b981',
  },
};

// Alternative Theme: Purple/Premium Theme
export const premiumTheme: ThemeConfig = {
  companyName: 'Cookie Barrel POS',
  colors: {
    primary: '#7c3aed',
    primaryHover: '#6d28d9',
    primaryActive: '#5b21b6',
    primaryLight: '#ede9fe',
    primaryDark: '#4c1d95',

    secondary: '#64748b',
    secondaryHover: '#475569',
    secondaryActive: '#334155',
    secondaryLight: '#e2e8f0',
    secondaryDark: '#1e293b',

    accent: '#ec4899',
    accentHover: '#db2777',
    accentLight: '#fce7f3',

    success: '#10b981',
    successLight: '#d1fae5',
    warning: '#f59e0b',
    warningLight: '#fef3c7',
    danger: '#ef4444',
    dangerLight: '#fee2e2',
    info: '#06b6d4',
    infoLight: '#cffafe',

    background: '#f8fafc',
    backgroundSecondary: '#f1f5f9',
    surface: '#ffffff',
    surfaceHover: '#f8fafc',
    border: '#e2e8f0',
    borderLight: '#f1f5f9',

    textPrimary: '#0f172a',
    textSecondary: '#64748b',
    textMuted: '#94a3b8',
    textLight: '#ffffff',
    textDark: '#000000',

    logoBackground: '#5b21b6',
    logoText: '#ffffff',
    logoAccent: '#ec4899',
    logoIcon: '#ec4899',
  },
};

// Export all available themes
export const availableThemes = {
  default: defaultTheme,
  dark: darkTheme,
  eco: ecoTheme,
  premium: premiumTheme,
};

export type ThemeName = keyof typeof availableThemes;
