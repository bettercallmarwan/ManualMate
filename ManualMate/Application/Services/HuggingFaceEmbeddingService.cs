using ManualMate.API.Controllers.Responses;
using ManualMate.Application.Interfaces;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ManualMate.Application.Services
{
    public class HuggingFaceEmbeddingService(IConfiguration configuration) : IEmbeddingService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string API_TOKEN = configuration["HuggingFace:ApiToken"]!;
        private readonly string MODEL_URL = configuration["HuggingFace:ModelUrl"]!;

        public async Task<Result<double[]>> GetEmbeddingAsync(string text)
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_TOKEN}");

            try
            {
                var response = await GetHuggingFaceResponse(text);

                string json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if(TryExtractHuggingFaceErrorMessage(json, out var message))
                    {
                        return Result<double[]>.Fail(message, response.StatusCode);
                    }

                    return Result<double[]>.Fail("Error Generating Answer.", response.StatusCode);
                }

                if(TryExtractHuggingFaceAnswer(json, out var embeddingsResponse))
                {
                    return Result<double[]>.Ok(embeddingsResponse!);
                }

                return Result<double[]>.Fail("Unexpected behaviour from Hugging Face Json response", response.StatusCode);
            }
            catch (Exception ex)
            {
                return Result<double[]>.Fail("Unexpected error: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        private async Task<HttpResponseMessage> GetHuggingFaceResponse(string text)
        {
            var payload = new { inputs = text };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(MODEL_URL, content);

            return response;
        }

        private static bool TryExtractHuggingFaceAnswer(string json, out double[]? result)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var embeddings = JsonSerializer.Deserialize<double[]>(json);

                result = embeddings!;
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        private static bool TryExtractHuggingFaceErrorMessage(string json, out string message)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                message = doc.RootElement
                    .GetProperty("error")
                    .GetString()!;

                return true;
            }
            catch
            {
                message = "";
                return false;
            }
        }

        public Result<double> CosineSimilarity(double[] embedding1, double[] embedding2)
        {
            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < embedding1.Length; i++)
            {
                dotProduct += embedding1[i] * embedding2[i];
                magnitude1 += embedding1[i] * embedding1[i];
                magnitude2 += embedding2[i] * embedding2[i];
            }

            if (magnitude1 == 0 || magnitude2 == 0)
                return Result<double>.Ok(0);

            double similarity = dotProduct / (Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
            double res = Math.Max(-1.0, Math.Min(1.0, similarity));

            return Result<double>.Ok(res);
        }
    }
}
