using AutoMapper;
using ManualMate.API.Controllers.Responses;
using ManualMate.Application.DTOs;
using ManualMate.Application.Interfaces;
using ManualMate.Domain.Models;
using ManualMate.Infrastructure.Presistence;
using ManualMate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace ManualMate.Application.Services
{
    public class ManualQaService(ManualMateDbContext dbContext,
        ProductRepository productRepository,
        IEmbeddingService embeddingService,
        ILlmService llmService,
        IConfiguration configuration,
        ICacheService cache) : IManualQaService
    {
        private static int top_k = 7;
        private static string context_seperator = "\n\n---\n\n";
        private TimeSpan ttl = TimeSpan.FromHours(double.Parse(configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!));

        public async Task<Result<string>> GetAnswerAsync(int productId, string question)
        {
            var productExists = await productRepository.ProductExists(productId);
            if (!productExists)
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

            var questionVector = questionEmbedding.Value;

            var topChunks = await dbContext.Set<ManualEmbedding>()
                .Where(e => e.ProductId == productId)
                .OrderBy(e => e.Embedding.CosineDistance(questionVector))
                .Take(top_k)
                .Select(e => e.TextChunk)
                .ToListAsync();

            var context = string.Join(context_seperator, topChunks);

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
    }
}