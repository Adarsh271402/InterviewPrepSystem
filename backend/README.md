# 🤖 AI-Powered Interview Preparation System

A full-stack web application built with **ASP.NET Core 8**, **SQL Server**, and **AI integration** (OpenAI/Gemini) to help students and job seekers prepare for technical interviews.

---

## 🚀 Features

| Module | Features |
|--------|----------|
| **Auth** | Register, Login, JWT Tokens, Role-Based Access |
| **Categories** | Java, SQL, ASP.NET Core, Python, HR, C#, React.js |
| **Questions** | CRUD, Difficulty Levels (Easy/Medium/Hard), AI-Generated |
| **Mock Interview** | Random Questions, Timer-Based, Submit & Get AI Feedback |
| **AI Evaluation** | Score/10, Feedback, Suggestions, Grammar Check |
| **AI Question Gen** | Generate questions dynamically via OpenAI/Gemini |
| **Dashboard** | Performance Analytics, Test History, Category Stats |
| **Admin Panel** | Manage Users, Questions, Categories |

---

## 🧠 AI Features (Core)

### 1. AI Answer Evaluation
- **POST** `/api/ai/evaluate`
- Input: Question + User Answer
- Output: Score (0-10), Feedback, Suggestions, Grammar Feedback, Technical Accuracy

### 2. AI Question Generation
- **POST** `/api/ai/generate-questions`
- Input: Category, Difficulty, Number of Questions, Target Audience
- Output: Generated questions with suggested answers

### 3. AI Career Suggestions
- **GET** `/api/ai/suggestions`
- Based on your test history, get personalized improvement tips

---

## 🛠️ Technologies

- **Backend**: ASP.NET Core 8 Web API
- **Database**: SQL Server + Entity Framework Core
- **Auth**: JWT Bearer Tokens + BCrypt Password Hashing
- **AI**: OpenAI GPT / Google Gemini API
- **Docs**: Swagger UI (auto-generated)
- **Patterns**: Repository Pattern, Dependency Injection, Middleware, DTOs

---

## ⚙️ Setup Instructions

### Prerequisites
- Visual Studio 2022
- SQL Server (or SQL Server Express)
- .NET 8 SDK
- OpenAI API Key (from [platform.openai.com](https://platform.openai.com)) OR Gemini API Key

### Step 1: Clone & Open
```bash
git clone https://github.com/yourusername/InterviewPrepSystem.git
cd InterviewPrepSystem
```
Open `InterviewPrepSystem.sln` in Visual Studio 2022.

### Step 2: Configure appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=InterviewPrepDB;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyHereMinimum32Characters!!",
    "Issuer": "InterviewPrepSystem",
    "Audience": "InterviewPrepUsers",
    "ExpiryInDays": 7
  },
  "AISettings": {
    "Provider": "OpenAI",
    "OpenAI": {
      "ApiKey": "sk-YOUR_OPENAI_KEY_HERE",
      "Model": "gpt-3.5-turbo"
    }
  }
}
```

### Step 3: Database Setup

**Option A — EF Migrations (Recommended)**
```bash
# In Package Manager Console (Visual Studio)
Add-Migration InitialCreate
Update-Database
```

**Option B — SQL Script**
```
Run DatabaseSetup.sql in SQL Server Management Studio (SSMS)
```

### Step 4: Run the Project
- Press `F5` in Visual Studio
- Swagger UI opens at `https://localhost:{port}`
- Default Admin: `admin@interviewprep.com` / `Admin@123`

---

## 📡 API Endpoints

### Auth
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login, get JWT token |

### Categories
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/category` | Get all categories |
| GET | `/api/category/{id}` | Get by ID |
| POST | `/api/category` | Create (Admin) |
| PUT | `/api/category/{id}` | Update (Admin) |
| DELETE | `/api/category/{id}` | Delete (Admin) |

### Questions
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/question` | Get all (filter by categoryId, difficulty) |
| GET | `/api/question/{id}` | Get by ID |
| POST | `/api/question` | Create (Admin) |
| PUT | `/api/question/{id}` | Update (Admin) |
| DELETE | `/api/question/{id}` | Delete (Admin) |

### Mock Test
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/mocktest/start` | Start a mock interview |
| POST | `/api/mocktest/submit` | Submit answer (AI evaluates) |
| GET | `/api/mocktest/result/{id}` | Get test result |
| GET | `/api/mocktest/history` | Get test history |

### AI
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/ai/evaluate` | AI answer evaluation |
| POST | `/api/ai/generate-questions` | AI question generation |
| POST | `/api/ai/generate-and-save` | Generate + save to DB (Admin) |
| GET | `/api/ai/suggestions` | Career improvement suggestions |

### Dashboard
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/dashboard` | User analytics & performance |

---

## 📝 Sample API Requests (Postman)

### Register
```json
POST /api/auth/register
{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "John@123"
}
```

### Start Mock Test
```json
POST /api/mocktest/start
Authorization: Bearer {token}
{
  "categoryId": 3,
  "numberOfQuestions": 5,
  "difficultyLevel": "Mixed"
}
```

### Submit Answer (AI Evaluates)
```json
POST /api/mocktest/submit
Authorization: Bearer {token}
{
  "mockTestId": 1,
  "questionId": 1,
  "userAnswer": "Dependency Injection is a design pattern where dependencies are provided to a class from outside rather than the class creating them itself. It promotes loose coupling and makes code more testable."
}
```
**AI Response:**
```json
{
  "aiScore": 8,
  "aiFeedback": "Good explanation of DI concept. You correctly identified loose coupling and testability benefits.",
  "aiSuggestions": "Add a code example showing constructor injection. Mention the three lifetime scopes: Singleton, Scoped, Transient."
}
```

### Generate AI Questions
```json
POST /api/ai/generate-questions
Authorization: Bearer {token}
{
  "categoryName": "ASP.NET Core",
  "difficultyLevel": "Medium",
  "numberOfQuestions": 5,
  "targetAudience": "freshers"
}
```

---

## 🏗️ Project Structure
```
InterviewPrepSystem/
├── Controllers/          ← Auth, Category, Question, MockTest, AI, Dashboard
├── Models/               ← User, Category, Question, MockTest, UserAnswer
├── DTOs/                 ← Request/Response data transfer objects
├── Services/             ← Business logic layer
├── AI/                   ← OpenAI & Gemini service implementations
├── Data/                 ← ApplicationDbContext (EF Core)
├── Helpers/              ← JWT Helper, Extensions
├── Middleware/           ← Global exception handler
├── Migrations/           ← EF Core database migrations
├── appsettings.json      ← Configuration
├── Program.cs            ← App startup, DI, middleware pipeline
└── DatabaseSetup.sql     ← Manual SQL script
```

---

## 🔐 Security
- JWT Bearer token authentication
- BCrypt password hashing (work factor 11)
- Role-based authorization (User / Admin)
- API key stored in configuration (not hardcoded)
- Global exception handling (no stack traces in production)

---

## 🔄 Switching AI Provider

In `appsettings.json`, change `"Provider"` to switch between OpenAI and Gemini:
```json
"AISettings": {
  "Provider": "Gemini"  // or "OpenAI"
}
```

---

## 📈 Resume Description
> Developed an AI-powered Interview Preparation System using ASP.NET Core 8 Web API and SQL Server with JWT authentication, mock interview functionality, AI-based answer evaluation using OpenAI/Gemini APIs, dynamic question generation, and performance analytics dashboard.

---

## 🚀 Future Enhancements
- [ ] Voice Interview (Speech-to-Text)
- [ ] Real-Time Chat (SignalR)
- [ ] PDF Report Download
- [ ] Leaderboard
- [ ] Mobile App (Flutter/React Native)
- [ ] Resume Analyzer Integration
