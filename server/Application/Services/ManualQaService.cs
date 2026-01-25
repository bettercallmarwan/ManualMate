using AutoMapper;
using ManualMate.API.Controllers.Responses;
using ManualMate.Application.DTOs;
using ManualMate.Application.Interfaces;
using ManualMate.Domain.Models;
using ManualMate.Infrastructure.Presistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ManualMate.Application.Services
{
    public class ManualQaService(ManualMateDbContext dbContext,
        IEmbeddingService embeddingService,
        ILlmService llmService,
        IMapper mapper,
        IConfiguration configuration,
        ICacheService cache) : IManualQaService
    {
        private static int top_k = 7;
        private static string context_seperator = "\n\n---\n\n";
        private TimeSpan ttl = TimeSpan.FromHours(double.Parse(configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!));

        public async Task<Result<string>> GetAnswerAsync(int productId, string question)
        {
            var product = await dbContext.Set<Product>().FirstOrDefaultAsync(p => p.Id == productId);
            if (product is null)
            {
                return Result<string>.Fail($"Product with id {productId} is not found");
            }

            string normalizedQuestion = string.Concat(question.Where(c => !char.IsWhiteSpace(c))).ToLower();
            string cacheKey = $"question:{productId}:{normalizedQuestion}";

            var cachedQuestion = await cache.GetAsync<QuestionCache>(cacheKey);
            if(cachedQuestion is not null)
            {
                return Result<string>.Ok(cachedQuestion.Answer);
            }

            var questionEmbedding = await embeddingService.GetEmbeddingAsync(question);
            if (!questionEmbedding.Success)
            {
                return Result<string>.Fail(questionEmbedding.Error, questionEmbedding.StatusCode);
            }

            var allEmbeddings = await GetManualEmbeddingsAsync(productId);
            if (!allEmbeddings.Value.Any())
            {
                return Result<string>.Ok("No information about this product");
            }

            var similarities = allEmbeddings.Value.Select(e =>
            {
                var embedding = JsonSerializer.Deserialize<double[]>(e.EmbeddingJson);
                var similarity = embeddingService.CosineSimilarity(questionEmbedding.Value, embedding);
                return new { Embedding = e, Similarity = similarity.Value };
            }).OrderByDescending(x => x.Similarity).Take(top_k).ToList();

            var context = string.Join(context_seperator, similarities.Select(s => s.Embedding.TextChunk));

            var answer = await llmService.GenerateAnswerAsync(context, question);
            if (!answer.Success)
            {
                return Result<string>.Fail(answer.Error, answer.StatusCode);
            }

            var questionToCache = new QuestionCache
            {
                ProductId = productId,
                Question = question,
                Answer = answer.Value
            };
            await cache.SetAsync(cacheKey, questionToCache, ttl);

            return Result<string>.Ok(answer.Value);
        }

        private async Task<Result<IEnumerable<ManualEmbeddingDto>>> GetManualEmbeddingsAsync(int productId)
        {
            var embeddings = await dbContext.Set<ManualEmbedding>().Where(e => e.ProductId == productId).ToListAsync();

            var embeddingDtos = mapper.Map<List<ManualEmbeddingDto>>(embeddings);

            return Result<IEnumerable<ManualEmbeddingDto>>.Ok(embeddingDtos);
        }
    }
}
