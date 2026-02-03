using ManualMate.API.Controllers.Responses;
using ManualMate.Application.Interfaces;
using ManualMate.Application.Interfaces.Services;
using ManualMate.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace ManualMate.Application.Services
{
    public class FileProcessingService : IFileProcessingService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IEmbeddingService _embeddingService;

        public FileProcessingService(IApplicationDbContext dbContext, IEmbeddingService embeddingService)
        {
            _dbContext = dbContext;
            _embeddingService = embeddingService;
        }

        public async Task<Result<bool>> ProcessFileAsync(int itemId)
        {
            var item = await _dbContext.Items.FindAsync(itemId);
            if (item is null)
            {
                return Result<bool>.Fail($"Item with id : {itemId} not found", HttpStatusCode.NotFound);
            }

            var pdfText = PdfExtractor.ExtractTextFromPdf(item.FilePath).Value;
            var chunks = TextChunker.ChunkText(pdfText).Value;

            for (int i = 0; i < chunks.Count; i++)
            {
                var embedding = await _embeddingService.GetEmbeddingAsync(chunks[i]);

                var newEmbedding = new FileEmbedding
                {
                    ItemId = itemId,
                    TextChunk = chunks[i],
                    Embedding = embedding.Value,
                    ChunkIndex = i
                };

                await _dbContext.FileEmbeddings.AddAsync(newEmbedding);
            }
            await _dbContext.SaveChangesAsync();
            return Result<bool>.Ok(true);
        }

        public async Task<Result<bool>> DeleteFileEmbeddingsAsync(int itemId)
        {
            var item = await _dbContext.Items.FindAsync(itemId);

            if (item is null)
                return Result<bool>.Fail($"Item with id : {itemId} not found", HttpStatusCode.NotFound);

            await _dbContext.FileEmbeddings.Where(fe => fe.ItemId == itemId).ExecuteDeleteAsync();
          
            return Result<bool>.Ok(true);
        }
    }
}