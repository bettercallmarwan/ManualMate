using ManualMate.API.Controllers.Responses;
using ManualMate.Application.Interfaces.Services;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ManualMate.Application.Services
{
    public class GeminiLlmService(IHttpClientFactory clientFactory) : ILlmService
    {
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

            var client = clientFactory.CreateClient("GeminiClient");
            return await client.PostAsync("", content);
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
            var prompt = $""""
                You are a helpful and precise technical assistant. Your task is to answer the user's question strictly based on the provided context.

                GUIDELINES:
                1. You must answer the question using ONLY the information found in the "Context" section below.
                2. Do not use your internal knowledge, outside facts, or general training data.
                3. If the answer is not explicitly contained within the Context, you must reply: "I cannot answer this question based on the provided documents."
                4. Do not speculate or make up information.
                5. If the context contains conflicting information, mention the conflict in your answer.
                6. Format your answer using Markdown (e.g., bullet points, bold text) for readability.
                7. Keep the tone professional, concise, and direct.

                CONTEXT:
                """
                {context}
                """

                USER QUESTION:
                """
                {question}
                """
                """";

            return prompt;
        }
    }
}