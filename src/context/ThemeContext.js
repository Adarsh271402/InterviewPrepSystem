import React, { createContext, useContext, useEffect, useState } from 'react';

const THEME_KEY = 'interviewai-theme';
const ThemeContext = createContext(null);

const applyTheme = (theme) => {
  document.documentElement.dataset.theme = theme;
  document.documentElement.style.colorScheme = theme === 'light' ? 'light' : 'dark';
  const meta = document.querySelector('meta[name="theme-color"]');
  if (meta) meta.setAttribute('content', theme === 'light' ? '#F5F6FC' : '#070914');
};

export function ThemeProvider({ children }) {
  const [theme, setTheme] = useState(() => document.documentElement.dataset.theme === 'light' ? 'light' : 'dark');

  useEffect(() => {
    applyTheme(theme);
    try { localStorage.setItem(THEME_KEY, theme); } catch {}
  }, [theme]);

  const toggleTheme = () => setTheme(current => current === 'dark' ? 'light' : 'dark');

  return <ThemeContext.Provider value={{ theme, toggleTheme }}>{children}</ThemeContext.Provider>;
}

export const useTheme = () => useContext(ThemeContext);
