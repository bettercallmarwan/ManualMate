using ManualMate.API.Controllers.Responses;
using ManualMate.Application.Interfaces;
using ManualMate.Domain.Models;
using ManualMate.Infrastructure.Repositories;
using System.Net;
using System.Text.Json;

namespace ManualMate.Application.Services
{
    public class ManualProcessingService(
        ManualEmbeddingRepository repository,
        IEmbeddingService embeddingService,
        ProductRepository productRepository) : IManualProcessingService
    {
        public async Task<Result<bool>> ProcessManualAsync(int productId)
        {
            var product = await productRepository.GetAsync(productId);
            if (product is null)
                return Result<bool>.Fail($"Product with id : {productId} not found", HttpStatusCode.NotFound);

            var pdfText = PdfExtractor.ExtractTextFromPdf(product.ManualPath).Value;
            var chunks = TextChunker.ChunkText(pdfText).Value;

            for (int i = 0; i < chunks.Count; i++)
            {
                var embedding = await embeddingService.GetEmbeddingAsync(chunks[i]);

                var newEmbedding = new ManualEmbedding
                {
                    ProductId = productId,
                    TextChunk = chunks[i],
                    EmbeddingJson = JsonSerializer.Serialize(embedding.Value),
                    ChunkIndex = i
                };

                await repository.AddAsync(newEmbedding);
            }
            await repository.SaveChangesAsync();
            return Result<bool>.Ok(true);
        }
    }
}
