using ManualMate.API.Controllers.Responses;
using ManualMate.Application.DTOs;
using ManualMate.Application.Interfaces;
using ManualMate.Infrastructure.Repositories;

namespace ManualMate.Application.Services
{
    public class QaService(ItemRepository itemRepository,
        ItemFileEmbeddingRepository itemFileEmbeddingRepository,
        IEmbeddingService embeddingService,
        ILlmService llmService,
        IConfiguration configuration,
        ICacheService cache) : IQaService
    {
        private string context_seperator = configuration.GetSection("RAG")["context_seperator"]!;
        private TimeSpan ttl = TimeSpan.FromHours(double.Parse(configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!));

        public async Task<Result<string>> GetAnswerAsync(int itemId, string question)
        {
            var itemExists = await itemRepository.ItemExists(itemId);
            if (!itemExists)
            {
                return Result<string>.Fail($"Item with id {itemId} is not found");
            }

            string normalizedQuestion = string.Concat(question.Where(c => !char.IsWhiteSpace(c))).ToLower();
            string cacheKey = $"question:{itemId}:{normalizedQuestion}";

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

            var topChunks = await itemFileEmbeddingRepository.GetItemTopChunks(itemId, questionVector);

            var context = string.Join(context_seperator, topChunks);

            var answer = await llmService.GenerateAnswerAsync(context, question);
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
            await cache.SetAsync(cacheKey, questionToCache, ttl);

            return Result<string>.Ok(answer.Value);
        }
    }
}