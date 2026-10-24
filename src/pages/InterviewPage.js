import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { categoryAPI, mockTestAPI } from '../services/api';

const DIFFICULTIES = ['Mixed', 'Easy', 'Medium', 'Hard'];
const QUESTION_TYPES = [
  { value: 'MCQ', label: 'MCQ', hint: 'Choose one answer' },
  { value: 'Text', label: 'Written', hint: 'AI evaluated response' },
  { value: 'Mixed', label: 'Mixed', hint: 'Both question types' },
];
const LETTERS = ['A', 'B', 'C', 'D'];
const optionText = (question, letter) => question?.[`option${letter}`] || '';

export default function InterviewPage() {
  const navigate = useNavigate();
  const [categories, setCategories] = useState([]);
  const [selectedCat, setCat] = useState(null);
  const [difficulty, setDifficulty] = useState('Mixed');
  const [questionType, setQuestionType] = useState('MCQ');
  const [questionCount, setQuestionCount] = useState(5);
  const [phase, setPhase] = useState('setup');
  const [mockTestId, setMockTestId] = useState(null);
  const [questions, setQuestions] = useState([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [answer, setAnswer] = useState('');
  const [feedback, setFeedback] = useState(null);
  const [submitted, setSubmitted] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [timeLeft, setTimeLeft] = useState(120);

  const question = questions[currentIndex];
  const isMCQ = Boolean(question?.isMCQ);

  useEffect(() => {
    categoryAPI.getAll().then(r => setCategories(r.data.data || []))
      .catch(() => setError('Could not load categories. Please refresh and try again.'));
  }, []);

  const submitAnswer = useCallback(async () => {
    if (submitted || loading || !answer.trim() || !question) return;
    setSubmitted(true); setLoading(true); setError('');
    try {
      const response = await mockTestAPI.submit({ mockTestId, questionId: question.id, userAnswer: answer });
      setFeedback(response.data.data);
    } catch (requestError) {
      setError(requestError.response?.data?.message || (isMCQ ? 'Could not check this answer. Please try again.' : 'AI evaluation failed. Please try again.'));
      setSubmitted(false);
    } finally { setLoading(false); }
  }, [submitted, loading, answer, question, mockTestId, isMCQ]);

  useEffect(() => {
    if (phase !== 'interview' || submitted || isMCQ) return undefined;
    if (timeLeft <= 0) {
      if (answer.trim()) submitAnswer();
      return undefined;
    }
    const timer = setTimeout(() => setTimeLeft(value => value - 1), 1000);
    return () => clearTimeout(timer);
  }, [phase, timeLeft, submitted, isMCQ, answer, submitAnswer]);

  const startInterview = async () => {
    if (!selectedCat) { setError('Please select a category.'); return; }
    setError(''); setLoading(true);
    try {
      const response = await mockTestAPI.start({ categoryId: selectedCat.id, numberOfQuestions: questionCount, difficultyLevel: difficulty, questionType });
      const data = response.data.data;
      setMockTestId(data.mockTestId); setQuestions(data.questions || []); setCurrentIndex(0);
      setAnswer(''); setFeedback(null); setSubmitted(false); setTimeLeft(120); setPhase('interview');
    } catch (requestError) {
      setError(requestError.response?.data?.message || 'Failed to start. Try another category or question type.');
    } finally { setLoading(false); }
  };

  const nextQuestion = () => {
    if (currentIndex + 1 >= questions.length) { navigate(`/result/${mockTestId}`); return; }
    setCurrentIndex(value => value + 1); setAnswer(''); setFeedback(null); setSubmitted(false); setError(''); setTimeLeft(120);
  };

  const categoryIcon = name => ({ Java: 'J', SQL: 'DB', 'ASP.NET Core': '.NET', Python: 'Py', 'HR Interview': 'HR', 'C#': 'C#', 'React.js': 'Rx' }[name] || 'Q');
  const formatTime = seconds => `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, '0')}`;
  const progress = questions.length ? ((currentIndex + (submitted ? 1 : 0)) / questions.length) * 100 : 0;

  if (phase === 'setup') return (
    <div className="page interview-setup-page">
      <div className="page-header"><div className="eyebrow">Practice studio</div><h2>Mock Interview</h2><p>Choose a topic and build a focused practice session.</p></div>
      {error && <div className="alert alert-error" role="alert">{error}</div>}
      <div className="interview-setup-grid">
        <div className="card">
          <div className="card-title">Select topic</div><div className="card-subtitle">What would you like to practice?</div>
          <div className="category-grid">
            {categories.map(category => (
              <button type="button" key={category.id} className={`category-card ${selectedCat?.id === category.id ? 'selected' : ''}`} onClick={() => { setCat(category); setError(''); }} aria-pressed={selectedCat?.id === category.id}>
                <span className="category-icon">{categoryIcon(category.name)}</span><span className="category-name">{category.name}</span><span className="category-count">{category.questionCount} questions</span>
              </button>
            ))}
          </div>
        </div>
        <aside className="card interview-settings">
          <div className="card-title">Session settings</div><div className="card-subtitle">Tune your practice experience</div>
          <div className="form-group"><label className="form-label">Question type</label>
            <div className="question-type-selector" role="group" aria-label="Question type">
              {QUESTION_TYPES.map(type => <button type="button" key={type.value} className={`question-type-option ${questionType === type.value ? 'active' : ''}`} onClick={() => setQuestionType(type.value)} aria-pressed={questionType === type.value}><strong>{type.label}</strong><small>{type.hint}</small></button>)}
            </div>
          </div>
          <div className="form-group"><label className="form-label">Difficulty</label>
            <div className="difficulty-selector">{DIFFICULTIES.map(level => <button type="button" key={level} className={`btn ${difficulty === level ? 'btn-primary' : 'btn-secondary'} btn-sm`} onClick={() => setDifficulty(level)} aria-pressed={difficulty === level}>{level}</button>)}</div>
          </div>
          <div className="form-group"><label className="form-label" htmlFor="question-count">Number of questions: <strong>{questionCount}</strong></label><input id="question-count" type="range" min={3} max={10} value={questionCount} onChange={e => setQuestionCount(+e.target.value)} className="range-control" /><div className="range-labels"><span>3</span><span>10</span></div></div>
          {selectedCat && <div className="session-summary"><strong>{selectedCat.name}</strong><span>{questionCount} questions · {difficulty} · {questionType === 'Text' ? '2 min each' : questionType}</span></div>}
          <button type="button" className="btn btn-primary btn-lg start-interview-button" onClick={startInterview} disabled={loading || !selectedCat}>{loading ? 'Starting…' : 'Start interview'}</button>
        </aside>
      </div>
    </div>
  );

  return (
    <div className="page interview-session-page">
      <button type="button" className="back-link" onClick={() => navigate(-1)}>← Back</button>
      <div className="page-header session-header">
        <div><div className="eyebrow">Live practice session</div><h2>Mock Interview</h2><p>{selectedCat?.name} · {difficulty} · {isMCQ ? 'MCQ' : 'Written'}</p></div>
        {!isMCQ ? <div className="circular-timer" style={{ '--timer': `${(timeLeft / 120) * 360}deg`, color: timeLeft <= 30 ? 'var(--red)' : timeLeft <= 60 ? 'var(--yellow)' : 'var(--accent2)' }}><div><strong>{formatTime(timeLeft)}</strong><small>remaining</small></div></div> : <div className="untimed-badge"><strong>Untimed</strong><small>Choose carefully</small></div>}
      </div>
      <div className="progress-meta"><span>Question {currentIndex + 1} of {questions.length}</span><span>{Math.round(progress)}% complete</span></div>
      <div className="progress-bar interview-progress"><div className="progress-fill" style={{ width: `${progress}%` }} /></div>
      <div className="question-indicators" aria-label="Question progress">{questions.map((item, index) => <span key={item.id || index} className={`${index < currentIndex ? 'complete' : ''} ${index === currentIndex ? 'current' : ''}`}>{index < currentIndex ? '✓' : index + 1}</span>)}</div>
      {error && <div className="alert alert-error" role="alert">{error}</div>}
      <div className="interview-layout">
        <main>
          <div className="question-card">
            <div className="question-toolbar"><div className="question-number">Question {currentIndex + 1}</div><div className="question-badges"><span className={`badge badge-${question?.difficultyLevel?.toLowerCase()}`}>{question?.difficultyLevel}</span><span className="badge badge-blue">{isMCQ ? 'Multiple choice' : 'Written answer'}</span></div></div>
            <div className="question-text">{question?.questionText}</div>
            {isMCQ ? (
              <div className="mcq-options" role="group" aria-label="Answer choices">
                {LETTERS.map(letter => {
                  const selected = answer === letter;
                  const correct = submitted && feedback?.correctAnswer === letter;
                  const incorrect = submitted && selected && feedback?.correctAnswer !== letter;
                  return <button type="button" key={letter} className={`mcq-option ${selected ? 'selected' : ''} ${correct ? 'correct' : ''} ${incorrect ? 'incorrect' : ''}`} onClick={() => !submitted && setAnswer(letter)} disabled={submitted || loading} aria-pressed={selected} aria-label={`Option ${letter}: ${optionText(question, letter)}`}><span className="mcq-letter">{letter}</span><span className="mcq-option-text">{optionText(question, letter)}</span>{correct && <span className="mcq-state" aria-label="Correct answer">✓</span>}{incorrect && <span className="mcq-state" aria-label="Incorrect answer">×</span>}</button>;
                })}
              </div>
            ) : <div className="written-answer-block"><label className="form-label" htmlFor="written-answer">Your answer</label><textarea id="written-answer" className="form-control interview-answer-input" placeholder="Explain your answer with examples, concepts, and reasoning…" value={answer} onChange={e => setAnswer(e.target.value)} disabled={submitted} /><span className="character-count">{answer.length} characters</span></div>}
            <div className="answer-footer"><span className="answer-helper">{isMCQ ? (answer ? `Option ${answer} selected` : 'Select one option to continue') : 'Your answer is evaluated securely by AI'}</span>
              {!submitted ? <button type="button" className="btn btn-primary btn-lg" onClick={submitAnswer} disabled={loading || !answer.trim()}>{loading ? (isMCQ ? 'Checking…' : 'AI evaluating…') : (isMCQ ? 'Submit answer' : 'Submit for AI evaluation')}</button> : <button type="button" className="btn btn-success btn-lg" onClick={nextQuestion}>{currentIndex + 1 >= questions.length ? 'See final results' : 'Next question →'}</button>}
            </div>
          </div>
        </main>
        <aside className="interview-side-panel">
          {feedback ? <div className={`ai-feedback-card ${isMCQ ? (feedback.isCorrect ? 'mcq-feedback-correct' : 'mcq-feedback-incorrect') : ''}`}><div className="feedback-score-row"><div className={`score-ring ${feedback.aiScore >= 8 ? 'score-high' : feedback.aiScore >= 5 ? 'score-medium' : 'score-low'}`}>{feedback.aiScore}/10</div><div><div className="feedback-result-title">{isMCQ ? (feedback.isCorrect ? 'Correct answer' : 'Needs review') : 'AI score'}</div><div className="feedback-result-subtitle">{isMCQ && feedback.correctAnswer ? `Correct option: ${feedback.correctAnswer}` : 'Personalized evaluation'}</div></div></div><div className="feedback-section"><div className="feedback-label">Feedback</div><div className="feedback-text">{feedback.aiFeedback}</div></div><div className="feedback-section"><div className="feedback-label">Suggestion</div><div className="feedback-text">{feedback.aiSuggestions}</div></div></div> : <div className="card evaluator-placeholder"><div className="evaluator-icon">AI</div><div className="card-title">{isMCQ ? 'Instant answer check' : 'AI evaluator'}</div><p>{isMCQ ? 'Select an option and submit to reveal the correct answer.' : 'Submit your response for detailed feedback and an improvement score.'}</p></div>}
          <div className="card question-queue"><div className="card-title">Session progress</div>{questions.map((item, index) => <div key={item.id || index} className={`queue-item ${index === currentIndex ? 'active' : ''}`}><span>{index < currentIndex ? '✓' : index + 1}</span><div><strong>{index < currentIndex ? 'Answered' : index === currentIndex ? 'Current question' : 'Upcoming'}</strong><small>{item.isMCQ ? 'MCQ' : 'Written'}</small></div></div>)}</div>
        </aside>
      </div>
    </div>
  );
}
