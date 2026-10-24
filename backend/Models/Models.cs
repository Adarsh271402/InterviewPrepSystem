// ===== Models/User.cs =====
namespace InterviewPrepSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // "User" or "Admin"
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<MockTest> MockTests { get; set; } = new List<MockTest>();
    }
}

// ===== Models/Category.cs =====
namespace InterviewPrepSystem.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<MockTest> MockTests { get; set; } = new List<MockTest>();
    }
}

// ===== Models/Question.cs =====
namespace InterviewPrepSystem.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = "Medium"; // Easy, Medium, Hard
        public string CorrectAnswer { get; set; } = string.Empty;
        public bool IsAIGenerated { get; set; } = false;
        public bool IsMCQ { get; set; } = false;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;

        // Navigation
        public Category? Category { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}

// ===== Models/MockTest.cs =====
namespace InterviewPrepSystem.Models
{
    public class MockTest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public string Status { get; set; } = "InProgress"; // InProgress, Completed
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
        public Category? Category { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}

// ===== Models/UserAnswer.cs =====
namespace InterviewPrepSystem.Models
{
    public class UserAnswer
    {
        public int Id { get; set; }
        public int MockTestId { get; set; }
        public int QuestionId { get; set; }
        public string UserAnswerText { get; set; } = string.Empty;
        public int AIScore { get; set; }
        public string AIFeedback { get; set; } = string.Empty;
        public string AISuggestions { get; set; } = string.Empty;
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public MockTest? MockTest { get; set; }
        public Question? Question { get; set; }
    }
}
