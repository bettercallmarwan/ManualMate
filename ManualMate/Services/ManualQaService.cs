using AutoMapper;
using ManualMate.Controllers.Responses;
using ManualMate.DTOs;
using ManualMate.Exceptions;
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
        IMapper mapper,
        IConfiguration configuration,
        ICacheService cache) : IManualQaService
    {
        private static int top_k = 7;
        private TimeSpan ttl = TimeSpan.FromHours(double.Parse(configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!));

        public async Task<Result<string>> GetAnswerAsync(int productId, string question)
        {
            var product = await dbContext.Set<Product>().FirstOrDefaultAsync(p => p.Id == productId);
            if (product is null) throw new NotFoundException("product", productId);

            string cacheKey = $"question:{productId}:{question.ToLower()}";
            var cachedQuestion = await cache.GetAsync<QuestionCache>(cacheKey);
            if(cachedQuestion is not null)
            {
                return Result<string>.Ok(cachedQuestion.Answer);
            }

            var questionEmbedding = await embeddingService.GetEmbeddingAsync(question);

            var allEmbeddings = await GetManualEmbeddingsAsync(productId);
            if (!allEmbeddings.Any())
            {
                return Result<string>.Ok("No information about this product");
            }

            var similarities = allEmbeddings.Select(e =>
            {
                var embedding = JsonSerializer.Deserialize<double[]>(e.EmbeddingJson);
                var similarity = embeddingService.CosineSimilarity(questionEmbedding, embedding);
                return new { Embedding = e, Similarity = similarity };
            })
                .OrderByDescending(x => x.Similarity).Take(top_k).ToList();

            var context = string.Join("\n\n---\n\n", similarities.Select(s => s.Embedding.TextChunk));

            var answer = await llmService.GenerateAnswerAsync(context, question);

            var questionToCache = new QuestionCache
            {
                ProductId = productId,
                Question = question,
                Answer = answer
            };
            await cache.SetAsync(cacheKey, questionToCache, ttl);

            return Result<string>.Ok(answer);
        }

        private async Task<IEnumerable<ManualEmbeddingDto>> GetManualEmbeddingsAsync(int productId)
        {
            var embeddings = await dbContext.Set<ManualEmbedding>().Where(e => e.ProductId == productId).ToListAsync();

            var embeddingDtos = mapper.Map<List<ManualEmbeddingDto>>(embeddings);

            return embeddingDtos;
        }
    }
}
