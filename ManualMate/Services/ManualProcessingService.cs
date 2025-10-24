using ManualMate.Interfaces;
using ManualMate.Models;
using ManualMate.Presistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ManualMate.Services
{
    public class ManualProcessingService(ManualMateDbContext dbContext,
        ILogger<ManualProcessingService> logger,
        IEmbeddingService embeddingService,
        ProductService productService,
        RedisService redisService,
        IConfiguration configuration) : IManualProcessingService
    {
        public async Task ProcessManualAsync(int productId)
        {
            var product = await productService.GetProductAsync(productId);

            if (product is null)
                throw new Exception($"can't process manual because product with id : {productId} is not found");
            logger.LogInformation($"Processing manual of product with id : {productId}");

            var pdfPath = product.ManualPath;
            var pdfText = PdfExtractor.ExtractTextFromPdf(pdfPath);
            logger.LogInformation("Extracted {Length} characters", pdfText.Length);

            var chunks = TextChunker.ChunkText(pdfText);
            logger.LogInformation($"Created {chunks.Count} chunks");

            for(int i = 0; i < chunks.Count; i++)
            {
                var embedding = await embeddingService.GetEmbeddingAsync(chunks[i]);
                var embeddingJson = JsonSerializer.Serialize(embedding);
                var newEmbedding = new ManualEmbedding
                {
                    ProductId = productId,
                    TextChunk = chunks[i],
                    EmbeddingJson = embeddingJson,
                    ChunkIndex = i
                };
                await dbContext.Set<ManualEmbedding>().AddAsync(newEmbedding);


                if((i + 1) % 5 == 0)
                {
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("saved batch");
                }
            }

            await dbContext.SaveChangesAsync();
            logger.LogInformation("Processing completed");

            var embeddingsFromDb = await dbContext.Set<ManualEmbedding>().ToListAsync();
            await redisService.SetAsync("embeddings:all", embeddingsFromDb, TimeSpan.FromHours(double.Parse(configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!)));
            logger.LogInformation($"cached embeddings after processing product embeddings with id : {productId}");
        }
    }
}
