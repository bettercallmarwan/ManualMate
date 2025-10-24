using ManualMate.Interfaces;
using System.Text.Json;

namespace ManualMate.Services
{

    public class GeminiLlmService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiToken;
        private readonly ILogger<GeminiLlmService> _logger;

        private const string MODEL_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key=";

        public GeminiLlmService(ILogger<GeminiLlmService> logger, IConfiguration configuration)
        {
            _apiToken = configuration["Gemini:GeminiToken"]!;
            if (string.IsNullOrEmpty(_apiToken))
                throw new Exception("can't configure model api token");

            _httpClient = new HttpClient();

            _logger = logger;
        }

        public async Task<string> GenerateAnswerAsync(string context, string question)
        {
            try
            {
                var prompt = $"""
                STRICT RAG MODE: Answer ONLY if the exact information is in the context. Otherwise, reject and Only say this exactly : Cant asnwer this question.

                CONTEXT:
                {context}

                QUESTION: {question}

                DECISION:
                - If CONTEXT contains direct answer to QUESTION: Provide the exact answer
                - If CONTEXT does not contain direct answer: Say "I cannot answer this question based on the available information"

                RESPONSE:
                """;

                var payload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[] { new { text = prompt } }
                        }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync( $"{MODEL_URL}{_apiToken}", payload);

                string result = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(result);
                string output = "Sorry, can't generate answer";
                if (doc.RootElement.TryGetProperty("candidates", out var candidates))
                {
                    output = candidates[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString() ?? "Sorry, can't generate answer";

                }
                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error generating answer");
                return "Sorry, can't generate answer";
            }
        }
    }
}

