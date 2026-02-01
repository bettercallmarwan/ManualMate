using AutoMapper;
using ManualMate.API.Controllers.Responses;
using ManualMate.Application.DTOs;
using ManualMate.Application.Interfaces.Repositories;
using ManualMate.Application.Interfaces.Services;
using ManualMate.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace ManualMate.Application.Services
{
    public class ItemService(IItemRepository itemRepository,
        IItemFileEmbeddingRepository  itemFileEmbeddingRepository,
        FileUploadService fileUploadService,
        IFileProcessingService fileProcessingService,
        IConfiguration configuration, 
        IMapper mapper,
        ICacheService cache)
    {
        private TimeSpan ttl = TimeSpan.FromHours(double.Parse(configuration.GetSection("RedisSettings")["TimeToLiveInHours"]!));

        public async Task<Result<GetItemDto>> GetItemAsync(int id)
        {
            string cacheKey = $"item:{id}";
            var cachedItem = await cache.GetAsync<GetItemDto>(cacheKey);
            if (cachedItem is not null)
            {
                return Result<GetItemDto>.Ok(cachedItem);
            }

            var item = await itemRepository.GetAsync(id);
            if(item is not null)
            {
                var itemToReturn = mapper.Map<GetItemDto>(item);
                await cache.SetAsync(cacheKey, itemToReturn, ttl);
                return Result<GetItemDto>.Ok(itemToReturn);
            }

            return Result<GetItemDto>.Fail("item Not Found", HttpStatusCode.NotFound);
        }

        public async Task<Result<IEnumerable<GetItemDto>>> GetItemsAsync()
        {
            var items = await itemRepository.GetAllAsync();
            var itemsToReturn = mapper.Map<IEnumerable<GetItemDto>>(items);

            return Result<IEnumerable<GetItemDto>>.Ok(itemsToReturn);
        }

        public async Task<Result<CreateItemDto>> CreateItemAsync(CreateItemDto dto)
        {
            var item = mapper.Map<Item>(dto);

            await itemRepository.AddAsync(item);
            await itemRepository.SaveChangesAsync();

            return Result<CreateItemDto>.Ok(dto);
        }

        public async Task<Result<ItemResponseDto>> CreateItemAndProcessFileAsync(CreateItemAndProcessFileDto dto)
        {
            var uploadFileResult = fileUploadService.UploadFileAsync(dto.File);
            if (!uploadFileResult.Success)
            {
                return Result<ItemResponseDto>.Fail("Error uploading item file", uploadFileResult.StatusCode);
            }

            var itemResponseDto = mapper.Map<ItemResponseDto>(dto);
            itemResponseDto.FilePath = uploadFileResult.Value;

            var item = mapper.Map<Item>(itemResponseDto);

            await itemRepository.AddAsync(item);
            await itemRepository.SaveChangesAsync();

            var itemId = item.Id;
            var processFileResult = await fileProcessingService.ProcessFileAsync(itemId);
            if (!processFileResult.Success)
            {
                return Result<ItemResponseDto>.Fail("Error processing file", processFileResult.StatusCode);
            }

            var itemToReturn = mapper.Map<CreateItemDto>(dto);
            return Result<ItemResponseDto>.Ok(itemResponseDto);
        }

        public async Task<Result<GetItemDto>> EditItemAsync(int id, CreateItemDto dto)
        {
            var item = await itemRepository.GetAsync(id);
            if (item is null)
                return Result<GetItemDto>.Fail($"Item with id {id} not found", HttpStatusCode.NotFound);

            mapper.Map(dto, item);

            itemRepository.Update(item);
            await itemRepository.SaveChangesAsync();

            string cacheKey = $"item:{id}";
            await cache.RemoveAsync<GetItemDto>(cacheKey);

            var itemToReturn = mapper.Map<GetItemDto>(item);

            return Result<GetItemDto>.Ok(itemToReturn);
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var item = await itemRepository.GetAsync(id);
            if (item is null)
                return Result<bool>.Fail($"Item with id :{id} is not found", HttpStatusCode.NotFound);

            itemRepository.Remove(item);
            await itemFileEmbeddingRepository.RemoveForItem(id);
            await itemRepository.SaveChangesAsync();

            string cacheKey = $"item:{id}";
            await cache.RemoveAsync<GetItemDto>(cacheKey);

            return Result<bool>.Ok(true);
        }
    }
}