import axios from 'axios';

const BASE_URL = 'https://localhost:7001';

const api = axios.create({ baseURL: BASE_URL });

// Attach JWT token to every request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Auto logout on 401
api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(err);
  }
);

// ── Auth ──────────────────────────────────────────
export const authAPI = {
  register: (data) => api.post('/api/auth/register', data),
  login:    (data) => api.post('/api/auth/login', data),
};

// ── Categories ────────────────────────────────────
export const categoryAPI = {
  getAll:    ()       => api.get('/api/category'),
  getById:   (id)     => api.get(`/api/category/${id}`),
  create:    (data)   => api.post('/api/category', data),
  update:    (id, d)  => api.put(`/api/category/${id}`, d),
  delete:    (id)     => api.delete(`/api/category/${id}`),
};

// ── Questions ─────────────────────────────────────
export const questionAPI = {
  getAll:  (params) => api.get('/api/question', { params }),
  getById: (id)     => api.get(`/api/question/${id}`),
  create:  (data)   => api.post('/api/question', data),
  update:  (id, d)  => api.put(`/api/question/${id}`, d),
  delete:  (id)     => api.delete(`/api/question/${id}`),
};

// ── Mock Test ─────────────────────────────────────
export const mockTestAPI = {
  start:      (data) => api.post('/api/mocktest/start', data),
  submit:     (data) => api.post('/api/mocktest/submit', data),
  getResult:  (id)   => api.get(`/api/mocktest/result/${id}`),
  getHistory: ()     => api.get('/api/mocktest/history'),
};

// ── AI ────────────────────────────────────────────
export const aiAPI = {
  evaluate:          (data) => api.post('/api/ai/evaluate', data),
  generateQuestions: (data) => api.post('/api/ai/generate-questions', data),
  generateMCQs:      (data) => api.post('/api/ai/generate-mcqs', data),
  getSuggestions:    ()     => api.get('/api/ai/suggestions'),
};

// ── Dashboard ─────────────────────────────────────
export const dashboardAPI = {
  get: () => api.get('/api/dashboard'),
};

export default api;
