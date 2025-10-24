using ManualMate.DTOs;
using ManualMate.Models;
using ManualMate.Presistence;
using Microsoft.EntityFrameworkCore;

namespace ManualMate.Services
{
    public class ProductService(ManualMateDbContext context,
        RedisService redisService,
        IConfiguration configuration)
    {
        public async Task<Product?> GetProductAsync(int id)
        {
            string cacheKey = $"products:{id}";

            var cached = await redisService.GetAsync<Product>(cacheKey);
            if (cached is not null)
                return cached;

            var product = await context.Set<Product>().FirstOrDefaultAsync(p => p.Id == id);
            if(product is not null)
            {
                await redisService.SetAsync(cacheKey, product, TimeSpan.FromHours(double.Parse(configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!)));
                return product;
            }

            return null;
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            const string cacheKey = "products:all";
            var cached = await redisService.GetAsync<IEnumerable<Product>>(cacheKey);
            if(cached is not null)
                return cached;

            var products = await context.Set<Product>().ToListAsync();

            await redisService.SetAsync(cacheKey, products, TimeSpan.FromHours(double.Parse(configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!)));

            return products;
        }

        public async Task<ProductDto> CreateProductAsync(ProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                ManualPath = dto.ManualPath,
                LastUpdated = DateTime.UtcNow
            };

            await context.Set<Product>().AddAsync(product);
            await context.SaveChangesAsync();

            return dto;
        }

        public async Task<ProductDto?> EditProductAsync(int id, ProductDto dto)
        {
            var product = await context.Set<Product>().FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return null;

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.ManualPath = dto.ManualPath;
            product.LastUpdated = DateTime.UtcNow;

            context.Set<Product>().Update(product);
            await context.SaveChangesAsync();

            string cacheKey = $"products:{id}";

            await redisService.DeleteAsync(cacheKey);

            return dto;
        }

        public async Task<bool?> DeleteAsync(int id)
        {
            var product = await context.Set<Product>().FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return null;

            context.Set<Product>().Remove(product);
            await context.SaveChangesAsync();

            await redisService.DeleteAsync($"products:{id}");

            return true;
        }
    }
}
