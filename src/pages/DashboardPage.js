import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { RadarChart, Radar, PolarGrid, PolarAngleAxis, ResponsiveContainer, BarChart, Bar, XAxis, YAxis, Tooltip, CartesianGrid } from 'recharts';
import { dashboardAPI, aiAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';

export default function DashboardPage() {
  const { user } = useAuth();
  const { theme } = useTheme();
  const navigate = useNavigate();
  const [data, setData] = useState(null);
  const [suggestions, setSuggestions] = useState('');
  const [loadingSuggestions, setLoadingSuggestions] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    dashboardAPI.get().then(r => setData(r.data.data)).catch(console.error).finally(() => setLoading(false));
  }, []);

  const fetchSuggestions = async () => {
    setLoadingSuggestions(true);
    try { const response = await aiAPI.getSuggestions(); setSuggestions(response.data.data); }
    catch { setSuggestions('Complete at least one mock test to get suggestions.'); }
    finally { setLoadingSuggestions(false); }
  };

  if (loading) return <div className="loading-center"><div className="spinner" /><span>Loading dashboard...</span></div>;

  const radarData = data?.categoryPerformance?.map(c => ({ subject: c.categoryName, score: c.averageScore })) || [];
  const barData = data?.recentTests?.map(t => ({ name: t.categoryName, score: t.percentage })) || [];
  const performance = (value) => value >= 80 ? 'Excellent' : value >= 60 ? 'Good' : value >= 40 ? 'Average' : 'Needs work';
  const scoreClass = (value) => value >= 80 ? 'score-high' : value >= 50 ? 'score-medium' : 'score-low';
  const isLight = theme === 'light';
  const chartMuted = isLight ? '#656D84' : '#A5AEC5';
  const chartGrid = isLight ? 'rgba(73,82,120,.13)' : 'rgba(190,202,255,.09)';
  const tooltipStyle = { background: isLight ? '#FFFFFF' : '#171D35', border: `1px solid ${isLight ? 'rgba(73,82,120,.13)' : 'rgba(190,202,255,.14)'}`, borderRadius: 10, color: isLight ? '#17192D' : '#F5F7FF', boxShadow: '0 14px 34px rgba(12,18,38,.16)' };

  return <div className="page dashboard-page">
    <div className="dashboard-heading">
      <div className="page-header"><div className="eyebrow">Performance workspace</div><h2>Welcome back, {user?.name?.split(' ')[0]}</h2><p>Track your progress and prepare for your next interview.</p></div>
      <div className="dashboard-promo">
        <div className="dashboard-promo-copy"><span>Focused practice</span><strong>Better preparation.<br />Bigger opportunities.</strong><button className="btn btn-primary btn-sm" onClick={() => navigate('/interview')}>Start Interview <b>→</b></button></div>
        <img src="/assets/interview-ai-hero.png" alt="" aria-hidden="true" />
      </div>
    </div>

    <div className="stats-grid">
      <div className="stat-card blue"><div className="stat-icon">◎</div><div className="stat-value">{data?.totalTests ?? 0}</div><div className="stat-label">Tests completed</div></div>
      <div className="stat-card green"><div className="stat-icon">↗</div><div className="stat-value">{data?.averageScore ?? 0}%</div><div className="stat-label">Average score</div></div>
      <div className="stat-card yellow"><div className="stat-icon">◇</div><div className="stat-value">{data?.bestScore ?? 0}%</div><div className="stat-label">Best score</div></div>
      <div className="stat-card purple"><div className="stat-icon">✦</div><div className="stat-value stat-value-text">{data?.favoriteCategory || '—'}</div><div className="stat-label">Top category</div></div>
    </div>

    {!data?.totalTests ? <div className="card"><div className="empty-state"><div className="empty-icon">◎</div><h3>Your practice journey starts here</h3><p>Complete a mock interview to unlock performance analytics and AI recommendations.</p><button className="btn btn-primary" onClick={() => navigate('/interview')}>Start Mock Interview</button></div></div> : <>
      <div className="analytics-grid">
        <div className="card chart-card">
          <div className="card-heading-row"><div><div className="card-title">Recent Test Scores</div><div className="card-subtitle">Latest performance trend</div></div><span className="panel-mark">01</span></div>
          <ResponsiveContainer width="100%" height={190}><BarChart data={barData} margin={{ top: 5, right: 0, left: -20, bottom: 0 }}><CartesianGrid strokeDasharray="3 3" vertical={false} stroke={chartGrid}/><XAxis dataKey="name" tick={{ fill:chartMuted,fontSize:10 }} axisLine={false} tickLine={false}/><YAxis domain={[0,100]} tick={{ fill:chartMuted,fontSize:10 }} axisLine={false} tickLine={false}/><Tooltip contentStyle={tooltipStyle} formatter={v => [`${v}%`,'Score']}/><Bar dataKey="score" fill={isLight ? '#318DE5' : '#68B8FF'} radius={[6,6,2,2]} maxBarSize={34}/></BarChart></ResponsiveContainer>
        </div>
        <div className="card chart-card">
          <div className="card-heading-row"><div><div className="card-title">Category Performance</div><div className="card-subtitle">Average by skill area</div></div><span className="panel-mark">02</span></div>
          <ResponsiveContainer width="100%" height={190}><RadarChart data={radarData}><PolarGrid stroke={isLight ? 'rgba(73,82,120,.16)' : 'rgba(190,202,255,.12)'}/><PolarAngleAxis dataKey="subject" tick={{ fill:chartMuted,fontSize:10 }}/><Radar dataKey="score" stroke={isLight ? '#7956E8' : '#9B7CFF'} fill={isLight ? '#7956E8' : '#9B7CFF'} fillOpacity={.22}/><Tooltip contentStyle={tooltipStyle}/></RadarChart></ResponsiveContainer>
        </div>
        <div className="card ai-panel compact-ai-panel">
          <div className="card-heading-row"><div><div className="card-title">AI Recommendations</div><div className="card-subtitle">Personalized next steps</div></div><span className="ai-signal">✦</span></div>
          {suggestions ? <div className="suggestion-copy">{suggestions}</div> : <div className="recommendation-empty"><div className="recommendation-orb">AI</div><p>Analyze your recent performance to receive targeted improvement suggestions.</p><button className="btn btn-primary btn-sm" onClick={fetchSuggestions} disabled={loadingSuggestions}>{loadingSuggestions ? 'Generating...' : 'Generate insights'}</button></div>}
        </div>
      </div>

      {data?.recentTests?.length > 0 && <div className="card recent-table-card">
        <div className="card-heading-row"><div><div className="card-title">Recent Interviews</div><div className="card-subtitle">Your latest completed sessions</div></div><button className="btn btn-secondary btn-sm" onClick={() => navigate('/history')}>View all</button></div>
        <div className="table-wrap"><table><thead><tr><th>Category</th><th>Score</th><th>Questions</th><th>Date</th><th>Performance</th><th></th></tr></thead><tbody>{data.recentTests.map(t => <tr key={t.mockTestId}><td><strong>{t.categoryName}</strong></td><td><span className={`compact-score ${scoreClass(t.percentage)}`}>{Math.round(t.percentage)}%</span></td><td>{t.totalQuestions}</td><td>{new Date(t.date).toLocaleDateString()}</td><td><span className={`badge ${scoreClass(t.percentage)}`}>{performance(t.percentage)}</span></td><td><button className="btn btn-secondary btn-sm" onClick={() => navigate(`/result/${t.mockTestId}`)}>View</button></td></tr>)}</tbody></table></div>
      </div>}
    </>}
  </div>;
}
