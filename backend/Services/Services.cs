using InterviewPrepSystem.Data;
using InterviewPrepSystem.DTOs;
using InterviewPrepSystem.Helpers;
using InterviewPrepSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InterviewPrepSystem.Services
{
    // ============================================================
    //  AUTH SERVICE
    // ============================================================
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto);
        Task<AuthResponseDTO> LoginAsync(LoginDTO dto);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtHelper _jwt;

        public AuthService(ApplicationDbContext db, IJwtHelper jwt)
        {
            _db  = db;
            _jwt = jwt;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email already registered.");

            var user = new User
            {
                Name         = dto.Name,
                Email        = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role         = "User"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
                       ?? throw new UnauthorizedAccessException("Invalid email or password.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            return BuildAuthResponse(user);
        }

        private AuthResponseDTO BuildAuthResponse(User user) => new()
        {
            Token     = _jwt.GenerateToken(user),
            Name      = user.Name,
            Email     = user.Email,
            Role      = user.Role,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
    }

    // ============================================================
    //  CATEGORY SERVICE
    // ============================================================
    public interface ICategoryService
    {
        Task<List<CategoryDTO>> GetAllAsync();
        Task<CategoryDTO?> GetByIdAsync(int id);
        Task<CategoryDTO> CreateAsync(CreateCategoryDTO dto);
        Task<CategoryDTO?> UpdateAsync(int id, CreateCategoryDTO dto);
        Task<bool> DeleteAsync(int id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _db;

        public CategoryService(ApplicationDbContext db) => _db = db;

        public async Task<List<CategoryDTO>> GetAllAsync() =>
            await _db.Categories
                .Include(c => c.Questions)
                .Select(c => new CategoryDTO
                {
                    Id            = c.Id,
                    Name          = c.Name,
                    QuestionCount = c.Questions.Count
                }).ToListAsync();

        public async Task<CategoryDTO?> GetByIdAsync(int id) =>
            await _db.Categories
                .Include(c => c.Questions)
                .Where(c => c.Id == id)
                .Select(c => new CategoryDTO
                {
                    Id            = c.Id,
                    Name          = c.Name,
                    QuestionCount = c.Questions.Count
                }).FirstOrDefaultAsync();

        public async Task<CategoryDTO> CreateAsync(CreateCategoryDTO dto)
        {
            var category = new Category { Name = dto.Name };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return new CategoryDTO { Id = category.Id, Name = category.Name };
        }

        public async Task<CategoryDTO?> UpdateAsync(int id, CreateCategoryDTO dto)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return null;
            category.Name = dto.Name;
            await _db.SaveChangesAsync();
            return new CategoryDTO { Id = category.Id, Name = category.Name };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return false;
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            return true;
        }
    }

    // ============================================================
    //  QUESTION SERVICE
    // ============================================================
    public interface IQuestionService
    {
        Task<List<QuestionDTO>> GetAllAsync(int? categoryId = null, string? difficulty = null);
        Task<QuestionDTO?> GetByIdAsync(int id);
        Task<QuestionDTO> CreateAsync(CreateQuestionDTO dto);
        Task<QuestionDTO?> UpdateAsync(int id, UpdateQuestionDTO dto);
        Task<bool> DeleteAsync(int id);
    }

    public class QuestionService : IQuestionService
    {
        private readonly ApplicationDbContext _db;

        public QuestionService(ApplicationDbContext db) => _db = db;

        public async Task<List<QuestionDTO>> GetAllAsync(int? categoryId = null, string? difficulty = null)
        {
            var query = _db.Questions.Include(q => q.Category).AsQueryable();
            if (categoryId.HasValue) query = query.Where(q => q.CategoryId == categoryId);
            if (!string.IsNullOrEmpty(difficulty)) query = query.Where(q => q.DifficultyLevel == difficulty);

            return await query.Select(q => new QuestionDTO
            {
                Id              = q.Id,
                CategoryId      = q.CategoryId,
                CategoryName    = q.Category!.Name,
                QuestionText    = q.QuestionText,
                DifficultyLevel = q.DifficultyLevel,
                IsAIGenerated   = q.IsAIGenerated,
                IsMCQ           = q.IsMCQ,
                OptionA         = q.OptionA,
                OptionB         = q.OptionB,
                OptionC         = q.OptionC,
                OptionD         = q.OptionD
            }).ToListAsync();
        }

        public async Task<QuestionDTO?> GetByIdAsync(int id) =>
            await _db.Questions.Include(q => q.Category)
                .Where(q => q.Id == id)
                .Select(q => new QuestionDTO
                {
                    Id              = q.Id,
                    CategoryId      = q.CategoryId,
                    CategoryName    = q.Category!.Name,
                    QuestionText    = q.QuestionText,
                    DifficultyLevel = q.DifficultyLevel,
                    IsAIGenerated   = q.IsAIGenerated,
                    IsMCQ           = q.IsMCQ,
                    OptionA         = q.OptionA,
                    OptionB         = q.OptionB,
                    OptionC         = q.OptionC,
                    OptionD         = q.OptionD
                }).FirstOrDefaultAsync();

        public async Task<QuestionDTO> CreateAsync(CreateQuestionDTO dto)
        {
            ValidateQuestion(dto.IsMCQ, dto.CorrectAnswer, dto.OptionA, dto.OptionB, dto.OptionC, dto.OptionD);
            var q = new Question
            {
                CategoryId      = dto.CategoryId,
                QuestionText    = dto.QuestionText,
                DifficultyLevel = dto.DifficultyLevel,
                CorrectAnswer   = dto.IsMCQ ? dto.CorrectAnswer.Trim().ToUpperInvariant() : dto.CorrectAnswer,
                IsAIGenerated   = dto.IsAIGenerated,
                IsMCQ           = dto.IsMCQ,
                OptionA         = dto.IsMCQ ? dto.OptionA.Trim() : string.Empty,
                OptionB         = dto.IsMCQ ? dto.OptionB.Trim() : string.Empty,
                OptionC         = dto.IsMCQ ? dto.OptionC.Trim() : string.Empty,
                OptionD         = dto.IsMCQ ? dto.OptionD.Trim() : string.Empty
            };
            _db.Questions.Add(q);
            await _db.SaveChangesAsync();
            await _db.Entry(q).Reference(x => x.Category).LoadAsync();
            return new QuestionDTO
            {
                Id              = q.Id,
                CategoryId      = q.CategoryId,
                CategoryName    = q.Category?.Name ?? "",
                QuestionText    = q.QuestionText,
                DifficultyLevel = q.DifficultyLevel,
                IsAIGenerated   = q.IsAIGenerated,
                IsMCQ           = q.IsMCQ,
                OptionA         = q.OptionA,
                OptionB         = q.OptionB,
                OptionC         = q.OptionC,
                OptionD         = q.OptionD
            };
        }

        public async Task<QuestionDTO?> UpdateAsync(int id, UpdateQuestionDTO dto)
        {
            var q = await _db.Questions.Include(q => q.Category).FirstOrDefaultAsync(q => q.Id == id);
            if (q == null) return null;
            ValidateQuestion(dto.IsMCQ, dto.CorrectAnswer, dto.OptionA, dto.OptionB, dto.OptionC, dto.OptionD);
            q.QuestionText    = dto.QuestionText;
            q.DifficultyLevel = dto.DifficultyLevel;
            q.CorrectAnswer   = dto.IsMCQ ? dto.CorrectAnswer.Trim().ToUpperInvariant() : dto.CorrectAnswer;
            q.IsMCQ           = dto.IsMCQ;
            q.OptionA         = dto.IsMCQ ? dto.OptionA.Trim() : string.Empty;
            q.OptionB         = dto.IsMCQ ? dto.OptionB.Trim() : string.Empty;
            q.OptionC         = dto.IsMCQ ? dto.OptionC.Trim() : string.Empty;
            q.OptionD         = dto.IsMCQ ? dto.OptionD.Trim() : string.Empty;
            await _db.SaveChangesAsync();
            return new QuestionDTO
            {
                Id              = q.Id,
                CategoryId      = q.CategoryId,
                CategoryName    = q.Category?.Name ?? "",
                QuestionText    = q.QuestionText,
                DifficultyLevel = q.DifficultyLevel,
                IsAIGenerated   = q.IsAIGenerated,
                IsMCQ           = q.IsMCQ,
                OptionA         = q.OptionA,
                OptionB         = q.OptionB,
                OptionC         = q.OptionC,
                OptionD         = q.OptionD
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var q = await _db.Questions.FindAsync(id);
            if (q == null) return false;
            _db.Questions.Remove(q);
            await _db.SaveChangesAsync();
            return true;
        }

        private static void ValidateQuestion(bool isMCQ, string correctAnswer, params string[] options)
        {
            if (!isMCQ) return;
            if (options.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("MCQ questions require all four options.");
            if (!new[] { "A", "B", "C", "D" }.Contains(correctAnswer.Trim().ToUpperInvariant()))
                throw new InvalidOperationException("MCQ correct answer must be A, B, C, or D.");
        }
    }

    // ============================================================
    //  MOCK TEST SERVICE
    // ============================================================
    public interface IMockTestService
    {
        Task<MockTestResponseDTO> StartAsync(int userId, StartMockTestDTO dto);
        Task<AnswerResultDTO> SubmitAnswerAsync(int userId, SubmitAnswerDTO dto);
        Task<MockTestResultDTO> GetResultAsync(int userId, int mockTestId);
        Task<List<TestHistoryDTO>> GetHistoryAsync(int userId);
    }

    public class MockTestService : IMockTestService
    {
        private readonly ApplicationDbContext _db;
        private readonly AI.IAIService _ai;

        public MockTestService(ApplicationDbContext db, AI.IAIService ai)
        {
            _db = db;
            _ai = ai;
        }

        public async Task<MockTestResponseDTO> StartAsync(int userId, StartMockTestDTO dto)
        {
            var category = await _db.Categories.FindAsync(dto.CategoryId)
                           ?? throw new KeyNotFoundException("Category not found.");

            if (dto.NumberOfQuestions < 1 || dto.NumberOfQuestions > 50)
                throw new InvalidOperationException("Number of questions must be between 1 and 50.");

            var questionType = string.IsNullOrWhiteSpace(dto.QuestionType)
                ? "Text"
                : dto.QuestionType.Trim();
            if (!new[] { "MCQ", "Text", "Mixed" }.Contains(questionType, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException("Question type must be MCQ, Text, or Mixed.");

            var query = _db.Questions.Where(q => q.CategoryId == dto.CategoryId);
            if (dto.DifficultyLevel != "Mixed")
                query = query.Where(q => q.DifficultyLevel == dto.DifficultyLevel);

            if (questionType.Equals("MCQ", StringComparison.OrdinalIgnoreCase))
                query = query.Where(q => q.IsMCQ);
            else if (questionType.Equals("Text", StringComparison.OrdinalIgnoreCase))
                query = query.Where(q => !q.IsMCQ);

            var questions = await query.OrderBy(_ => Guid.NewGuid())
                                       .Take(dto.NumberOfQuestions)
                                       .ToListAsync();

            if (!questions.Any())
                throw new InvalidOperationException(
                    questionType.Equals("MCQ", StringComparison.OrdinalIgnoreCase)
                        ? "No MCQ questions available for the selected criteria."
                        : "No questions available for the selected criteria.");

            var mockTest = new MockTest
            {
                UserId         = userId,
                CategoryId     = dto.CategoryId,
                TotalQuestions = questions.Count,
                Status         = "InProgress"
            };
            _db.MockTests.Add(mockTest);
            await _db.SaveChangesAsync();

            return new MockTestResponseDTO
            {
                MockTestId   = mockTest.Id,
                CategoryName = category.Name,
                StartedAt    = mockTest.CreatedDate,
                Questions    = questions.Select(q => new QuestionDTO
                {
                    Id              = q.Id,
                    CategoryId      = q.CategoryId,
                    QuestionText    = q.QuestionText,
                    DifficultyLevel = q.DifficultyLevel,
                    IsMCQ           = q.IsMCQ,
                    OptionA         = q.OptionA,
                    OptionB         = q.OptionB,
                    OptionC         = q.OptionC,
                    OptionD         = q.OptionD
                }).ToList()
            };
        }

        public async Task<AnswerResultDTO> SubmitAnswerAsync(int userId, SubmitAnswerDTO dto)
        {
            var mockTest = await _db.MockTests.FirstOrDefaultAsync(m => m.Id == dto.MockTestId && m.UserId == userId)
                           ?? throw new KeyNotFoundException("Mock test not found.");

            if (mockTest.Status == "Completed")
                throw new InvalidOperationException("This mock test is already completed.");

            var question = await _db.Questions.FindAsync(dto.QuestionId)
                           ?? throw new KeyNotFoundException("Question not found.");

            if (question.CategoryId != mockTest.CategoryId)
                throw new InvalidOperationException("Question does not belong to this mock test category.");
            if (await _db.UserAnswers.AnyAsync(ua => ua.MockTestId == dto.MockTestId && ua.QuestionId == dto.QuestionId))
                throw new InvalidOperationException("This question has already been answered.");
            if (string.IsNullOrWhiteSpace(dto.UserAnswer))
                throw new InvalidOperationException("Answer is required.");

            AnswerResultDTO result;
            UserAnswer userAnswer;

            if (question.IsMCQ)
            {
                var selectedAnswer = dto.UserAnswer.Trim().ToUpperInvariant();
                if (!new[] { "A", "B", "C", "D" }.Contains(selectedAnswer))
                    throw new InvalidOperationException("MCQ answer must be A, B, C, or D.");

                var correctAnswer = question.CorrectAnswer.Trim().ToUpperInvariant();
                var isCorrect = selectedAnswer == correctAnswer;
                var score = isCorrect ? 10 : 0;
                var feedback = isCorrect
                    ? "Correct! Well done."
                    : $"Incorrect. The correct answer is {correctAnswer}.";
                var suggestions = isCorrect
                    ? "Great job! Move on to the next question."
                    : $"Review this concept: {GetOptionText(question, correctAnswer)}";

                userAnswer = new UserAnswer
                {
                    MockTestId     = dto.MockTestId,
                    QuestionId     = dto.QuestionId,
                    UserAnswerText = selectedAnswer,
                    AIScore        = score,
                    AIFeedback     = feedback,
                    AISuggestions  = suggestions
                };
                result = new AnswerResultDTO
                {
                    QuestionText   = question.QuestionText,
                    UserAnswer     = selectedAnswer,
                    AIScore        = score,
                    AIFeedback     = feedback,
                    AISuggestions  = suggestions,
                    IsMCQ          = true,
                    IsCorrect      = isCorrect,
                    CorrectAnswer  = correctAnswer,
                    OptionA        = question.OptionA,
                    OptionB        = question.OptionB,
                    OptionC        = question.OptionC,
                    OptionD        = question.OptionD
                };
            }
            else
            {
                var evaluation = await _ai.EvaluateAnswerAsync(new AIEvaluationRequestDTO
                {
                    Question        = question.QuestionText,
                    UserAnswer      = dto.UserAnswer,
                    CorrectAnswer   = question.CorrectAnswer,
                    DifficultyLevel = question.DifficultyLevel
                });

                userAnswer = new UserAnswer
                {
                    MockTestId     = dto.MockTestId,
                    QuestionId     = dto.QuestionId,
                    UserAnswerText = dto.UserAnswer,
                    AIScore        = evaluation.Score,
                    AIFeedback     = evaluation.Feedback,
                    AISuggestions  = evaluation.Suggestions
                };
                result = new AnswerResultDTO
                {
                    QuestionText   = question.QuestionText,
                    UserAnswer     = dto.UserAnswer,
                    AIScore        = evaluation.Score,
                    AIFeedback     = evaluation.Feedback,
                    AISuggestions  = evaluation.Suggestions,
                    IsMCQ          = false,
                    IsCorrect      = evaluation.Score >= 7
                };
            }

            _db.UserAnswers.Add(userAnswer);
            mockTest.Score += userAnswer.AIScore;

            // Check if test is complete
            var answeredCount = await _db.UserAnswers.CountAsync(ua => ua.MockTestId == dto.MockTestId);
            if (answeredCount + 1 >= mockTest.TotalQuestions)
                mockTest.Status = "Completed";

            await _db.SaveChangesAsync();
            return result;
        }

        public async Task<MockTestResultDTO> GetResultAsync(int userId, int mockTestId)
        {
            var mockTest = await _db.MockTests
                .Include(m => m.Category)
                .Include(m => m.UserAnswers).ThenInclude(ua => ua.Question)
                .FirstOrDefaultAsync(m => m.Id == mockTestId && m.UserId == userId)
                ?? throw new KeyNotFoundException("Mock test not found.");

            var maxScore  = mockTest.TotalQuestions * 10;
            var pct       = maxScore > 0 ? Math.Round((double)mockTest.Score / maxScore * 100, 1) : 0;
            var level     = pct >= 80 ? "Excellent" : pct >= 60 ? "Good" : pct >= 40 ? "Average" : "Needs Improvement";

            return new MockTestResultDTO
            {
                MockTestId       = mockTest.Id,
                CategoryName     = mockTest.Category?.Name ?? "",
                Score            = mockTest.Score,
                TotalQuestions   = mockTest.TotalQuestions,
                Percentage       = pct,
                PerformanceLevel = level,
                CompletedAt      = mockTest.CreatedDate,
                Answers          = mockTest.UserAnswers.Select(ua => new AnswerResultDTO
                {
                    QuestionText  = ua.Question?.QuestionText ?? "",
                    UserAnswer    = ua.UserAnswerText,
                    AIScore       = ua.AIScore,
                    AIFeedback    = ua.AIFeedback,
                    AISuggestions = ua.AISuggestions,
                    IsMCQ         = ua.Question?.IsMCQ ?? false,
                    IsCorrect     = ua.Question?.IsMCQ == true ? ua.AIScore == 10 : ua.AIScore >= 7,
                    CorrectAnswer = ua.Question?.IsMCQ == true ? ua.Question.CorrectAnswer : string.Empty,
                    OptionA       = ua.Question?.OptionA ?? string.Empty,
                    OptionB       = ua.Question?.OptionB ?? string.Empty,
                    OptionC       = ua.Question?.OptionC ?? string.Empty,
                    OptionD       = ua.Question?.OptionD ?? string.Empty
                }).ToList()
            };
        }

        public async Task<List<TestHistoryDTO>> GetHistoryAsync(int userId) =>
            await _db.MockTests
                .Include(m => m.Category)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedDate)
                .Select(m => new TestHistoryDTO
                {
                    MockTestId      = m.Id,
                    CategoryName    = m.Category!.Name,
                    Score           = m.Score,
                    TotalQuestions  = m.TotalQuestions,
                    Percentage      = m.TotalQuestions > 0
                                      ? Math.Round((double)m.Score / (m.TotalQuestions * 10) * 100, 1)
                                      : 0,
                    Date            = m.CreatedDate
                }).ToListAsync();

        private static string GetOptionText(Question question, string option) => option switch
        {
            "A" => question.OptionA,
            "B" => question.OptionB,
            "C" => question.OptionC,
            "D" => question.OptionD,
            _ => string.Empty
        };
    }

    // ============================================================
    //  DASHBOARD SERVICE
    // ============================================================
    public interface IDashboardService
    {
        Task<DashboardDTO> GetDashboardAsync(int userId);
    }

    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _db;

        public DashboardService(ApplicationDbContext db) => _db = db;

        public async Task<DashboardDTO> GetDashboardAsync(int userId)
        {
            var tests = await _db.MockTests
                .Include(m => m.Category)
                .Where(m => m.UserId == userId && m.Status == "Completed")
                .ToListAsync();

            if (!tests.Any())
                return new DashboardDTO { RecentTests = new(), CategoryPerformance = new() };

            var avgScore      = tests.Average(t => t.TotalQuestions > 0 ? (double)t.Score / (t.TotalQuestions * 10) * 100 : 0);
            var bestScore     = tests.Max(t => t.TotalQuestions > 0 ? (int)((double)t.Score / (t.TotalQuestions * 10) * 100) : 0);
            var favCategory   = tests.GroupBy(t => t.Category?.Name ?? "")
                                     .OrderByDescending(g => g.Count())
                                     .First().Key;

            var recentTests = tests.OrderByDescending(t => t.CreatedDate).Take(5)
                .Select(t => new TestHistoryDTO
                {
                    MockTestId     = t.Id,
                    CategoryName   = t.Category?.Name ?? "",
                    Score          = t.Score,
                    TotalQuestions = t.TotalQuestions,
                    Percentage     = t.TotalQuestions > 0 ? Math.Round((double)t.Score / (t.TotalQuestions * 10) * 100, 1) : 0,
                    Date           = t.CreatedDate
                }).ToList();

            var categoryPerf = tests
                .GroupBy(t => t.Category?.Name ?? "")
                .Select(g => new CategoryPerformanceDTO
                {
                    CategoryName = g.Key,
                    TestCount    = g.Count(),
                    AverageScore = Math.Round(g.Average(t => t.TotalQuestions > 0 ? (double)t.Score / (t.TotalQuestions * 10) * 100 : 0), 1)
                }).ToList();

            return new DashboardDTO
            {
                TotalTests           = tests.Count,
                AverageScore         = Math.Round(avgScore, 1),
                BestScore            = bestScore,
                FavoriteCategory     = favCategory,
                RecentTests          = recentTests,
                CategoryPerformance  = categoryPerf
            };
        }
    }
}
