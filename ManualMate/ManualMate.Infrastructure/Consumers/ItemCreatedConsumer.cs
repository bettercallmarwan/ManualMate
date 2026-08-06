using ManualMate.Application.Contracts;
using ManualMate.Application.Interfaces.Services;
using ManualMate.Domain.Enums;
using ManualMate.Infrastructure.Presistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManualMate.Infrastructure.Consumers;

public class ItemCreatedConsumer(
    IFileProcessingService fileProcessingService,
    ManualMateDbContext manualMateDbContext,
    ILogger<ItemCreatedConsumer> logger)
    : IConsumer<ItemCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ItemCreatedIntegrationEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Processing file for Item ID: {ItemId}", message.ItemId);
        
        var item = await manualMateDbContext.Items.FindAsync(message.ItemId);
        if (item == null)
        {
            logger.LogWarning("Item with ID {ItemId} not found. Skipping processing.", message.ItemId);
            return;
        }

        await manualMateDbContext.FileEmbeddings.Where(e => e.ItemId == message.ItemId).ExecuteDeleteAsync();
        
        try
        {
            item.Status = ItemStatus.Processing;
            await manualMateDbContext.SaveChangesAsync();
            
            await fileProcessingService.ProcessFileAsync(message.ItemId);
            
            item.Status = ItemStatus.Completed;
            await manualMateDbContext.SaveChangesAsync();
            
            logger.LogInformation("Successfully processed file for Item ID: {ItemId}", message.ItemId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process file for Item ID: {ItemId}", message.ItemId);
            item.Status = ItemStatus.Failed;
            await manualMateDbContext.SaveChangesAsync();
            throw;
        }
    }
}