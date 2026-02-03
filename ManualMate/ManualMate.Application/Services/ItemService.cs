using AutoMapper;
using ManualMate.API.Controllers.Responses;
using ManualMate.Application.DTOs;
using ManualMate.Application.Interfaces;
using ManualMate.Application.Interfaces.Services;
using ManualMate.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace ManualMate.Application.Services
{
    public class ItemService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly FileUploadService _fileUploadService;
        private readonly IFileProcessingService _fileProcessingService;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _ttl;

        public ItemService(IApplicationDbContext dbContext,
                           FileUploadService fileUploadService,
                           IFileProcessingService fileProcessingService,
                           IMapper mapper,
                           ICacheService cache,
                           IConfiguration configuration)
        {
            _dbContext = dbContext;
            _fileUploadService = fileUploadService;
            _fileProcessingService = fileProcessingService;
            _mapper = mapper;
            _cache = cache;
            _configuration = configuration;
            _ttl = TimeSpan.FromHours(double.Parse(_configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!));
        }

        public async Task<Result<GetItemDto>> GetItemAsync(int id)
        {
            string cacheKey = $"item:{id}";
            var cachedItem = await _cache.GetAsync<GetItemDto>(cacheKey);
            if (cachedItem is not null)
            {
                return Result<GetItemDto>.Ok(cachedItem);
            }

            var item = await _dbContext.Items.FindAsync(id);
            if(item is not null)
            {
                var itemToReturn = _mapper.Map<GetItemDto>(item);
                await _cache.SetAsync(cacheKey, itemToReturn, _ttl);
                return Result<GetItemDto>.Ok(itemToReturn);
            }

            return Result<GetItemDto>.Fail("item Not Found", HttpStatusCode.NotFound);
        }

        public async Task<Result<IEnumerable<GetItemDto>>> GetItemsAsync()
        {
            var items = await _dbContext.Items.ToListAsync();
            var itemsToReturn = _mapper.Map<IEnumerable<GetItemDto>>(items);

            return Result<IEnumerable<GetItemDto>>.Ok(itemsToReturn);
        }

        public async Task<Result<CreateItemDto>> CreateItemAsync(CreateItemDto dto)
        {
            var item = _mapper.Map<Item>(dto);

            var uploadFileResult = await _fileUploadService.UploadFileAsync(dto.File);
            if (!uploadFileResult.Success)
            {
                return Result<CreateItemDto>.Fail("Error uploading item file", uploadFileResult.StatusCode);
            }
            item.FilePath = uploadFileResult.Value;

            await _dbContext.Items.AddAsync(item);
            await _dbContext.SaveChangesAsync();

            return Result<CreateItemDto>.Ok(dto);
        }

        public async Task<Result<GetItemDto>> EditItemAsync(int id, CreateItemDto dto)
        {
            var item = await _dbContext.Items.FindAsync(id);
            if (item is null)
                return Result<GetItemDto>.Fail($"Item with id {id} not found", HttpStatusCode.NotFound);

            _mapper.Map(dto, item);

            _dbContext.Items.Update(item);
            await _dbContext.SaveChangesAsync();

            string cacheKey = $"item:{id}";
            await _cache.RemoveAsync<GetItemDto>(cacheKey);

            var itemToReturn = _mapper.Map<GetItemDto>(item);

            return Result<GetItemDto>.Ok(itemToReturn);
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            await _dbContext.Items.Where(i => i.Id == id).ExecuteDeleteAsync();

            string cacheKey = $"item:{id}";
            await _cache.RemoveAsync<GetItemDto>(cacheKey);

            return Result<bool>.Ok(true);
        }
    }
}