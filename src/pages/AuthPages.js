import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

function AuthShowcase() {
  return (
    <section className="auth-showcase">
      <div className="auth-brand"><span>IA</span> InterviewAI</div>
      <div className="auth-showcase-copy">
        <div className="eyebrow">Interview intelligence</div>
        <h2><span>Practice with purpose.</span><span>Interview with confidence.</span></h2>
        <p>AI-powered mock interviews, instant feedback, and clear performance insights in one focused workspace.</p>
        <div className="auth-features">
          <div><span>01</span><b>Real-time AI feedback</b><small>Turn every answer into a useful learning moment.</small></div>
          <div><span>02</span><b>Role-ready practice</b><small>Prepare across technical and behavioral categories.</small></div>
          <div><span>03</span><b>Measurable progress</b><small>See trends, strengths, and the skills to improve next.</small></div>
        </div>
      </div>
      <img className="auth-hero-image" src="/assets/interview-ai-hero.png" alt="" aria-hidden="true" />
    </section>
  );
}

// ── LOGIN ──────────────────────────────────────────────────────
export function LoginPage() {
  const { login }   = useAuth();
  const navigate    = useNavigate();
  const [form, setForm]   = useState({ email: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handle = (e) => setForm(f => ({ ...f, [e.target.name]: e.target.value }));

  const submit = async (e) => {
    e.preventDefault();
    setError(''); setLoading(true);
    try {
      await login(form.email, form.password);
      navigate('/dashboard');
    } catch (err) {
      setError(err.response?.data?.message || 'Invalid email or password.');
    } finally { setLoading(false); }
  };

  return (
    <div className="auth-page">
      <AuthShowcase />
      <section className="auth-form-panel">
      <div className="auth-card">
        <div className="auth-logo">
          <h1>InterviewAI</h1>
          <p>Sign in to continue your preparation</p>
        </div>

        {error && <div className="alert alert-error">{error}</div>}

        <form onSubmit={submit}>
          <div className="form-group">
            <label className="form-label">Email Address</label>
            <input name="email" type="email" className="form-control"
              placeholder="you@example.com" value={form.email} onChange={handle} required />
          </div>
          <div className="form-group">
            <label className="form-label">Password</label>
            <input name="password" type="password" className="form-control"
              placeholder="••••••••" value={form.password} onChange={handle} required />
          </div>
          <button type="submit" className="btn btn-primary btn-lg" style={{ width: '100%', marginTop: 8 }} disabled={loading}>
            {loading ? '⏳ Signing in...' : '→ Sign In'}
          </button>
        </form>

        <p style={{ textAlign: 'center', marginTop: 24, fontSize: 14, color: 'var(--text2)' }}>
          No account?{' '}
          <Link to="/register" style={{ color: 'var(--accent2)', textDecoration: 'none', fontWeight: 600 }}>
            Register here
          </Link>
        </p>

      </div>
      </section>
    </div>
  );
}

// ── REGISTER ───────────────────────────────────────────────────
export function RegisterPage() {
  const { register } = useAuth();
  const navigate     = useNavigate();
  const [form, setForm]   = useState({ name: '', email: '', password: '', confirm: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handle = (e) => setForm(f => ({ ...f, [e.target.name]: e.target.value }));

  const submit = async (e) => {
    e.preventDefault();
    setError('');
    if (form.password !== form.confirm) { setError('Passwords do not match.'); return; }
    if (form.password.length < 6)       { setError('Password must be at least 6 characters.'); return; }
    setLoading(true);
    try {
      await register(form.name, form.email, form.password);
      navigate('/dashboard');
    } catch (err) {
      setError(err.response?.data?.message || 'Registration failed. Try again.');
    } finally { setLoading(false); }
  };

  return (
    <div className="auth-page">
      <AuthShowcase />
      <section className="auth-form-panel">
      <div className="auth-card">
        <div className="auth-logo">
          <h1>InterviewAI</h1>
          <p>Create your free account</p>
        </div>

        {error && <div className="alert alert-error">{error}</div>}

        <form onSubmit={submit}>
          <div className="form-group">
            <label className="form-label">Full Name</label>
            <input name="name" className="form-control"
              placeholder="John Doe" value={form.name} onChange={handle} required />
          </div>
          <div className="form-group">
            <label className="form-label">Email Address</label>
            <input name="email" type="email" className="form-control"
              placeholder="you@example.com" value={form.email} onChange={handle} required />
          </div>
          <div className="form-group">
            <label className="form-label">Password</label>
            <input name="password" type="password" className="form-control"
              placeholder="Min 6 characters" value={form.password} onChange={handle} required />
          </div>
          <div className="form-group">
            <label className="form-label">Confirm Password</label>
            <input name="confirm" type="password" className="form-control"
              placeholder="Re-enter password" value={form.confirm} onChange={handle} required />
          </div>
          <button type="submit" className="btn btn-primary btn-lg" style={{ width: '100%', marginTop: 8 }} disabled={loading}>
            {loading ? '⏳ Creating account...' : '→ Create Account'}
          </button>
        </form>

        <p style={{ textAlign: 'center', marginTop: 24, fontSize: 14, color: 'var(--text2)' }}>
          Already have an account?{' '}
          <Link to="/login" style={{ color: 'var(--accent2)', textDecoration: 'none', fontWeight: 600 }}>
            Sign in
          </Link>
        </p>
      </div>
      </section>
    </div>
  );
}
