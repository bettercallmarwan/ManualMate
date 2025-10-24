using ManualMate.Models;
using ManualMate.Presistence;
using ManualMate.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace ManualMate.Jobs
{
    public class EmbeddingCacheJob(ManualMateDbContext dbContext,
        RedisService redisService,
        IConfiguration configuration,
        ILogger<ProductCacheJob> logger) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await SyncNewEmbeddings();
        }

        private async Task SyncNewEmbeddings()
        {
            try
            {
                var embeddings = await dbContext.Set<ManualEmbedding>().ToListAsync();
                if (!embeddings.Any())
                {
                    logger.LogWarning("no embeddings found in db, skipping cache update");
                    return;
                }

                var ttl = TimeSpan.FromHours(double.Parse(configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!));

                await redisService.SetAsync("embeddings:all", embeddings, ttl);
                logger.LogInformation("embeddings cached successfully");
            }
            catch(Exception e)
            {
                logger.LogError(e, "error while cahcing embeddings");
            }
        }
    }
}
