using ManualMate.API.Controllers.Responses;
using ManualMate.Application.DTOs;
using ManualMate.Application.Interfaces;
using ManualMate.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pgvector.EntityFrameworkCore;

namespace ManualMate.Application.Services
{
    public class QaService : IQaService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IEmbeddingService _embeddingService;
        private readonly ILlmService _llmService;
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cache;

        private readonly string context_seperator;
        private readonly int top_k;

        private readonly TimeSpan ttl;

        public QaService(
            IApplicationDbContext dbContext,
            IEmbeddingService embeddingService,
            ILlmService llmService,
            IConfiguration configuration,
            ICacheService cache)
        {
            _dbContext = dbContext;
            _embeddingService = embeddingService;
            _llmService = llmService;
            _configuration = configuration;
            _cache = cache;

            context_seperator = _configuration.GetSection("RAG")["context_seperator"]!;
            top_k = int.Parse(_configuration.GetSection("RAG")["top_k"]!);

            ttl = TimeSpan.FromHours(double.Parse(_configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!));
        }

        public async Task<Result<string>> GetAnswerAsync(int itemId, string question)
        {
            var itemExists = await _dbContext.Items.AnyAsync(i => i.Id == itemId);
            if (!itemExists)
            {
                return Result<string>.Fail($"Item with id {itemId} is not found");
            }

            string normalizedQuestion = string.Concat(question.Where(c => !char.IsWhiteSpace(c))).ToLower();
            string cacheKey = $"question:{itemId}:{normalizedQuestion}";

            var cachedQuestion = await _cache.GetAsync<QuestionCache>(cacheKey);
            if(cachedQuestion is not null)
            {
                return Result<string>.Ok(cachedQuestion.Answer);
            }

            var questionEmbedding = await _embeddingService.GetEmbeddingAsync(question);

            if (!questionEmbedding.Success)
            {
                return Result<string>.Fail(questionEmbedding.Error, questionEmbedding.StatusCode);
            }

            var questionVector = questionEmbedding.Value;

            var topChunks = await _dbContext.FileEmbeddings
                .Where(fe => fe.ItemId == itemId)
                .OrderBy(fe => fe.Embedding.CosineDistance(questionVector))
                .Select(fe => fe.TextChunk)
                .Take(top_k)
                .ToListAsync();

            var context = string.Join(context_seperator, topChunks);

            var answer = await _llmService.GenerateAnswerAsync(context, question);
            if (!answer.Success)
            {
                return Result<string>.Fail(answer.Error, answer.StatusCode);
            }

            var questionToCache = new QuestionCache
            {
                ItemId = itemId,
                Question = question,
                Answer = answer.Value
            };
            await _cache.SetAsync(cacheKey, questionToCache, ttl);

            return Result<string>.Ok(answer.Value);
        }
    }
}