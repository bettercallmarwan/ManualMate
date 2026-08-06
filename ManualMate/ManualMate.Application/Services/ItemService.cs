using AutoMapper;
using ManualMate.Application.DTOs;
using ManualMate.Application.Interfaces;
using ManualMate.Application.Interfaces.Services;
using ManualMate.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ManualMate.Application.Contracts;
using ManualMate.Application.Responses;

namespace ManualMate.Application.Services
{
    public class ItemService(
        IApplicationDbContext dbContext,
        IFileUploadService fileUploadService,
        IIntegrationEventBus eventBus,
        IMapper mapper)
        : IItemService
    {

        public async Task<Result<GetItemDto>> GetItemAsync(Guid id)
        {
            var item = await dbContext.Items.FindAsync(id);
            if (item is null) return Result<GetItemDto>.Fail("item Not Found", HttpStatusCode.NotFound);
            
            var itemToReturn = mapper.Map<GetItemDto>(item);
            return Result<GetItemDto>.Ok(itemToReturn);
        }

        public async Task<Result<IEnumerable<GetItemDto>>> GetItemsAsync()
        {
            var items = await dbContext.Items.ToListAsync();
            var itemsToReturn = mapper.Map<IEnumerable<GetItemDto>>(items);

            return Result<IEnumerable<GetItemDto>>.Ok(itemsToReturn);
        }

        public async Task<Result<CreateItemDto>> CreateItemAsync(CreateItemDto dto)
        {
            var item = mapper.Map<Item>(dto);

            var uploadFileResult = await fileUploadService.UploadFileAsync(dto.File);
            if (!uploadFileResult.Success)
            {
                return Result<CreateItemDto>.Fail("Error uploading item file", uploadFileResult.StatusCode);
            }
            item.FilePath = uploadFileResult.Value;

            await dbContext.Items.AddAsync(item);

            await eventBus.PublishAsync(new ItemCreatedIntegrationEvent { ItemId = item.Id });

            await dbContext.SaveChangesAsync();
            return Result<CreateItemDto>.Ok(dto);
        }

        public async Task<Result<GetItemDto>> EditItemAsync(Guid id, UpdateItemDto dto)
        {
            var item = await dbContext.Items.FindAsync(id);
            if (item is null)
            {
                return Result<GetItemDto>.Fail($"Item with id {id} not found", HttpStatusCode.NotFound);
            }

            mapper.Map(dto, item);
            await dbContext.SaveChangesAsync();

            var itemToReturn = mapper.Map<GetItemDto>(item);
            return Result<GetItemDto>.Ok(itemToReturn);
        }

        public async Task<Result<bool>> DeleteAsync(Guid id)
        {
            var item = await dbContext.Items.FindAsync(id);
            if (item is null)
            {
                return Result<bool>.Fail($"Item with id {id} not found", HttpStatusCode.NotFound);
            }

            await dbContext.FileEmbeddings.Where(f => f.ItemId == id).ExecuteDeleteAsync();
            await dbContext.Items.Where(i => i.Id == id).ExecuteDeleteAsync();

            return Result<bool>.Ok(true);
        }
    }
}