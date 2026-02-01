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

                if(TryExtractHuggingFaceAnswer(json, out var embeddingsResponse))
                {
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
            catch (Exception ex)
            {
                Console.WriteLine($"Vector extraction failed: {ex.Message}");
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

        //public Result<float> CosineSimilarity(float[] embedding1, float[] embedding2)
        //{
        //    if (embedding1 == null)
        //        return Result<float>.Fail("embedding1 cannot be null", HttpStatusCode.BadRequest);
            
        //    if (embedding2 == null)
        //        return Result<float>.Fail("embedding2 cannot be null", HttpStatusCode.BadRequest);

        //    if (embedding1.Length != embedding2.Length)
        //        return Result<float>.Fail($"Embeddings must have the same length. embedding1: {embedding1.Length}, embedding2: {embedding2.Length}", HttpStatusCode.BadRequest);

        //    if (embedding1.Length == 0)
        //        return Result<float>.Fail("Embeddings cannot be empty", HttpStatusCode.BadRequest);

        //    float dotProduct = 0;
        //    float magnitude1 = 0;
        //    float magnitude2 = 0;

        //    for (int i = 0; i < embedding1.Length; i++)
        //    {
        //        dotProduct += embedding1[i] * embedding2[i];
        //        magnitude1 += embedding1[i] * embedding1[i];
        //        magnitude2 += embedding2[i] * embedding2[i];
        //    }

        //    if (magnitude1 == 0 || magnitude2 == 0)
        //        return Result<float>.Ok(0);

        //    double similarity = dotProduct / (Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
        //    float res = (float)Math.Max(-1.0, Math.Min(1.0, similarity));

        //    return Result<float>.Ok(res);
        //}
    }
}
