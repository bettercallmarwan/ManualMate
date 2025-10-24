using ManualMate.Interfaces;
using ManualMate.Models;
using ManualMate.Presistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ManualMate.Services
{
    public class ManualQaService(ManualMateDbContext dbContext,
        IEmbeddingService embeddingService,
        ILlmService llmService,
        ILogger<ManualQaService> logger,
        RedisService redisService) : IManualQaService
    {
        public async Task<string> GetAnswerAsync(int productId, string question)
        {
            logger.LogInformation($"Getting answer for product wih id : {productId}");

            var questionEmbedding = await embeddingService.GetEmbeddingAsync(question);

            var allEmbeddings = await GetManualEmbeddingsAsync(productId);
            ;
            if (!allEmbeddings.Any())
                return "No information about this product";

            var similarities = allEmbeddings.Select(e =>
            {
                var embedding = JsonSerializer.Deserialize<float[]>(e.EmbeddingJson);
                var similarity = embeddingService.CosineSimilarity(questionEmbedding, embedding);
                return new { Embedding = e, Similarity = similarity };
            })
                .OrderByDescending(x => x.Similarity).Take(3).ToList();

            var context = string.Join("\n\n---\n\n",
                similarities.Select(s => s.Embedding.TextChunk));

            var answer = await llmService.GenerateAnswerAsync(context, question);

            return answer;
        }

        private async Task<IEnumerable<ManualEmbedding>> GetManualEmbeddingsAsync(int productId)
        {
            var cachedEmbeddings = await redisService.GetAsync<IEnumerable<ManualEmbedding>>("embeddings:all");
            if (cachedEmbeddings is not null)
                return cachedEmbeddings.Where(e => e.ProductId == productId);

            var embeddings = await dbContext.Set<ManualEmbedding>().Where(e => e.ProductId == productId).ToListAsync();
            return embeddings;
        }
    }
}
