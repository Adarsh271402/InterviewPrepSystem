using InterviewPrepSystem.DTOs;
using System.Text;
using System.Text.Json;

namespace InterviewPrepSystem.AI
{
    // ============================================================
    //  INTERFACE
    // ============================================================
    public interface IAIService
    {
        Task<AIEvaluationResponseDTO> EvaluateAnswerAsync(AIEvaluationRequestDTO request);
        Task<AIQuestionGenerationResponseDTO> GenerateQuestionsAsync(AIQuestionGenerationRequestDTO request);
        Task<AIMCQGenerationResponseDTO> GenerateMCQsAsync(AIMCQGenerationRequestDTO request);
        Task<string> GetCareerSuggestionsAsync(string userProfile);
    }

    internal static class MCQSupport
    {
        public static string BuildPrompt(AIMCQGenerationRequestDTO request) =>
            "You are an expert technical interviewer. Generate " + request.NumberOfQuestions + " multiple-choice questions.\n" +
            "Topic: " + request.CategoryName + "\n" +
            "Difficulty: " + request.DifficultyLevel + "\n" +
            "Audience: " + (request.TargetAudience ?? "freshers") + "\n\n" +
            "Each question must have exactly four plausible options and exactly one correct answer. " +
            "correctAnswer must be only A, B, C, or D. Return ONLY valid JSON in this shape:\n" +
            "{\"questions\":[{\"questionText\":\"...\",\"optionA\":\"...\",\"optionB\":\"...\",\"optionC\":\"...\",\"optionD\":\"...\",\"correctAnswer\":\"A\",\"explanation\":\"...\",\"difficultyLevel\":\"Medium\"}]}";

        public static AIMCQGenerationResponseDTO Parse(string json)
        {
            json = json.Trim();
            if (json.StartsWith("```"))
                json = json[json.IndexOf('{')..(json.LastIndexOf('}') + 1)];

            using var doc = JsonDocument.Parse(json);
            var result = new AIMCQGenerationResponseDTO();
            foreach (var q in doc.RootElement.GetProperty("questions").EnumerateArray())
            {
                var correctAnswer = q.GetProperty("correctAnswer").GetString()?.Trim().ToUpperInvariant() ?? "";
                if (!new[] { "A", "B", "C", "D" }.Contains(correctAnswer))
                    continue;

                result.Questions.Add(new GeneratedMCQDTO
                {
                    QuestionText = q.GetProperty("questionText").GetString() ?? "",
                    OptionA = q.GetProperty("optionA").GetString() ?? "",
                    OptionB = q.GetProperty("optionB").GetString() ?? "",
                    OptionC = q.GetProperty("optionC").GetString() ?? "",
                    OptionD = q.GetProperty("optionD").GetString() ?? "",
                    CorrectAnswer = correctAnswer,
                    Explanation = q.TryGetProperty("explanation", out var explanation) ? explanation.GetString() ?? "" : "",
                    DifficultyLevel = q.TryGetProperty("difficultyLevel", out var difficulty) ? difficulty.GetString() ?? "Medium" : "Medium"
                });
            }
            return result;
        }
    }

    // ============================================================
    //  GROQ SERVICE  (Free, Fast, Recommended)
    //  Model is configured under AISettings:Groq:Model.
    // ============================================================
    public class GroqService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<GroqService> _logger;

        private string ApiKey => _config["AISettings:Groq:ApiKey"]
            ?? throw new InvalidOperationException("Groq API Key missing in appsettings.json");
        private string Model => _config["AISettings:Groq:Model"] ?? "openai/gpt-oss-20b";
        private string BaseUrl => _config["AISettings:Groq:BaseUrl"] ?? "https://api.groq.com/openai/v1/chat/completions";

        public GroqService(HttpClient httpClient, IConfiguration config, ILogger<GroqService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        // ----------------------------------------------------------
        // 1. EVALUATE ANSWER
        // ----------------------------------------------------------
        public async Task<AIEvaluationResponseDTO> EvaluateAnswerAsync(AIEvaluationRequestDTO request)
        {
            var correctPart = string.IsNullOrEmpty(request.CorrectAnswer)
                ? "" : "Reference Answer: " + request.CorrectAnswer + "\n";

            var prompt =
                "You are an expert technical interview evaluator.\n\n" +
                "Question      : " + request.Question + "\n" +
                "Difficulty    : " + request.DifficultyLevel + "\n" +
                "User Answer   : " + request.UserAnswer + "\n" +
                correctPart + "\n" +
                "Return ONLY valid JSON, no markdown, no explanation:\n" +
                "{\n" +
                "  \"score\": 7,\n" +
                "  \"feedback\": \"detailed feedback here\",\n" +
                "  \"suggestions\": \"improvement suggestions here\",\n" +
                "  \"grammarFeedback\": \"grammar feedback here\",\n" +
                "  \"technicalAccuracy\": \"technical accuracy here\"\n" +
                "}";

            var text = await CallAsync(prompt);
            return ParseEvaluation(text);
        }

        // ----------------------------------------------------------
        // 2. GENERATE QUESTIONS
        // ----------------------------------------------------------
        public async Task<AIQuestionGenerationResponseDTO> GenerateQuestionsAsync(AIQuestionGenerationRequestDTO request)
        {
            var prompt =
                "You are an expert technical interviewer.\n" +
                "Generate " + request.NumberOfQuestions + " interview questions.\n" +
                "Topic      : " + request.CategoryName + "\n" +
                "Difficulty : " + request.DifficultyLevel + "\n" +
                "Audience   : " + (request.TargetAudience ?? "freshers") + "\n\n" +
                "Return ONLY valid JSON, no markdown, no explanation:\n" +
                "{\n" +
                "  \"questions\": [\n" +
                "    {\n" +
                "      \"questionText\": \"the interview question\",\n" +
                "      \"suggestedAnswer\": \"the ideal answer\",\n" +
                "      \"difficultyLevel\": \"Medium\"\n" +
                "    }\n" +
                "  ]\n" +
                "}";

            var text = await CallAsync(prompt);
            return ParseGeneration(text);
        }

        public async Task<AIMCQGenerationResponseDTO> GenerateMCQsAsync(AIMCQGenerationRequestDTO request)
        {
            var text = await CallAsync(MCQSupport.BuildPrompt(request));
            return MCQSupport.Parse(text);
        }

        // ----------------------------------------------------------
        // 3. CAREER SUGGESTIONS
        // ----------------------------------------------------------
        public async Task<string> GetCareerSuggestionsAsync(string userProfile)
        {
            var prompt =
                "You are a career coach. Give 5 specific, actionable career improvement tips " +
                "based on this interview performance:\n\n" + userProfile + "\n\n" +
                "Format as a numbered list. Keep each tip concise.";

            return await CallAsync(prompt);
        }

        // ----------------------------------------------------------
        // CORE: Call Groq API (same format as OpenAI)
        // ----------------------------------------------------------
        private async Task<string> CallAsync(string prompt)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + ApiKey);

                var body = new
                {
                    model = Model,
                    messages = new[]
                    {
                        new { role = "system", content = "You are a professional interview coach and evaluator. Always respond with valid JSON when asked." },
                        new { role = "user",   content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 1024
                };

                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(BaseUrl, content);
                var respBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Groq API error {Status}: {Body}", response.StatusCode, respBody);
                    throw new InvalidOperationException(
                        response.StatusCode == System.Net.HttpStatusCode.TooManyRequests
                            ? "Rate limit reached. Please wait a moment and try again."
                            : response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                            ? "Invalid Groq API key. Get a free key at console.groq.com"
                            : "Groq API error " + (int)response.StatusCode + ": " + respBody);
                }

                using var doc = JsonDocument.Parse(respBody);
                return doc.RootElement
                          .GetProperty("choices")[0]
                          .GetProperty("message")
                          .GetProperty("content")
                          .GetString() ?? "";
            }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Groq API call failed");
                throw new InvalidOperationException("Groq service error: " + ex.Message);
            }
        }

        // ----------------------------------------------------------
        // PARSERS
        // ----------------------------------------------------------
        private AIEvaluationResponseDTO ParseEvaluation(string json)
        {
            try
            {
                json = CleanJson(json);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                return new AIEvaluationResponseDTO
                {
                    Score = root.GetProperty("score").GetInt32(),
                    Feedback = root.GetProperty("feedback").GetString() ?? "",
                    Suggestions = root.GetProperty("suggestions").GetString() ?? "",
                    GrammarFeedback = root.GetProperty("grammarFeedback").GetString() ?? "",
                    TechnicalAccuracy = root.GetProperty("technicalAccuracy").GetString() ?? ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Parse failed. Raw: {Json}", json);
                return new AIEvaluationResponseDTO { Score = 5, Feedback = json, Suggestions = "Review official docs." };
            }
        }

        private AIQuestionGenerationResponseDTO ParseGeneration(string json)
        {
            try
            {
                json = CleanJson(json);
                using var doc = JsonDocument.Parse(json);
                var result = new AIQuestionGenerationResponseDTO();
                foreach (var q in doc.RootElement.GetProperty("questions").EnumerateArray())
                    result.Questions.Add(new GeneratedQuestionDTO
                    {
                        QuestionText = q.GetProperty("questionText").GetString() ?? "",
                        SuggestedAnswer = q.GetProperty("suggestedAnswer").GetString() ?? "",
                        DifficultyLevel = q.GetProperty("difficultyLevel").GetString() ?? "Medium"
                    });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Parse failed. Raw: {Json}", json);
                return new AIQuestionGenerationResponseDTO();
            }
        }

        private static string CleanJson(string json)
        {
            json = json.Trim();
            if (json.StartsWith("```"))
                json = json[json.IndexOf('{')..(json.LastIndexOf('}') + 1)];
            return json;
        }
    }

    // ============================================================
    //  OPENAI SERVICE
    // ============================================================
    public class OpenAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<OpenAIService> _logger;

        private string ApiKey => _config["AISettings:OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API Key missing in appsettings.json");
        private string Model => _config["AISettings:OpenAI:Model"] ?? "gpt-3.5-turbo";
        private string BaseUrl => "https://api.openai.com/v1/chat/completions";

        public OpenAIService(HttpClient httpClient, IConfiguration config, ILogger<OpenAIService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<AIEvaluationResponseDTO> EvaluateAnswerAsync(AIEvaluationRequestDTO request)
        {
            var correctPart = string.IsNullOrEmpty(request.CorrectAnswer)
                ? "" : "Reference Answer: " + request.CorrectAnswer + "\n";

            var prompt =
                "You are an expert technical interview evaluator.\n\n" +
                "Question: " + request.Question + "\n" +
                "Difficulty: " + request.DifficultyLevel + "\n" +
                "User Answer: " + request.UserAnswer + "\n" +
                correctPart + "\n" +
                "Return ONLY valid JSON:\n" +
                "{\"score\":7,\"feedback\":\"...\",\"suggestions\":\"...\",\"grammarFeedback\":\"...\",\"technicalAccuracy\":\"...\"}";

            var text = await CallAsync(prompt);
            return ParseEvaluation(text);
        }

        public async Task<AIQuestionGenerationResponseDTO> GenerateQuestionsAsync(AIQuestionGenerationRequestDTO request)
        {
            var prompt =
                "Generate " + request.NumberOfQuestions + " " + request.DifficultyLevel +
                " " + request.CategoryName + " interview questions for " + (request.TargetAudience ?? "freshers") + ".\n" +
                "Return ONLY valid JSON:\n" +
                "{\"questions\":[{\"questionText\":\"...\",\"suggestedAnswer\":\"...\",\"difficultyLevel\":\"Medium\"}]}";

            var text = await CallAsync(prompt);
            return ParseGeneration(text);
        }

        public async Task<AIMCQGenerationResponseDTO> GenerateMCQsAsync(AIMCQGenerationRequestDTO request)
        {
            var text = await CallAsync(MCQSupport.BuildPrompt(request));
            return MCQSupport.Parse(text);
        }

        public async Task<string> GetCareerSuggestionsAsync(string userProfile)
            => await CallAsync("Give 5 career tips for: " + userProfile);

        private async Task<string> CallAsync(string prompt)
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + ApiKey);

            var body = new { model = Model, messages = new[] { new { role = "user", content = prompt } }, max_tokens = 1024 };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var resp = await _httpClient.PostAsync(BaseUrl, content);
            var respStr = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException("OpenAI error " + (int)resp.StatusCode);

            using var doc = JsonDocument.Parse(respStr);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        }

        private AIEvaluationResponseDTO ParseEvaluation(string json)
        {
            try
            {
                json = CleanJson(json);
                using var doc = JsonDocument.Parse(json);
                var r = doc.RootElement;
                return new AIEvaluationResponseDTO
                {
                    Score = r.GetProperty("score").GetInt32(),
                    Feedback = r.GetProperty("feedback").GetString() ?? "",
                    Suggestions = r.GetProperty("suggestions").GetString() ?? "",
                    GrammarFeedback = r.GetProperty("grammarFeedback").GetString() ?? "",
                    TechnicalAccuracy = r.GetProperty("technicalAccuracy").GetString() ?? ""
                };
            }
            catch { return new AIEvaluationResponseDTO { Score = 5, Feedback = json }; }
        }

        private AIQuestionGenerationResponseDTO ParseGeneration(string json)
        {
            try
            {
                json = CleanJson(json);
                using var doc = JsonDocument.Parse(json);
                var result = new AIQuestionGenerationResponseDTO();
                foreach (var q in doc.RootElement.GetProperty("questions").EnumerateArray())
                    result.Questions.Add(new GeneratedQuestionDTO
                    {
                        QuestionText = q.GetProperty("questionText").GetString() ?? "",
                        SuggestedAnswer = q.GetProperty("suggestedAnswer").GetString() ?? "",
                        DifficultyLevel = q.GetProperty("difficultyLevel").GetString() ?? "Medium"
                    });
                return result;
            }
            catch { return new AIQuestionGenerationResponseDTO(); }
        }

        private static string CleanJson(string json)
        {
            json = json.Trim();
            if (json.StartsWith("```"))
                json = json[json.IndexOf('{')..(json.LastIndexOf('}') + 1)];
            return json;
        }
    }

    // ============================================================
    //  GEMINI SERVICE  (Backup)
    // ============================================================
    public class GeminiService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<GeminiService> _logger;

        private string ApiKey => _config["AISettings:Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini API Key missing in appsettings.json");
        private string BuildUrl() =>
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=" + ApiKey;

        public GeminiService(HttpClient httpClient, IConfiguration config, ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<AIEvaluationResponseDTO> EvaluateAnswerAsync(AIEvaluationRequestDTO request)
        {
            var prompt =
                "Evaluate this interview answer. Return ONLY JSON:\n" +
                "Question: " + request.Question + "\nAnswer: " + request.UserAnswer + "\n" +
                "{\"score\":7,\"feedback\":\"...\",\"suggestions\":\"...\",\"grammarFeedback\":\"...\",\"technicalAccuracy\":\"...\"}";
            var text = await CallAsync(prompt);
            return ParseEvaluation(text);
        }

        public async Task<AIQuestionGenerationResponseDTO> GenerateQuestionsAsync(AIQuestionGenerationRequestDTO request)
        {
            var prompt =
                "Generate " + request.NumberOfQuestions + " " + request.CategoryName + " interview questions. Return ONLY JSON:\n" +
                "{\"questions\":[{\"questionText\":\"...\",\"suggestedAnswer\":\"...\",\"difficultyLevel\":\"Medium\"}]}";
            var text = await CallAsync(prompt);
            return ParseGeneration(text);
        }

        public async Task<AIMCQGenerationResponseDTO> GenerateMCQsAsync(AIMCQGenerationRequestDTO request)
        {
            var text = await CallAsync(MCQSupport.BuildPrompt(request));
            return MCQSupport.Parse(text);
        }

        public async Task<string> GetCareerSuggestionsAsync(string userProfile)
            => await CallAsync("Give 5 career tips for: " + userProfile);

        private async Task<string> CallAsync(string prompt)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var body = new { contents = new[] { new { role = "user", parts = new[] { new { text = prompt } } } } };
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var resp = await _httpClient.PostAsync(BuildUrl(), content);
                var respStr = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    throw new InvalidOperationException(
                        resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests
                            ? "Gemini rate limit. Wait 60 seconds or switch to Groq."
                            : "Gemini error " + (int)resp.StatusCode);

                using var doc = JsonDocument.Parse(respStr);
                return doc.RootElement
                          .GetProperty("candidates")[0]
                          .GetProperty("content")
                          .GetProperty("parts")[0]
                          .GetProperty("text")
                          .GetString() ?? "";
            }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex) { throw new InvalidOperationException("Gemini error: " + ex.Message); }
        }

        private AIEvaluationResponseDTO ParseEvaluation(string json)
        {
            try
            {
                json = CleanJson(json);
                using var doc = JsonDocument.Parse(json);
                var r = doc.RootElement;
                return new AIEvaluationResponseDTO
                {
                    Score = r.GetProperty("score").GetInt32(),
                    Feedback = r.GetProperty("feedback").GetString() ?? "",
                    Suggestions = r.GetProperty("suggestions").GetString() ?? "",
                    GrammarFeedback = r.GetProperty("grammarFeedback").GetString() ?? "",
                    TechnicalAccuracy = r.GetProperty("technicalAccuracy").GetString() ?? ""
                };
            }
            catch { return new AIEvaluationResponseDTO { Score = 5, Feedback = json }; }
        }

        private AIQuestionGenerationResponseDTO ParseGeneration(string json)
        {
            try
            {
                json = CleanJson(json);
                using var doc = JsonDocument.Parse(json);
                var result = new AIQuestionGenerationResponseDTO();
                foreach (var q in doc.RootElement.GetProperty("questions").EnumerateArray())
                    result.Questions.Add(new GeneratedQuestionDTO
                    {
                        QuestionText = q.GetProperty("questionText").GetString() ?? "",
                        SuggestedAnswer = q.GetProperty("suggestedAnswer").GetString() ?? "",
                        DifficultyLevel = q.GetProperty("difficultyLevel").GetString() ?? "Medium"
                    });
                return result;
            }
            catch { return new AIQuestionGenerationResponseDTO(); }
        }

        private static string CleanJson(string json)
        {
            json = json.Trim();
            if (json.StartsWith("```"))
                json = json[json.IndexOf('{')..(json.LastIndexOf('}') + 1)];
            return json;
        }
    }
}
