# InterviewAI — React Frontend

## 🚀 Quick Setup (3 Steps)

### Step 1 — Install Node.js
Download from: https://nodejs.org (choose LTS version)

### Step 2 — Install & Run
Open terminal/command prompt in this folder and run:
```
npm install
npm start
```
Browser opens automatically at http://localhost:3000

### Step 3 — Connect to your API
Open `src/services/api.js` and update the BASE_URL to match your backend:
```js
const BASE_URL = 'https://localhost:7001'; // ← change port if needed
```
Check your ASP.NET Core project's launchSettings.json for the correct port.

---

## 📄 Pages Included

| Page | Route | Description |
|------|-------|-------------|
| Login | /login | JWT login |
| Register | /register | New account |
| Dashboard | /dashboard | Analytics + charts |
| Categories | /categories | Browse topics |
| Mock Interview | /interview | Full interview session |
| Results | /result/:id | AI feedback per answer |
| History | /history | All past tests |
| AI Tools | /ai-tools | Evaluate + Generate |
| Admin | /admin | Admin panel |

---

## ⚠️ CORS Fix (if you get network errors)

In your ASP.NET Core `Program.cs`, make sure this is set:
```csharp
policy.WithOrigins("http://localhost:3000")
```

---

## 🔑 Default Admin Login
- Email: admin@interviewprep.com
- Password: Admin@123

## Backend

The ASP.NET Core API is in `backend/`. Install the .NET 8 SDK and SQL Server.
Before running, supply your settings through environment variables:

```powershell
$env:ConnectionStrings__DefaultConnection = 'YOUR_SQL_SERVER_CONNECTION_STRING'
$env:JwtSettings__SecretKey = 'YOUR_RANDOM_SECRET_AT_LEAST_32_CHARACTERS'
$env:AISettings__Groq__ApiKey = 'YOUR_GROQ_API_KEY'
dotnet run --project backend/InterviewPrepSystem.csproj
```

See `backend/README.md` and `backend/DatabaseSetup.sql` for database setup.

## Separate production hosting

- Frontend: Netlify (`netlify.toml`), set `REACT_APP_API_URL` to the live Render API origin, then rebuild.
- Backend: Render Docker service (`render.yaml`), using the Free plan.
- Database: a dedicated Neon PostgreSQL database. Set `DatabaseProvider=PostgreSQL` and `ConnectionStrings__DefaultConnection` to its .NET connection string. The initial PostgreSQL schema and categories are created on first startup. Existing SQL Server data is not transferred.
- Configure `JwtSettings__SecretKey` and `AISettings__Groq__ApiKey` only in the hosting dashboard. The Blueprint generates the JWT signing secret.
- Backend health endpoint: `/health`.

Free Render services can sleep after inactivity. Keep the database separate from other applications.
