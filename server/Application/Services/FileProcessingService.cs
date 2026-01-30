using ManualMate.API.Controllers.Responses;
using ManualMate.Application.Interfaces;
using ManualMate.Domain.Models;
using ManualMate.Infrastructure.Repositories;
using System.Net;

namespace ManualMate.Application.Services
{
    public class FileProcessingService(
        ItemRepository itemRepository,
        ItemFileEmbeddingRepository itemFileEmbeddingRepository,
        IEmbeddingService embeddingService) : IFileProcessingService
    {
        public async Task<Result<bool>> ProcessFileAsync(int itemId)
        {
            var item = await itemRepository.GetAsync(itemId);
            if (item is null)
            {
                return Result<bool>.Fail($"Item with id : {itemId} not found", HttpStatusCode.NotFound);
            }

            var pdfText = PdfExtractor.ExtractTextFromPdf(item.FilePath).Value;
            var chunks = TextChunker.ChunkText(pdfText).Value;

            for (int i = 0; i < chunks.Count; i++)
            {
                var embedding = await embeddingService.GetEmbeddingAsync(chunks[i]);

                var newEmbedding = new FileEmbedding
                {
                    ItemId = itemId,
                    TextChunk = chunks[i],
                    Embedding = embedding.Value,
                    ChunkIndex = i
                };

                await itemFileEmbeddingRepository.AddAsync(newEmbedding);
            }
            await itemFileEmbeddingRepository.SaveChangesAsync();
            return Result<bool>.Ok(true);
        }

        public async Task<Result<bool>> DeleteFileEmbeddingsAsync(int itemId)
        {
            var item = await itemRepository.GetAsync(itemId);
            if (item is null)
                return Result<bool>.Fail($"Item with id : {itemId} not found", HttpStatusCode.NotFound);

            await itemFileEmbeddingRepository.RemoveForItem(itemId);
            return Result<bool>.Ok(true);
        }
    }
}