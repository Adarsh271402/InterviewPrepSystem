using InterviewPrepSystem.AI;
using InterviewPrepSystem.DTOs;
using InterviewPrepSystem.Helpers;
using InterviewPrepSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewPrepSystem.Controllers
{
    // ============================================================
    //  AUTH CONTROLLER
    // ============================================================
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        /// <summary>Register a new user</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            try
            {
                var result = await _auth.RegisterAsync(dto);
                return Ok(ApiResponseDTO<AuthResponseDTO>.Ok(result, "Registration successful"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponseDTO<string>.Fail(ex.Message));
            }
        }

        /// <summary>Login and get JWT token</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            try
            {
                var result = await _auth.LoginAsync(dto);
                return Ok(ApiResponseDTO<AuthResponseDTO>.Ok(result, "Login successful"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseDTO<string>.Fail(ex.Message));
            }
        }
    }

    // ============================================================
    //  CATEGORY CONTROLLER
    // ============================================================
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service) => _service = service;

        /// <summary>Get all interview categories</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(ApiResponseDTO<List<CategoryDTO>>.Ok(data));
        }

        /// <summary>Get a category by ID</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound(ApiResponseDTO<string>.Fail("Category not found"));
            return Ok(ApiResponseDTO<CategoryDTO>.Ok(data));
        }

        /// <summary>Add a new category (Admin only)</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDTO dto)
        {
            var data = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = data.Id },
                ApiResponseDTO<CategoryDTO>.Ok(data, "Category created"));
        }

        /// <summary>Update a category (Admin only)</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDTO dto)
        {
            var data = await _service.UpdateAsync(id, dto);
            if (data == null) return NotFound(ApiResponseDTO<string>.Fail("Category not found"));
            return Ok(ApiResponseDTO<CategoryDTO>.Ok(data, "Category updated"));
        }

        /// <summary>Delete a category (Admin only)</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound(ApiResponseDTO<string>.Fail("Category not found"));
            return Ok(ApiResponseDTO<string>.Ok("Deleted", "Category deleted"));
        }
    }

    // ============================================================
    //  QUESTION CONTROLLER
    // ============================================================
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _service;

        public QuestionController(IQuestionService service) => _service = service;

        /// <summary>Get all questions, optionally filtered</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? categoryId, [FromQuery] string? difficulty)
        {
            var data = await _service.GetAllAsync(categoryId, difficulty);
            return Ok(ApiResponseDTO<List<QuestionDTO>>.Ok(data));
        }

        /// <summary>Get a question by ID</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound(ApiResponseDTO<string>.Fail("Question not found"));
            return Ok(ApiResponseDTO<QuestionDTO>.Ok(data));
        }

        /// <summary>Add a new question (Admin only)</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateQuestionDTO dto)
        {
            var data = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = data.Id },
                ApiResponseDTO<QuestionDTO>.Ok(data, "Question created"));
        }

        /// <summary>Update a question (Admin only)</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateQuestionDTO dto)
        {
            var data = await _service.UpdateAsync(id, dto);
            if (data == null) return NotFound(ApiResponseDTO<string>.Fail("Question not found"));
            return Ok(ApiResponseDTO<QuestionDTO>.Ok(data, "Question updated"));
        }

        /// <summary>Delete a question (Admin only)</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound(ApiResponseDTO<string>.Fail("Question not found"));
            return Ok(ApiResponseDTO<string>.Ok("Deleted", "Question deleted"));
        }
    }

    // ============================================================
    //  MOCK TEST CONTROLLER
    // ============================================================
    [ApiController]
    [Route("api/mocktest")]
    [Authorize]
    public class MockTestController : ControllerBase
    {
        private readonly IMockTestService _service;

        public MockTestController(IMockTestService service) => _service = service;

        /// <summary>Start a new mock interview session</summary>
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartMockTestDTO dto)
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _service.StartAsync(userId, dto);
                return Ok(ApiResponseDTO<MockTestResponseDTO>.Ok(result, "Mock test started"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseDTO<string>.Fail(ex.Message));
            }
        }

        /// <summary>Submit an answer — AI evaluates it instantly</summary>
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerDTO dto)
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _service.SubmitAnswerAsync(userId, dto);
                return Ok(ApiResponseDTO<AnswerResultDTO>.Ok(result, "Answer evaluated"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseDTO<string>.Fail(ex.Message));
            }
        }

        /// <summary>Get full result of a completed mock test</summary>
        [HttpGet("result/{mockTestId}")]
        public async Task<IActionResult> GetResult(int mockTestId)
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _service.GetResultAsync(userId, mockTestId);
                return Ok(ApiResponseDTO<MockTestResultDTO>.Ok(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponseDTO<string>.Fail(ex.Message));
            }
        }

        /// <summary>Get test history for current user</summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = User.GetUserId();
            var result = await _service.GetHistoryAsync(userId);
            return Ok(ApiResponseDTO<List<TestHistoryDTO>>.Ok(result));
        }
    }

    // ============================================================
    //  AI CONTROLLER (Direct AI access endpoints)
    // ============================================================
    [ApiController]
    [Route("api/ai")]
    [Authorize]
    public class AIController : ControllerBase
    {
        private readonly IAIService _ai;
        private readonly IQuestionService _questions;
        private readonly Data.ApplicationDbContext _db;

        public AIController(IAIService ai, IQuestionService questions, Data.ApplicationDbContext db)
        {
            _ai        = ai;
            _questions = questions;
            _db        = db;
        }

        /// <summary>Evaluate a single answer using AI</summary>
        [HttpPost("evaluate")]
        public async Task<IActionResult> Evaluate([FromBody] AIEvaluationRequestDTO dto)
        {
            try
            {
                var result = await _ai.EvaluateAnswerAsync(dto);
                return Ok(ApiResponseDTO<AIEvaluationResponseDTO>.Ok(result, "AI evaluation complete"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseDTO<string>.Fail(ex.Message));
            }
        }

        /// <summary>Generate interview questions dynamically using AI</summary>
        [HttpPost("generate-questions")]
        public async Task<IActionResult> GenerateQuestions([FromBody] AIQuestionGenerationRequestDTO dto)
        {
            try
            {
                var result = await _ai.GenerateQuestionsAsync(dto);
                return Ok(ApiResponseDTO<AIQuestionGenerationResponseDTO>.Ok(result, $"Generated {result.Questions.Count} questions"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseDTO<string>.Fail(ex.Message));
            }
        }

        /// <summary>Generate multiple-choice questions dynamically using AI</summary>
        [HttpPost("generate-mcqs")]
        public async Task<IActionResult> GenerateMCQs([FromBody] AIMCQGenerationRequestDTO dto)
        {
            try
            {
                var result = await _ai.GenerateMCQsAsync(dto);
                return Ok(ApiResponseDTO<AIMCQGenerationResponseDTO>.Ok(result, $"Generated {result.Questions.Count} MCQs"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseDTO<string>.Fail(ex.Message));
            }
        }

        /// <summary>Generate questions and save them to the database</summary>
        [HttpPost("generate-and-save")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateAndSave([FromBody] AIQuestionGenerationRequestDTO dto, [FromQuery] int categoryId)
        {
            try
            {
                var generated = await _ai.GenerateQuestionsAsync(dto);
                var saved     = new List<QuestionDTO>();

                foreach (var q in generated.Questions)
                {
                    var created = await _questions.CreateAsync(new CreateQuestionDTO
                    {
                        CategoryId      = categoryId,
                        QuestionText    = q.QuestionText,
                        DifficultyLevel = q.DifficultyLevel,
                        CorrectAnswer   = q.SuggestedAnswer,
                        IsAIGenerated   = true
                    });
                    saved.Add(created);
                }

                return Ok(ApiResponseDTO<List<QuestionDTO>>.Ok(saved, $"{saved.Count} AI questions saved to database"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseDTO<string>.Fail(ex.Message));
            }
        }

        /// <summary>Generate MCQs and save them to the database</summary>
        [HttpPost("generate-and-save-mcqs")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateAndSaveMCQs([FromBody] AIMCQGenerationRequestDTO dto, [FromQuery] int categoryId)
        {
            try
            {
                var generated = await _ai.GenerateMCQsAsync(dto);
                var saved = new List<QuestionDTO>();

                foreach (var q in generated.Questions)
                {
                    var created = await _questions.CreateAsync(new CreateQuestionDTO
                    {
                        CategoryId = categoryId,
                        QuestionText = q.QuestionText,
                        DifficultyLevel = q.DifficultyLevel,
                        CorrectAnswer = q.CorrectAnswer,
                        IsAIGenerated = true,
                        IsMCQ = true,
                        OptionA = q.OptionA,
                        OptionB = q.OptionB,
                        OptionC = q.OptionC,
                        OptionD = q.OptionD
                    });
                    saved.Add(created);
                }

                return Ok(ApiResponseDTO<List<QuestionDTO>>.Ok(saved, $"{saved.Count} AI MCQs saved to database"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseDTO<string>.Fail(ex.Message));
            }
        }

        /// <summary>Get career improvement suggestions based on user profile</summary>
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions()
        {
            try
            {
                var userId = User.GetUserId();
                var tests  = await _db.MockTests
                    .Include(m => m.Category)
                    .Where(m => m.UserId == userId && m.Status == "Completed")
                    .OrderByDescending(m => m.CreatedDate)
                    .Take(5)
                    .ToListAsync();

                if (!tests.Any())
                    return Ok(ApiResponseDTO<string>.Ok("Complete at least one mock test to get personalized suggestions."));

                var profile = $"""
                    Tests Completed: {tests.Count}
                    Average Score: {tests.Average(t => t.TotalQuestions > 0 ? (double)t.Score / (t.TotalQuestions * 10) * 100 : 0):F1}%
                    Categories Attempted: {string.Join(", ", tests.Select(t => t.Category?.Name).Distinct())}
                    """;

                var suggestions = await _ai.GetCareerSuggestionsAsync(profile);
                return Ok(ApiResponseDTO<string>.Ok(suggestions, "Career suggestions generated"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseDTO<string>.Fail(ex.Message));
            }
        }
    }

    // ============================================================
    //  DASHBOARD CONTROLLER
    // ============================================================
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service) => _service = service;

        /// <summary>Get user performance dashboard with analytics</summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = User.GetUserId();
            var result = await _service.GetDashboardAsync(userId);
            return Ok(ApiResponseDTO<DashboardDTO>.Ok(result));
        }
    }
}
