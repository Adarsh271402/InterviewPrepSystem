import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import './index.css';

import { AuthProvider, useAuth } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import Sidebar from './components/Sidebar';
import ThemeToggle from './components/ThemeToggle';

import { LoginPage, RegisterPage }                                          from './pages/AuthPages';
import DashboardPage                                                         from './pages/DashboardPage';
import InterviewPage                                                         from './pages/InterviewPage';
import { ResultPage, HistoryPage, AIToolsPage, QuestionsPage, CategoriesPage, AdminPage } from './pages/OtherPages';

// ── Protected Route ────────────────────────────────────────────
function ProtectedRoute({ children }) {
  const { user, loading } = useAuth();
  if (loading) return <div className="loading-center"><div className="spinner" /><span>Loading...</span></div>;
  if (!user)   return <Navigate to="/login" replace />;
  return (
    <div className="app-layout">
      <Sidebar />
      <main className="main-content">
        <header className="desktop-header">
          <div className="header-context"><span className="header-signal" /><span><b>InterviewAI</b><small>Interview preparation workspace</small></span></div>
          <div className="header-actions">
            <ThemeToggle />
            <div className="header-profile" aria-label={`Signed in as ${user?.name || 'User'}`}>
              <span>{user?.name?.[0]?.toUpperCase() || 'U'}</span>
              <div><b>{user?.name}</b><small>{user?.role}</small></div>
            </div>
          </div>
        </header>
        <div className="page-shell">{children}</div>
      </main>
    </div>
  );
}

// ── Public Route (redirect to dashboard if logged in) ──────────
function PublicRoute({ children }) {
  const { user, loading } = useAuth();
  if (loading) return <div className="loading-center"><div className="spinner" /></div>;
  if (user)    return <Navigate to="/dashboard" replace />;
  return <><div className="auth-theme-control"><ThemeToggle compact /></div>{children}</>;
}

export default function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <BrowserRouter>
        <Routes>
          {/* Public */}
          <Route path="/login"    element={<PublicRoute><LoginPage /></PublicRoute>} />
          <Route path="/register" element={<PublicRoute><RegisterPage /></PublicRoute>} />

          {/* Protected */}
          <Route path="/dashboard"  element={<ProtectedRoute><DashboardPage /></ProtectedRoute>} />
          <Route path="/interview"  element={<ProtectedRoute><InterviewPage /></ProtectedRoute>} />
          <Route path="/result/:id" element={<ProtectedRoute><ResultPage /></ProtectedRoute>} />
          <Route path="/history"    element={<ProtectedRoute><HistoryPage /></ProtectedRoute>} />
          <Route path="/ai-tools"   element={<ProtectedRoute><AIToolsPage /></ProtectedRoute>} />
          <Route path="/questions"  element={<ProtectedRoute><QuestionsPage /></ProtectedRoute>} />
          <Route path="/categories" element={<ProtectedRoute><CategoriesPage /></ProtectedRoute>} />
          <Route path="/admin"      element={<ProtectedRoute><AdminPage /></ProtectedRoute>} />

          {/* Default */}
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  );
}
