using ManualMate.Application.DTOs;
using ManualMate.Application.Interfaces;
using ManualMate.Application.Interfaces.Services;
using ManualMate.Application.Responses;
using ManualMate.Domain.Enums;
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
        private readonly ICacheService _cache;

        private readonly string _contextSeparator;
        private readonly int _topK;

        private readonly TimeSpan _ttl;

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
            _cache = cache;

            _contextSeparator = configuration.GetSection("RAG")["context_separator"]!;
            _topK = int.Parse(configuration.GetSection("RAG")["top_k"]!);

            _ttl = TimeSpan.FromHours(double.Parse(configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!));
        }

        public async Task<Result<string>> GetAnswerAsync(Guid itemId, string question)
        {
            var item = await _dbContext.Items.FindAsync(itemId);
            if (item == null)
            {
                return Result<string>.Fail($"Item with id {itemId} is not found");
            }

            if (item.Status != ItemStatus.Completed)
            {
                return Result<string>.Fail($"Item with id {itemId} is not processed yet");
            }

            var normalizedQuestion = string.Concat(question.Where(c => !char.IsWhiteSpace(c))).ToLower();
            var cacheKey = $"question:{itemId}:{normalizedQuestion}";

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
                .Take(_topK)
                .ToListAsync();

            var context = string.Join(_contextSeparator, topChunks);

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
            await _cache.SetAsync(cacheKey, questionToCache, _ttl);

            return Result<string>.Ok(answer.Value);
        }
    }
}