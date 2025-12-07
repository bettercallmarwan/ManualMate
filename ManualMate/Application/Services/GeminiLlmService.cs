using ManualMate.API.Controllers.Responses;
using ManualMate.Application.Interfaces;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ManualMate.Application.Services
{
    public class GeminiLlmService(IConfiguration configuration) : ILlmService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string API_TOKEN = configuration["Gemini:GeminiToken"]!;
        private readonly string MODEL_URL = configuration["Gemini:ModelUrl"]!;

        public async Task<Result<string>> GenerateAnswerAsync(string context, string question)
        {
            try
            {
                var prompt = GeneratePrompt(context, question);

                var response = await GetGeminiResponse(prompt);

                string json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (TryExtractGeminiErrorMessage(json, out var message))
                    {
                        return Result<string>.Fail(message, response.StatusCode);
                    }

                    return Result<string>.Fail("Error Generating Answer.", response.StatusCode);
                }

                if (TryExtractGeminiAnswer(json, out string textResponse))
                {
                    return Result<string>.Ok(textResponse);
                }

                return Result<string>.Fail("Unexpected behaviour from Gemini Json response", response.StatusCode);
            }
            catch (Exception ex)
            {
                return Result<string>.Fail("Unexpected error: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        private async Task<HttpResponseMessage> GetGeminiResponse(string prompt)
        {
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{MODEL_URL}{API_TOKEN}", content);

            return response;
        }
        private static bool TryExtractGeminiAnswer(string json, out string result)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var text = root
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                result = text!;
                return true;
            }
            catch
            {
                result = "";
                return false;
            }
        }
        private static bool TryExtractGeminiErrorMessage(string json, out string message)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                message = doc.RootElement
                    .GetProperty("error")
                    .GetProperty("message")
                    .GetString()!;

                return true;
            }
            catch
            {
                message = "";
                return false;
            }
        }
        private static string GeneratePrompt(string context, string question)
        {
            var prompt = $"""
                STRICT RAG MODE: Answer ONLY if the information is in the context. Otherwise, reject and Only say this exactly : Can't asnwer this question.

                CONTEXT:
                {context}

                QUESTION: {question}

                DECISION:
                - If CONTEXT contains answer to QUESTION: Provide the exact answer
                - If CONTEXT does not contain  answer: Say "I cannot answer this question based on the available information"

                RESPONSE:
                """;

            return prompt;
        }
    }
}