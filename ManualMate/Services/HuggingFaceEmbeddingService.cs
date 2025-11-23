using ManualMate.Interfaces;
using System.Text;
using System.Text.Json;

namespace ManualMate.Services
{
    public class HuggingFaceEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiToken;
        private readonly ILogger<HuggingFaceEmbeddingService> _logger;
         
        private const string MODEL_URL = "https://router.huggingface.co/hf-inference/models/BAAI/bge-small-en-v1.5";
        public HuggingFaceEmbeddingService(ILogger<HuggingFaceEmbeddingService> logger, IConfiguration configuration)
        {
            _apiToken = configuration["HuggingFace:ApiToken"]!;
            if (string.IsNullOrEmpty(_apiToken))
                throw new Exception("HuggingFace api is not configured");

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");

            _logger = logger;
        }
        public async Task<double[]> GetEmbeddingAsync(string text)
        {
            try
            {
                var payload = new { inputs = text };
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(MODEL_URL, content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var embedding = JsonSerializer.Deserialize<double[]>(jsonResponse);

                return embedding!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting embedding");
                throw;
            }
        }

        public double CosineSimilarity(double[] embedding1, double[] embedding2)
        {
            if (embedding1.Length != embedding2.Length)
                throw new ArgumentException("Embeddings must have the same length");

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
                return 0;

            double similarity = dotProduct / (Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));

            return Math.Max(-1.0, Math.Min(1.0, similarity));
        }
    }
}
