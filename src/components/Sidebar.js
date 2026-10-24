import React, { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import ThemeToggle from './ThemeToggle';

const navItems = [
  { path: '/dashboard', icon: '◫', label: 'Dashboard' },
  { path: '/categories', icon: '◇', label: 'Categories' },
  { path: '/interview', icon: '◎', label: 'Mock Interview' },
  { path: '/history', icon: '▤', label: 'Test History' },
  { path: '/ai-tools', icon: '✦', label: 'AI Tools' },
  { path: '/questions', icon: '?', label: 'Questions' },
];

export default function Sidebar() {
  const { user, logout, isAdmin } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [open, setOpen] = useState(false);
  const go = (path) => { navigate(path); setOpen(false); };

  useEffect(() => { setOpen(false); }, [location.pathname]);
  useEffect(() => {
    const onKey = (event) => event.key === 'Escape' && setOpen(false);
    document.addEventListener('keydown', onKey);
    document.body.classList.toggle('nav-open', open);
    return () => { document.removeEventListener('keydown', onKey); document.body.classList.remove('nav-open'); };
  }, [open]);

  const items = isAdmin ? [...navItems, { path: '/admin', icon: '⌘', label: 'Admin Panel', admin: true }] : navItems;

  return <>
    <header className="mobile-header">
      <button className="menu-button" onClick={() => setOpen(true)} aria-label="Open navigation" aria-expanded={open}><span /><span /><span /></button>
      <div className="mobile-brand"><b>Interview<span>AI</span></b><small>Prep workspace</small></div>
      <div className="mobile-header-actions"><ThemeToggle compact /><div className="mobile-avatar">{user?.name?.[0]?.toUpperCase() || 'U'}</div></div>
    </header>
    <button className={`sidebar-backdrop ${open ? 'open' : ''}`} onClick={() => setOpen(false)} aria-label="Close navigation" />
    <aside className={`sidebar ${open ? 'open' : ''}`} aria-label="Main navigation">
      <div className="sidebar-logo">
        <div className="brand-mark">IA</div>
        <div><h1>Interview<span>AI</span></h1><small>Prep workspace</small></div>
        <button className="drawer-close" onClick={() => setOpen(false)} aria-label="Close navigation">×</button>
      </div>
      <nav className="sidebar-nav">
        <div className="nav-label">Workspace</div>
        {items.map((item) => <React.Fragment key={item.path}>
          {item.admin && <><div className="nav-divider" /><div className="nav-label">Administration</div></>}
          <button className={`nav-item ${location.pathname === item.path ? 'active' : ''}`} onClick={() => go(item.path)}>
            <span className="icon">{item.icon}</span><span>{item.label}</span>
          </button>
        </React.Fragment>)}
      </nav>
      <div className="sidebar-footer"><div className="user-card">
        <div className="user-avatar">{user?.name?.[0]?.toUpperCase() || 'U'}</div>
        <div className="user-info"><div className="user-name">{user?.name}</div><div className="user-role">{user?.role}</div></div>
        <button onClick={logout} className="logout-button" title="Logout" aria-label="Logout">↗</button>
      </div></div>
    </aside>
  </>;
}
