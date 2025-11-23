using AutoMapper;
using ManualMate.Controllers.Responses;
using ManualMate.DTOs;
using ManualMate.Exceptions;
using ManualMate.Interfaces;
using ManualMate.Models;
using ManualMate.Repositories;

namespace ManualMate.Services
{
    public class ProductService(ProductRepository repository,
        IConfiguration configuration, 
        IMapper mapper,
        ICacheService cache)
    {
        private TimeSpan ttl = TimeSpan.FromHours(double.Parse(configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!));

        public async Task<Result<GetProductDto>> GetProductAsync(int id)
        {
            string cacheKey = $"product:{id}";
            var cachedProduct = await cache.GetAsync<GetProductDto>(cacheKey);
            if (cachedProduct is not null)
            {
                return Result<GetProductDto>.Ok(cachedProduct);
            }

            var product = await repository.GetAsync(id);
            if(product is not null)
            {
                var productToReturn = mapper.Map<GetProductDto>(product);
                await cache.SetAsync(cacheKey, productToReturn, ttl);
                return Result<GetProductDto>.Ok(productToReturn);
            }

            return Result<GetProductDto>.Fail("Product Not Found");
        }

        public async Task<Result<IEnumerable<GetProductDto>>> GetProductsAsync()
        {
            var products = await repository.GetAllAsync();
            var productsToReturn = mapper.Map<IEnumerable<GetProductDto>>(products);

            return Result<IEnumerable<GetProductDto>>.Ok(productsToReturn);
        }

        public async Task<Result<CreateProductDto>> CreateProductAsync(CreateProductDto dto)
        {
            var product = mapper.Map<Product>(dto);

            await repository.AddAsync(product);
            await repository.SaveChangesAsync();

            return Result<CreateProductDto>.Ok(dto);
        }

        public async Task<Result<GetProductDto>> EditProductAsync(int id, CreateProductDto dto)
        {
            var product = await repository.GetAsync(id);
            if (product is null)
                throw new NotFoundException("Product", id);

            mapper.Map(dto, product);

            repository.Update(product);
            await repository.SaveChangesAsync();

            string cacheKey = $"product:{id}";
            await cache.RemoveAsync<GetProductDto>(cacheKey);

            var productToReturn = mapper.Map<GetProductDto>(product);

            return Result<GetProductDto>.Ok(productToReturn);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await repository.GetAsync(id);
            if (product is null)
                throw new NotFoundException("Product", id);

            repository.Remove(product);
            await repository.SaveChangesAsync();

            string cacheKey = $"product:{id}";
            await cache.RemoveAsync<GetProductDto>(cacheKey);
        }
    }
}