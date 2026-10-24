import React, { useEffect, useState } from 'react';
import { createPortal } from 'react-dom';
import { useParams, useNavigate } from 'react-router-dom';
import { mockTestAPI, aiAPI, categoryAPI, questionAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';

const answerOptionText = (answer, letter) => answer?.[`option${letter}`] || '';
const formatMCQAnswer = (answer, letter) => letter ? `${letter}. ${answerOptionText(answer, letter)}` : '(No answer)';

// ── RESULT PAGE ────────────────────────────────────────────────
export function ResultPage() {
  const { id }   = useParams();
  const navigate = useNavigate();
  const [result, setResult]   = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    mockTestAPI.getResult(id)
      .then(r => setResult(r.data.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="loading-center"><div className="spinner" /><span>Loading results...</span></div>;
  if (!result)  return <div className="alert alert-error">Result not found.</div>;

  const pct   = result.percentage;
  const cls   = pct >= 80 ? 'score-excellent' : pct >= 60 ? 'score-good' : pct >= 40 ? 'score-average' : 'score-poor';
  const emoji = pct >= 80 ? '🌟' : pct >= 60 ? '👍' : pct >= 40 ? '📖' : '💪';

  return (
    <div className="page result-page">
      <button className="back-link" onClick={() => navigate('/history')}>← Back to History</button>
      <div className="page-header">
        <h2>Test Results</h2>
        <p>{result.categoryName}</p>
      </div>

      <div className="result-hero">
        <div className="result-score-ring" style={{ '--score': `${Math.min(100, Math.max(0, pct)) * 3.6}deg` }}>
          <div><span className={`big-score ${cls}`}>{Math.round(pct)}%</span><small>Overall score</small></div>
        </div>
        <div className="result-summary-content">
          <div className="eyebrow">Interview complete</div>
          <div className="result-level">{emoji} {result.performanceLevel}</div>
          <p>Your answers have been evaluated. Review the question-level feedback below to focus your next practice session.</p>
          <div className="result-summary-meta">
            <span><small>Total score</small><strong>{result.score} / {result.totalQuestions * 10}</strong></span>
            <span><small>Questions</small><strong>{result.totalQuestions}</strong></span>
          </div>
          <div className="result-actions">
            <button className="btn btn-primary" onClick={() => navigate('/interview')}>Try Again</button>
            <button className="btn btn-secondary" onClick={() => navigate('/history')}>All Results</button>
            <button className="btn btn-secondary" onClick={() => navigate('/dashboard')}>Dashboard</button>
          </div>
        </div>
      </div>

      <div className="card" style={{ marginBottom: 16 }}>
        <div className="card-title">Answer Review</div>
        <div className="card-subtitle">Detailed AI feedback for each answer</div>
        <div className="answer-review">
          {result.answers.map((a, i) => (
            <div key={i} className="answer-item">
              <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 12 }}>
                <div className={`score-ring ${a.aiScore >= 8 ? 'score-high' : a.aiScore >= 5 ? 'score-medium' : 'score-low'}`}>
                  {a.aiScore}/10
                </div>
                <div className="answer-item-question">Q{i + 1}. {a.questionText}</div>
              </div>
              {a.isMCQ ? (
                <div className="result-mcq-review">
                  <div className={`mcq-result-status ${a.isCorrect ? 'correct' : 'incorrect'}`}>{a.isCorrect ? '✓ Correct' : '× Incorrect'}</div>
                  <div className="mcq-result-answers">
                    <div><small>Your answer</small><strong>{formatMCQAnswer(a, a.userAnswer)}</strong></div>
                    <div><small>Correct answer</small><strong>{formatMCQAnswer(a, a.correctAnswer)}</strong></div>
                  </div>
                  <div className="feedback-text">{a.aiFeedback || a.aiSuggestions}</div>
                </div>
              ) : (
                <>
                  <div className="answer-item-user"><strong className="answer-review-label">Your Answer</strong><div>{a.userAnswer || '(No answer)'}</div></div>
                  <div className="answer-item-meta">
                    <div className="meta-block"><div className="feedback-label">Feedback</div><div className="feedback-text">{a.aiFeedback}</div></div>
                    <div className="meta-block"><div className="feedback-label">Suggestions</div><div className="feedback-text">{a.aiSuggestions}</div></div>
                  </div>
                </>
              )}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

// ── HISTORY PAGE ───────────────────────────────────────────────
export function HistoryPage() {
  const navigate = useNavigate();
  const [history, setHistory] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    mockTestAPI.getHistory()
      .then(r => setHistory(r.data.data || []))
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="loading-center"><div className="spinner" /><span>Loading history...</span></div>;

  return (
    <div className="page ai-tools-page">
      <div className="page-header">
        <h2>📋 Test History</h2>
        <p>All your past mock interview sessions</p>
      </div>

      {history.length === 0 ? (
        <div className="card">
          <div className="empty-state">
            <div className="empty-icon">📋</div>
            <h3>No tests yet</h3>
            <p>Complete a mock interview to see your history.</p>
            <button className="btn btn-primary" style={{ marginTop: 20 }} onClick={() => navigate('/interview')}>
              Start Mock Interview
            </button>
          </div>
        </div>
      ) : (
        <div className="card">
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>Category</th>
                  <th>Score</th>
                  <th>Questions</th>
                  <th>Performance</th>
                  <th>Date</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {history.map((t, i) => {
                  const pct = Math.round(t.percentage);
                  return (
                    <tr key={t.mockTestId}>
                      <td style={{ color: 'var(--text3)', fontSize: 13 }}>{i + 1}</td>
                      <td style={{ fontWeight: 600 }}>{t.categoryName}</td>
                      <td>
                        <span className={`score-ring ${pct >= 80 ? 'score-high' : pct >= 50 ? 'score-medium' : 'score-low'}`}
                          style={{ width: 48, height: 48, fontSize: 14 }}>
                          {pct}%
                        </span>
                      </td>
                      <td>{t.totalQuestions}</td>
                      <td>
                        <span className={`badge ${pct >= 80 ? 'badge-easy' : pct >= 60 ? 'badge-blue' : pct >= 40 ? 'badge-medium' : 'badge-hard'}`}>
                          {pct >= 80 ? 'Excellent' : pct >= 60 ? 'Good' : pct >= 40 ? 'Average' : 'Needs Work'}
                        </span>
                      </td>
                      <td style={{ color: 'var(--text3)', fontSize: 13 }}>{new Date(t.date).toLocaleDateString()}</td>
                      <td>
                        <button className="btn btn-secondary btn-sm" onClick={() => navigate(`/result/${t.mockTestId}`)}>
                          View
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}

// ── AI TOOLS PAGE ──────────────────────────────────────────────
export function AIToolsPage() {
  const [categories, setCategories] = useState([]);
  const [tab, setTab]               = useState('evaluate');

  // Evaluate state
  const [evalForm, setEvalForm] = useState({ question: '', userAnswer: '', difficultyLevel: 'Medium' });
  const [evalResult, setEvalResult] = useState(null);
  const [evalLoading, setEvalLoading] = useState(false);

  // Generate state
  const [genForm, setGenForm] = useState({ categoryName: '', difficultyLevel: 'Medium', numberOfQuestions: 5, targetAudience: 'freshers' });
  const [generationMode, setGenerationMode] = useState('Written');
  const [genResult, setGenResult] = useState(null);
  const [genLoading, setGenLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    categoryAPI.getAll().then(r => setCategories(r.data.data || []));
  }, []);

  const evaluate = async () => {
    if (!evalForm.question.trim() || !evalForm.userAnswer.trim()) { setError('Enter both question and answer.'); return; }
    setError(''); setEvalLoading(true);
    try {
      const r = await aiAPI.evaluate(evalForm);
      setEvalResult(r.data.data);
    } catch (e) { setError(e.response?.data?.message || 'AI evaluation failed.'); }
    finally { setEvalLoading(false); }
  };

  const generate = async () => {
    if (!genForm.categoryName) { setError('Select a category.'); return; }
    setError(''); setGenLoading(true);
    try {
      const r = generationMode === 'MCQ' ? await aiAPI.generateMCQs(genForm) : await aiAPI.generateQuestions(genForm);
      setGenResult(r.data.data);
    } catch (e) { setError(e.response?.data?.message || 'AI generation failed.'); }
    finally { setGenLoading(false); }
  };

  return (
    <div>
      <div className="page-header">
        <h2>🤖 AI Tools</h2>
        <p>Evaluate answers and generate questions with AI</p>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      <div className="tabs">
        <button className={`tab ${tab === 'evaluate' ? 'active' : ''}`} onClick={() => { setTab('evaluate'); setError(''); }}>
          📝 Evaluate Answer
        </button>
        <button className={`tab ${tab === 'generate' ? 'active' : ''}`} onClick={() => { setTab('generate'); setError(''); }}>
          ✨ Generate Questions
        </button>
      </div>

      {tab === 'evaluate' && (
        <div className="ai-workspace-grid">
          <div className="card">
            <div className="card-title">Answer Evaluation</div>
            <div className="card-subtitle">Get AI score + feedback on any answer</div>
            <div className="form-group">
              <label className="form-label">Interview Question</label>
              <textarea className="form-control" style={{ minHeight: 80 }}
                placeholder="e.g. What is Dependency Injection?"
                value={evalForm.question}
                onChange={e => setEvalForm(f => ({ ...f, question: e.target.value }))} />
            </div>
            <div className="form-group">
              <label className="form-label">Your Answer</label>
              <textarea className="form-control" style={{ minHeight: 120 }}
                placeholder="Type your answer here..."
                value={evalForm.userAnswer}
                onChange={e => setEvalForm(f => ({ ...f, userAnswer: e.target.value }))} />
            </div>
            <div className="form-group">
              <label className="form-label">Difficulty</label>
              <select className="form-control" value={evalForm.difficultyLevel}
                onChange={e => setEvalForm(f => ({ ...f, difficultyLevel: e.target.value }))}>
                <option>Easy</option><option>Medium</option><option>Hard</option>
              </select>
            </div>
            <button className="btn btn-primary" onClick={evaluate} disabled={evalLoading}>
              {evalLoading ? '⏳ Evaluating...' : '🤖 Evaluate with AI'}
            </button>
          </div>

          <div>
            {evalResult ? (
              <div className="ai-feedback-card">
                <div style={{ display: 'flex', alignItems: 'center', gap: 16, marginBottom: 20, paddingBottom: 16, borderBottom: '1px solid var(--border)' }}>
                  <div className={`score-ring ${evalResult.score >= 8 ? 'score-high' : evalResult.score >= 5 ? 'score-medium' : 'score-low'}`}
                    style={{ width: 80, height: 80, fontSize: 24 }}>
                    {evalResult.score}/10
                  </div>
                  <div>
                    <div style={{ fontFamily: 'var(--font-head)', fontSize: 22, fontWeight: 800 }}>AI Score</div>
                    <div style={{ color: 'var(--text2)', fontSize: 14 }}>
                      {evalResult.score >= 8 ? '🌟 Excellent answer!' : evalResult.score >= 6 ? '👍 Good answer' : evalResult.score >= 4 ? '📖 Needs improvement' : '💪 Keep practicing'}
                    </div>
                  </div>
                </div>
                {[
                  { label: '📝 Feedback',          value: evalResult.feedback },
                  { label: '💡 Suggestions',        value: evalResult.suggestions },
                  { label: '🔤 Grammar',            value: evalResult.grammarFeedback },
                  { label: '⚙️ Technical Accuracy', value: evalResult.technicalAccuracy },
                ].map(({ label, value }) => (
                  <div key={label} className="feedback-section">
                    <div className="feedback-label">{label}</div>
                    <div className="feedback-text">{value}</div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="card ai-empty-state">
                <div style={{ fontSize: 56, marginBottom: 16 }}>🤖</div>
                <div style={{ fontFamily: 'var(--font-head)', fontSize: 18, fontWeight: 700, marginBottom: 8 }}>AI Evaluator Ready</div>
                <div style={{ fontSize: 13, color: 'var(--text2)' }}>Submit a question and answer to get instant AI evaluation</div>
              </div>
            )}
          </div>
        </div>
      )}

      {tab === 'generate' && (
        <div className="ai-workspace-grid">
          <div className="card">
            <div className="card-title">AI Question Generator</div>
            <div className="card-subtitle">Generate interview questions dynamically</div>
            <div className="form-group">
              <label className="form-label">Generation mode</label>
              <div className="generation-mode-selector" role="group" aria-label="Generation mode">
                {['Written', 'MCQ'].map(mode => <button type="button" key={mode} className={generationMode === mode ? 'active' : ''} onClick={() => { setGenerationMode(mode); setGenResult(null); }} aria-pressed={generationMode === mode}>{mode}</button>)}
              </div>
            </div>
            <div className="form-group">
              <label className="form-label">Topic / Category</label>
              <select className="form-control" value={genForm.categoryName}
                onChange={e => setGenForm(f => ({ ...f, categoryName: e.target.value }))}>
                <option value="">Select topic...</option>
                {categories.map(c => <option key={c.id} value={c.name}>{c.name}</option>)}
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">Difficulty</label>
              <select className="form-control" value={genForm.difficultyLevel}
                onChange={e => setGenForm(f => ({ ...f, difficultyLevel: e.target.value }))}>
                <option>Easy</option><option>Medium</option><option>Hard</option>
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">Target Audience</label>
              <select className="form-control" value={genForm.targetAudience}
                onChange={e => setGenForm(f => ({ ...f, targetAudience: e.target.value }))}>
                <option value="freshers">Freshers</option>
                <option value="1-3 years experience">1-3 Years Experience</option>
                <option value="senior developers">Senior Developers</option>
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">Number of Questions: <strong>{genForm.numberOfQuestions}</strong></label>
              <input type="range" min={3} max={10} value={genForm.numberOfQuestions}
                onChange={e => setGenForm(f => ({ ...f, numberOfQuestions: +e.target.value }))}
                style={{ width: '100%', accentColor: 'var(--accent)' }} />
            </div>
            <button className="btn btn-primary" onClick={generate} disabled={genLoading}>
              {genLoading ? '⏳ Generating...' : `✨ Generate ${generationMode === 'MCQ' ? 'MCQs' : 'Questions'}`}
            </button>
          </div>

          <div>
            {genResult?.questions?.length > 0 ? (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
                {genResult.questions.map((q, i) => (
                  <div key={i} className="card" style={{ padding: 18 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 10 }}>
                      <span style={{ fontSize: 12, color: 'var(--text3)', fontWeight: 700 }}>Q{i + 1}</span>
                      <span className={`badge badge-${q.difficultyLevel?.toLowerCase()}`}>{q.difficultyLevel}</span>
                    </div>
                    <div style={{ fontWeight: 600, marginBottom: 10, lineHeight: 1.5 }}>{q.questionText}</div>
                    {generationMode === 'MCQ' ? (
                      <div className="generated-mcq">
                        <div className="generated-options">{['A', 'B', 'C', 'D'].map(letter => <div key={letter} className={q.correctAnswer === letter ? 'correct' : ''}><span>{letter}</span>{q[`option${letter}`]}{q.correctAnswer === letter && <small>Correct</small>}</div>)}</div>
                        {q.explanation && <div className="generated-explanation"><strong>Explanation</strong>{q.explanation}</div>}
                      </div>
                    ) : (
                      <div className="generated-answer"><div>Suggested Answer</div>{q.suggestedAnswer}</div>
                    )}
                  </div>
                ))}
              </div>
            ) : (
              <div className="card ai-empty-state">
                <div style={{ fontSize: 56, marginBottom: 16 }}>✨</div>
                <div style={{ fontFamily: 'var(--font-head)', fontSize: 18, fontWeight: 700, marginBottom: 8 }}>Question Generator</div>
                <div style={{ fontSize: 13, color: 'var(--text2)' }}>Configure settings and generate {generationMode === 'MCQ' ? 'multiple-choice questions' : 'written interview questions'}.</div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

// ── QUESTIONS PAGE ─────────────────────────────────────────────
export function QuestionsPage() {
  const { isAdmin } = useAuth();
  const [questions,   setQuestions]  = useState([]);
  const [categories,  setCategories] = useState([]);
  const [filterCat,   setFilterCat]  = useState('');
  const [filterDiff,  setFilterDiff] = useState('');
  const [search,      setSearch]     = useState('');
  const [loading,     setLoading]    = useState(true);
  const [modal,       setModal]      = useState(false);
  const [editQ,       setEditQ]      = useState(null);
  const emptyQuestion = { categoryId: '', questionText: '', difficultyLevel: 'Medium', correctAnswer: '', isMCQ: false, optionA: '', optionB: '', optionC: '', optionD: '' };
  const [form, setForm]              = useState(emptyQuestion);
  const [saving, setSaving]          = useState(false);
  const [error, setError]            = useState('');

  const load = () => {
    setLoading(true);
    const params = {};
    if (filterCat)  params.categoryId = filterCat;
    if (filterDiff) params.difficulty  = filterDiff;
    questionAPI.getAll(params)
      .then(r => setQuestions(r.data.data || []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { categoryAPI.getAll().then(r => setCategories(r.data.data || [])); }, []);
  useEffect(() => { load(); }, [filterCat, filterDiff]);

  const openAdd  = () => { setEditQ(null); setError(''); setForm(emptyQuestion); setModal(true); };
  const openEdit = (q) => { setEditQ(q); setError(''); setForm({ categoryId: q.categoryId, questionText: q.questionText, difficultyLevel: q.difficultyLevel, correctAnswer: '', isMCQ: Boolean(q.isMCQ), optionA: q.optionA || '', optionB: q.optionB || '', optionC: q.optionC || '', optionD: q.optionD || '' }); setModal(true); };

  const save = async () => {
    if (!form.questionText.trim() || (!editQ && !form.categoryId)) { setError('Category and question are required.'); return; }
    if (form.isMCQ && (!form.optionA.trim() || !form.optionB.trim() || !form.optionC.trim() || !form.optionD.trim() || !form.correctAnswer)) { setError('All four options and the correct answer are required for an MCQ.'); return; }
    if (!form.isMCQ && !form.correctAnswer.trim()) { setError('A reference answer is required for a written question.'); return; }
    setError(''); setSaving(true);
    try {
      const payload = { questionText: form.questionText.trim(), difficultyLevel: form.difficultyLevel, correctAnswer: form.correctAnswer, isMCQ: form.isMCQ, optionA: form.isMCQ ? form.optionA.trim() : '', optionB: form.isMCQ ? form.optionB.trim() : '', optionC: form.isMCQ ? form.optionC.trim() : '', optionD: form.isMCQ ? form.optionD.trim() : '' };
      if (editQ) await questionAPI.update(editQ.id, payload);
      else       await questionAPI.create({ ...payload, categoryId: +form.categoryId });
      setModal(false); load();
    } catch (e) { setError(e.response?.data?.message || 'Save failed.'); }
    finally { setSaving(false); }
  };

  const del = async (id) => {
    if (!window.confirm('Delete this question?')) return;
    await questionAPI.delete(id); load();
  };

  const visibleQuestions = questions.filter(q => !search.trim() || q.questionText?.toLowerCase().includes(search.trim().toLowerCase()));

  return (
    <div className="page questions-page">
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <div><h2>❓ Questions</h2><p>Browse and manage interview questions</p></div>
        {isAdmin && <button className="btn btn-primary" onClick={openAdd}>+ Add Question</button>}
      </div>

      {/* Filters */}
      <div className="question-filters">
        <input className="form-control question-search" type="search" placeholder="Search questions..." value={search} onChange={e => setSearch(e.target.value)} />
        <select className="form-control" style={{ width: 180 }} value={filterCat} onChange={e => setFilterCat(e.target.value)}>
          <option value="">All Categories</option>
          {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
        <select className="form-control" style={{ width: 150 }} value={filterDiff} onChange={e => setFilterDiff(e.target.value)}>
          <option value="">All Difficulties</option>
          <option>Easy</option><option>Medium</option><option>Hard</option>
        </select>
      </div>

      {loading ? (
        <div className="loading-center"><div className="spinner" /></div>
      ) : (
        <div className="card">
          {visibleQuestions.length === 0 ? (
            <div className="empty-state"><div className="empty-icon">❓</div><h3>No questions found</h3><p>Try adjusting your filters.</p></div>
          ) : (
            <div className="table-wrap">
              <table>
                <thead><tr><th>Question</th><th>Category</th><th>Difficulty</th><th>Type</th>{isAdmin && <th>Actions</th>}</tr></thead>
                <tbody>
                  {visibleQuestions.map(q => (
                    <tr key={q.id}>
                      <td style={{ maxWidth: 400 }}>{q.questionText}</td>
                      <td><span className="badge badge-blue">{q.categoryName}</span></td>
                      <td><span className={`badge badge-${q.difficultyLevel.toLowerCase()}`}>{q.difficultyLevel}</span></td>
                      <td><div className="question-type-cell"><span className={`badge ${q.isMCQ ? 'badge-purple' : 'badge-blue'}`}>{q.isMCQ ? 'MCQ' : 'Written'}</span><small>{q.isAIGenerated ? 'AI generated' : 'Manual'}</small></div></td>
                      {isAdmin && (
                        <td>
                          <div style={{ display: 'flex', gap: 8 }}>
                            <button className="btn btn-secondary btn-sm" onClick={() => openEdit(q)}>Edit</button>
                            <button className="btn btn-danger btn-sm" onClick={() => del(q.id)}>Delete</button>
                          </div>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {modal && createPortal(
        <div className="modal-overlay" onClick={e => e.target === e.currentTarget && setModal(false)}>
          <div className="modal" role="dialog" aria-modal="true" aria-labelledby="question-modal-title">
            <div className="modal-header">
              <div className="modal-title" id="question-modal-title">{editQ ? 'Edit Question' : 'Add Question'}</div>
              <button type="button" className="modal-close" onClick={() => setModal(false)} aria-label="Close question form">×</button>
            </div>
            {error && <div className="alert alert-error">{error}</div>}
            {!editQ && (
              <div className="form-group">
                <label className="form-label">Category</label>
                <select className="form-control" value={form.categoryId} onChange={e => setForm(f => ({ ...f, categoryId: e.target.value }))}>
                  <option value="">Select category...</option>
                  {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </div>
            )}
            <div className="form-group">
              <label className="form-label">Question type</label>
              <div className="generation-mode-selector" role="group" aria-label="Question type">
                <button type="button" className={!form.isMCQ ? 'active' : ''} onClick={() => setForm(f => ({ ...f, isMCQ: false, correctAnswer: '' }))} aria-pressed={!form.isMCQ}>Written</button>
                <button type="button" className={form.isMCQ ? 'active' : ''} onClick={() => setForm(f => ({ ...f, isMCQ: true, correctAnswer: '' }))} aria-pressed={form.isMCQ}>MCQ</button>
              </div>
            </div>
            <div className="form-group">
              <label className="form-label">Question</label>
              <textarea className="form-control" style={{ minHeight: 100 }} value={form.questionText}
                onChange={e => setForm(f => ({ ...f, questionText: e.target.value }))} placeholder="Enter the interview question..." />
            </div>
            {form.isMCQ ? (
              <div className="mcq-admin-fields">
                {['A', 'B', 'C', 'D'].map(letter => <div className="form-group" key={letter}><label className="form-label" htmlFor={`option-${letter}`}>Option {letter}</label><input id={`option-${letter}`} className="form-control" value={form[`option${letter}`]} onChange={e => setForm(f => ({ ...f, [`option${letter}`]: e.target.value }))} placeholder={`Enter option ${letter}`} /></div>)}
                <div className="form-group"><label className="form-label" htmlFor="correct-option">Correct answer</label><select id="correct-option" className="form-control" value={form.correctAnswer} onChange={e => setForm(f => ({ ...f, correctAnswer: e.target.value }))}><option value="">Select correct option…</option>{['A', 'B', 'C', 'D'].map(letter => <option key={letter} value={letter}>{letter}{form[`option${letter}`] ? ` — ${form[`option${letter}`]}` : ''}</option>)}</select>{editQ && !form.correctAnswer && <small className="field-help">For security, select the correct answer again before saving.</small>}</div>
              </div>
            ) : (
              <div className="form-group"><label className="form-label">Reference answer</label><textarea className="form-control" value={form.correctAnswer} onChange={e => setForm(f => ({ ...f, correctAnswer: e.target.value }))} placeholder="Reference answer for AI evaluation..." /></div>
            )}
            <div className="form-group">
              <label className="form-label">Difficulty</label>
              <select className="form-control" value={form.difficultyLevel} onChange={e => setForm(f => ({ ...f, difficultyLevel: e.target.value }))}>
                <option>Easy</option><option>Medium</option><option>Hard</option>
              </select>
            </div>
            <div style={{ display: 'flex', gap: 12, justifyContent: 'flex-end' }}>
              <button className="btn btn-secondary" onClick={() => setModal(false)}>Cancel</button>
              <button className="btn btn-primary" onClick={save} disabled={saving}>{saving ? 'Saving...' : 'Save'}</button>
            </div>
          </div>
        </div>
      , document.body)}
    </div>
  );
}

// ── CATEGORIES PAGE ────────────────────────────────────────────
export function CategoriesPage() {
  const { isAdmin } = useAuth();
  const navigate    = useNavigate();
  const [categories, setCategories] = useState([]);
  const [loading, setLoading]       = useState(true);
  const [modal, setModal]           = useState(false);
  const [editC, setEditC]           = useState(null);
  const [name, setName]             = useState('');
  const [saving, setSaving]         = useState(false);

  const load = () => {
    setLoading(true);
    categoryAPI.getAll().then(r => setCategories(r.data.data || [])).finally(() => setLoading(false));
  };
  useEffect(load, []);

  const catIcon = (n) => ({ Java:'☕', SQL:'🗄️', 'ASP.NET Core':'⚙️', Python:'🐍', 'HR Interview':'👔', 'C#':'💎', 'React.js':'⚛️' }[n] || '📚');

  const save = async () => {
    setSaving(true);
    try {
      if (editC) await categoryAPI.update(editC.id, { name });
      else       await categoryAPI.create({ name });
      setModal(false); load();
    } catch { } finally { setSaving(false); }
  };

  const del = async (id) => {
    if (!window.confirm('Delete this category?')) return;
    await categoryAPI.delete(id); load();
  };

  return (
    <div className="page categories-page">
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <div><h2>📚 Categories</h2><p>All interview topics available</p></div>
        {isAdmin && <button className="btn btn-primary" onClick={() => { setEditC(null); setName(''); setModal(true); }}>+ Add Category</button>}
      </div>

      {loading ? (
        <div className="loading-center"><div className="spinner" /></div>
      ) : (
        <div className="category-grid">
          {categories.map(cat => (
            <div key={cat.id} className="category-card" onClick={() => navigate(`/interview`)}>
              <div className="category-icon">{catIcon(cat.name)}</div>
              <div className="category-name">{cat.name}</div>
              <div className="category-count">{cat.questionCount} questions</div>
              {isAdmin && (
                <div style={{ display: 'flex', gap: 8, marginTop: 14, justifyContent: 'center' }} onClick={e => e.stopPropagation()}>
                  <button className="btn btn-secondary btn-sm" onClick={() => { setEditC(cat); setName(cat.name); setModal(true); }}>Edit</button>
                  <button className="btn btn-danger btn-sm" onClick={() => del(cat.id)}>Del</button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {modal && (
        <div className="modal-overlay" onClick={e => e.target === e.currentTarget && setModal(false)}>
          <div className="modal">
            <div className="modal-header">
              <div className="modal-title">{editC ? 'Edit Category' : 'Add Category'}</div>
              <button className="modal-close" onClick={() => setModal(false)}>×</button>
            </div>
            <div className="form-group">
              <label className="form-label">Category Name</label>
              <input className="form-control" value={name} onChange={e => setName(e.target.value)} placeholder="e.g. Node.js" />
            </div>
            <div style={{ display: 'flex', gap: 12, justifyContent: 'flex-end' }}>
              <button className="btn btn-secondary" onClick={() => setModal(false)}>Cancel</button>
              <button className="btn btn-primary" onClick={save} disabled={saving}>{saving ? 'Saving...' : 'Save'}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// ── ADMIN PAGE ─────────────────────────────────────────────────
export function AdminPage() {
  const { isAdmin } = useAuth();
  const navigate    = useNavigate();

  if (!isAdmin) {
    return (
      <div className="card" style={{ textAlign: 'center', padding: 60 }}>
        <div style={{ fontSize: 48, marginBottom: 16 }}>🔒</div>
        <h3 style={{ fontFamily: 'var(--font-head)', marginBottom: 8 }}>Access Denied</h3>
        <p style={{ color: 'var(--text2)' }}>You need Admin role to access this page.</p>
      </div>
    );
  }

  const panels = [
    { icon: '📚', title: 'Manage Categories', desc: 'Add, edit, delete interview categories', path: '/categories', color: 'var(--accent)' },
    { icon: '❓', title: 'Manage Questions',  desc: 'Add, edit, delete interview questions',  path: '/questions',  color: 'var(--purple)' },
    { icon: '🤖', title: 'AI Tools',          desc: 'Generate questions and evaluate answers', path: '/ai-tools',   color: 'var(--green)' },
    { icon: '📊', title: 'Dashboard',         desc: 'View system analytics and performance',  path: '/dashboard',  color: 'var(--yellow)' },
  ];

  return (
    <div>
      <div className="page-header">
        <h2>⚙️ Admin Panel</h2>
        <p>Manage your interview preparation system</p>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(260px, 1fr))', gap: 20 }}>
        {panels.map(p => (
          <div key={p.path} className="card" style={{ cursor: 'pointer', borderTop: `3px solid ${p.color}` }}
            onClick={() => navigate(p.path)}>
            <div style={{ fontSize: 40, marginBottom: 14 }}>{p.icon}</div>
            <div style={{ fontFamily: 'var(--font-head)', fontWeight: 700, fontSize: 18, marginBottom: 6 }}>{p.title}</div>
            <div style={{ fontSize: 13, color: 'var(--text2)' }}>{p.desc}</div>
          </div>
        ))}
      </div>

      <div className="generate-box" style={{ marginTop: 24 }}>
        <h3>🤖 Quick AI Actions</h3>
        <p>Use AI to bulk generate questions for any category</p>
        <button className="btn btn-primary" onClick={() => navigate('/ai-tools')}>
          Open AI Tools →
        </button>
      </div>
    </div>
  );
}
