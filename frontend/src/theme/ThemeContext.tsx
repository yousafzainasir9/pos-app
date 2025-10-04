import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { ThemeConfig, ThemeName, availableThemes, defaultTheme } from './theme.config';

interface ThemeContextType {
  theme: ThemeConfig;
  themeName: ThemeName;
  setTheme: (themeName: ThemeName) => void;
  updateColors: (colors: Partial<ThemeConfig['colors']>) => void;
  updateCompanyName: (name: string) => void;
  updateLogo: (logoUrl: string) => void;
  resetTheme: () => void;
  availableThemeNames: ThemeName[];
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

const THEME_STORAGE_KEY = 'pos-theme';
const THEME_NAME_STORAGE_KEY = 'pos-theme-name';

export const ThemeProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [themeName, setThemeName] = useState<ThemeName>('default');
  const [theme, setThemeState] = useState<ThemeConfig>(defaultTheme);

  // Load theme from localStorage on mount
  useEffect(() => {
    const savedThemeName = localStorage.getItem(THEME_NAME_STORAGE_KEY) as ThemeName;
    const savedTheme = localStorage.getItem(THEME_STORAGE_KEY);

    if (savedTheme) {
      try {
        const parsedTheme = JSON.parse(savedTheme);
        setThemeState(parsedTheme);
        if (savedThemeName) {
          setThemeName(savedThemeName);
        }
      } catch (error) {
        console.error('Failed to parse saved theme:', error);
        setThemeState(defaultTheme);
      }
    } else if (savedThemeName && availableThemes[savedThemeName]) {
      setThemeName(savedThemeName);
      setThemeState(availableThemes[savedThemeName]);
    }
  }, []);

  // Apply theme to CSS variables whenever theme changes
  useEffect(() => {
    applyThemeToDOM(theme);
    localStorage.setItem(THEME_STORAGE_KEY, JSON.stringify(theme));
    localStorage.setItem(THEME_NAME_STORAGE_KEY, themeName);
  }, [theme, themeName]);

  const applyThemeToDOM = (themeConfig: ThemeConfig) => {
    const root = document.documentElement;
    const { colors } = themeConfig;

    // Apply all color variables
    Object.entries(colors).forEach(([key, value]) => {
      root.style.setProperty(`--color-${kebabCase(key)}`, value);
    });

    // Update Bootstrap CSS variables
    root.style.setProperty('--bs-primary', colors.primary);
    root.style.setProperty('--bs-primary-rgb', hexToRgb(colors.primary));
    root.style.setProperty('--bs-secondary', colors.secondary);
    root.style.setProperty('--bs-secondary-rgb', hexToRgb(colors.secondary));
    root.style.setProperty('--bs-success', colors.success);
    root.style.setProperty('--bs-success-rgb', hexToRgb(colors.success));
    root.style.setProperty('--bs-danger', colors.danger);
    root.style.setProperty('--bs-danger-rgb', hexToRgb(colors.danger));
    root.style.setProperty('--bs-warning', colors.warning);
    root.style.setProperty('--bs-warning-rgb', hexToRgb(colors.warning));
    root.style.setProperty('--bs-info', colors.info);
    root.style.setProperty('--bs-info-rgb', hexToRgb(colors.info));
    root.style.setProperty('--bs-body-bg', colors.background);
    root.style.setProperty('--bs-body-color', colors.textPrimary);
    root.style.setProperty('--bs-border-color', colors.border);

    // Update document title
    document.title = themeConfig.companyName;

    // Update favicon if provided
    if (themeConfig.favicon) {
      updateFavicon(themeConfig.favicon);
    }
  };

  const setTheme = (newThemeName: ThemeName) => {
    if (availableThemes[newThemeName]) {
      setThemeName(newThemeName);
      setThemeState(availableThemes[newThemeName]);
    }
  };

  const updateColors = (colors: Partial<ThemeConfig['colors']>) => {
    setThemeState(prev => ({
      ...prev,
      colors: {
        ...prev.colors,
        ...colors,
      },
    }));
  };

  const updateCompanyName = (name: string) => {
    setThemeState(prev => ({
      ...prev,
      companyName: name,
    }));
  };

  const updateLogo = (logoUrl: string) => {
    setThemeState(prev => ({
      ...prev,
      logoUrl,
    }));
  };

  const resetTheme = () => {
    setThemeName('default');
    setThemeState(defaultTheme);
    localStorage.removeItem(THEME_STORAGE_KEY);
    localStorage.removeItem(THEME_NAME_STORAGE_KEY);
  };

  const value: ThemeContextType = {
    theme,
    themeName,
    setTheme,
    updateColors,
    updateCompanyName,
    updateLogo,
    resetTheme,
    availableThemeNames: Object.keys(availableThemes) as ThemeName[],
  };

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
};

export const useTheme = () => {
  const context = useContext(ThemeContext);
  if (context === undefined) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
};

// Helper functions
const kebabCase = (str: string): string => {
  return str.replace(/([a-z0-9])([A-Z])/g, '$1-$2').toLowerCase();
};

const hexToRgb = (hex: string): string => {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
  return result
    ? `${parseInt(result[1], 16)}, ${parseInt(result[2], 16)}, ${parseInt(result[3], 16)}`
    : '0, 0, 0';
};

const updateFavicon = (url: string) => {
  let link: HTMLLinkElement | null = document.querySelector("link[rel~='icon']");
  if (!link) {
    link = document.createElement('link');
    link.rel = 'icon';
    document.getElementsByTagName('head')[0].appendChild(link);
  }
  link.href = url;
};
