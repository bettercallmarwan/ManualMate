using ManualMate.API.Controllers.Responses;
using ManualMate.Application.Interfaces.Services;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ManualMate.Application.Services
{
    public class HuggingFaceEmbeddingService(IHttpClientFactory clientFactory) : IEmbeddingService
    {
        public async Task<Result<Pgvector.Vector>> GetEmbeddingAsync(string text)
        {
            try
            {
                var response = await GetHuggingFaceResponse(text);

                string json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if(TryExtractHuggingFaceErrorMessage(json, out var message))
                    {
                        return Result<Pgvector.Vector>.Fail(message, response.StatusCode);
                    }

                    return Result<Pgvector.Vector>.Fail("Error Generating Answer.", response.StatusCode);
                }

                if(TryExtractHuggingFaceAnswer(json, out Pgvector.Vector? embeddingsResponse))
                {
                    if(embeddingsResponse is null)
                    {
                        return Result<Pgvector.Vector>.Fail("Cannot generate question embeddings");
                    }
                    return Result<Pgvector.Vector>.Ok(embeddingsResponse);
                }

                return Result<Pgvector.Vector>.Fail("Unexpected behaviour from Hugging Face Json response", response.StatusCode);
            }
            catch (Exception ex)
            {
                return Result<Pgvector.Vector>.Fail("Unexpected error: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        private async Task<HttpResponseMessage> GetHuggingFaceResponse(string text)
        {
            var payload = new { inputs = text };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var client = clientFactory.CreateClient("HuggingFaceClient");
            var response = await client.PostAsync("", content);

            return response;
        }

        private static bool TryExtractHuggingFaceAnswer(string json, out Pgvector.Vector? result)
        {
            try
            {
                var floatArray = JsonSerializer.Deserialize<float[]>(json);

                if (floatArray == null || floatArray.Length == 0)
                {
                    result = null;
                    return false;
                }

                result = new Pgvector.Vector(floatArray);
                return true;
            }
            catch (Exception)
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
    }
}
