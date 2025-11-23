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
        private TimeSpan ttl = TimeSpan.FromHours(double.Parse(configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!));

        public async Task<Result<string>> GetAnswerAsync(int productId, string question)
        {
            var product = await dbContext.Set<Product>().FirstOrDefaultAsync(p => p.Id == productId);
            if (product is null)
            {
                return Result<string>.Fail($"Product with id {productId} is not found");
            }

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
            if (!answer.Success)
            {
                return Result<string>.Fail(answer.Error);
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

        private async Task<IEnumerable<ManualEmbeddingDto>> GetManualEmbeddingsAsync(int productId)
        {
            var embeddings = await dbContext.Set<ManualEmbedding>().Where(e => e.ProductId == productId).ToListAsync();

            var embeddingDtos = mapper.Map<List<ManualEmbeddingDto>>(embeddings);

            return embeddingDtos;
        }
    }
}
