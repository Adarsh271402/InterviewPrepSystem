import React from 'react';
import { useTheme } from '../context/ThemeContext';

export default function ThemeToggle({ compact = false }) {
  const { theme, toggleTheme } = useTheme();
  const isLight = theme === 'light';
  return (
    <button
      type="button"
      className={`theme-toggle ${compact ? 'compact' : ''}`}
      onClick={toggleTheme}
      aria-label={`Switch to ${isLight ? 'dark' : 'light'} theme`}
      title={`Switch to ${isLight ? 'dark' : 'light'} theme`}
    >
      <span className="theme-toggle-track" aria-hidden="true"><span className="theme-toggle-thumb">{isLight ? '☀' : '☾'}</span></span>
      {!compact && <span className="theme-toggle-label">{isLight ? 'Light' : 'Dark'}</span>}
    </button>
  );
}
