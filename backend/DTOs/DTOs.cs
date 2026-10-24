// ===== DTOs/AuthDTOs.cs =====
namespace InterviewPrepSystem.DTOs
{
    // Auth
    public class RegisterDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    // Category
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
    }

    public class CreateCategoryDTO
    {
        public string Name { get; set; } = string.Empty;
    }

    // Question
    public class QuestionDTO
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public bool IsAIGenerated { get; set; }
        public bool IsMCQ { get; set; }
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
    }

    public class CreateQuestionDTO
    {
        public int CategoryId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = "Medium";
        public string CorrectAnswer { get; set; } = string.Empty;
        public bool IsAIGenerated { get; set; }
        public bool IsMCQ { get; set; }
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
    }

    public class UpdateQuestionDTO
    {
        public string QuestionText { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = "Medium";
        public string CorrectAnswer { get; set; } = string.Empty;
        public bool IsMCQ { get; set; }
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
    }

    // MockTest
    public class StartMockTestDTO
    {
        public int CategoryId { get; set; }
        public int NumberOfQuestions { get; set; } = 5;
        public string DifficultyLevel { get; set; } = "Mixed"; // Easy, Medium, Hard, Mixed
        public string QuestionType { get; set; } = "Text"; // MCQ, Text, Mixed
    }

    public class MockTestResponseDTO
    {
        public int MockTestId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<QuestionDTO> Questions { get; set; } = new();
        public DateTime StartedAt { get; set; }
    }

    public class SubmitAnswerDTO
    {
        public int MockTestId { get; set; }
        public int QuestionId { get; set; }
        public string UserAnswer { get; set; } = string.Empty;
    }

    public class MockTestResultDTO
    {
        public int MockTestId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public double Percentage { get; set; }
        public string PerformanceLevel { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
        public List<AnswerResultDTO> Answers { get; set; } = new();
    }

    public class AnswerResultDTO
    {
        public string QuestionText { get; set; } = string.Empty;
        public string UserAnswer { get; set; } = string.Empty;
        public int AIScore { get; set; }
        public string AIFeedback { get; set; } = string.Empty;
        public string AISuggestions { get; set; } = string.Empty;
        public bool IsMCQ { get; set; }
        public bool IsCorrect { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
    }

    // AI DTOs
    public class AIEvaluationRequestDTO
    {
        public string Question { get; set; } = string.Empty;
        public string UserAnswer { get; set; } = string.Empty;
        public string? CorrectAnswer { get; set; }
        public string DifficultyLevel { get; set; } = "Medium";
    }

    public class AIEvaluationResponseDTO
    {
        public int Score { get; set; }        // Score out of 10
        public string Feedback { get; set; } = string.Empty;
        public string Suggestions { get; set; } = string.Empty;
        public string GrammarFeedback { get; set; } = string.Empty;
        public string TechnicalAccuracy { get; set; } = string.Empty;
    }

    public class AIQuestionGenerationRequestDTO
    {
        public string CategoryName { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = "Medium";
        public int NumberOfQuestions { get; set; } = 5;
        public string? TargetAudience { get; set; } = "freshers";
    }

    public class AIQuestionGenerationResponseDTO
    {
        public List<GeneratedQuestionDTO> Questions { get; set; } = new();
    }

    public class GeneratedQuestionDTO
    {
        public string QuestionText { get; set; } = string.Empty;
        public string SuggestedAnswer { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
    }

    public class AIMCQGenerationRequestDTO
    {
        public string CategoryName { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = "Medium";
        public int NumberOfQuestions { get; set; } = 5;
        public string? TargetAudience { get; set; } = "freshers";
    }

    public class AIMCQGenerationResponseDTO
    {
        public List<GeneratedMCQDTO> Questions { get; set; } = new();
    }

    public class GeneratedMCQDTO
    {
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = "Medium";
    }

    // Dashboard
    public class DashboardDTO
    {
        public int TotalTests { get; set; }
        public double AverageScore { get; set; }
        public int BestScore { get; set; }
        public string FavoriteCategory { get; set; } = string.Empty;
        public List<TestHistoryDTO> RecentTests { get; set; } = new();
        public List<CategoryPerformanceDTO> CategoryPerformance { get; set; } = new();
    }

    public class TestHistoryDTO
    {
        public int MockTestId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public double Percentage { get; set; }
        public DateTime Date { get; set; }
    }

    public class CategoryPerformanceDTO
    {
        public string CategoryName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
        public int TestCount { get; set; }
    }

    public class ApiResponseDTO<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResponseDTO<T> Ok(T data, string message = "Success")
            => new() { Success = true, Message = message, Data = data };

        public static ApiResponseDTO<T> Fail(string message)
            => new() { Success = false, Message = message };
    }
}
