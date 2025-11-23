using ManualMate.Interfaces;
using ManualMate.Models;
using ManualMate.Repositories;
using System.Text.Json;

namespace ManualMate.Services
{
    public class ManualProcessingService(
        ManualEmbeddingRepository repository,
        IEmbeddingService embeddingService,
        ProductRepository productRepository) : IManualProcessingService
    {
        public async Task ProcessManualAsync(int productId)
        {
            var product = await productRepository.GetAsync(productId);
            if (product is null)
                throw new Exception($"can't process manual because product with id : {productId} is not found");

            var pdfText = PdfExtractor.ExtractTextFromPdf(product.ManualPath);
            var chunks = TextChunker.ChunkText(pdfText);

            for (int i = 0; i < chunks.Count; i++)
            {
                var embedding = await embeddingService.GetEmbeddingAsync(chunks[i]);

                var newEmbedding = new ManualEmbedding
                {
                    ProductId = productId,
                    TextChunk = chunks[i],
                    EmbeddingJson = JsonSerializer.Serialize(embedding),
                    ChunkIndex = i
                };

                await repository.AddAsync(newEmbedding);
            }
            await repository.SaveChangesAsync();
        }

    }
}
