using ManualMate.Models;
using ManualMate.Presistence;
using ManualMate.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace ManualMate.Jobs
{
    public class ProductCacheJob(ManualMateDbContext dbContext,
        RedisService redisService,
        IConfiguration configuration,
        ILogger<ProductCacheJob> logger) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await SyncNewProducts();
        }

        private async Task SyncNewProducts()
        {
            try
            {
                var products = await dbContext.Set<Product>().ToListAsync();
                if (!products.Any())
                {
                    logger.LogWarning("no products found in db, skipping cache update");
                    return;
                }

                var cacheKey = "products:all";
                var ttl = TimeSpan.FromHours(double.Parse(configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!));
                await redisService.SetAsync(cacheKey, products, ttl);
                logger.LogInformation($"products:all cached successfully at time :{DateTime.UtcNow}");

                foreach (var product in products)
                {
                    cacheKey = $"products:{product.Id}";
                    await redisService.SetAsync(cacheKey, product, ttl);
                    logger.LogInformation($"products:{product.Id} cached successfully at time :{DateTime.UtcNow}");
                }

                logger.LogInformation($"Products Cache Refreshed Successfully at time :{DateTime.UtcNow}");
            }
            catch (Exception e)
            {
                logger.LogError(e, "error while cahcing products");
            }
        }
    }
}
