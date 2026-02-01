using ManualMate.API.Controllers.Responses;
using ManualMate.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace ManualMate.Application.Services
{
    public class FileUploadService(IItemRepository itemRepository) 
    {
        public async Task<Result<string>> UploadItemFileAsync(int itemId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Result<string>.Fail("File Is Empty or Null", HttpStatusCode.BadRequest);
            }

            var itemExists = await itemRepository.ItemExists(itemId);
            if (!itemExists)
            {
                return Result<string>.Fail($"Item with id : {itemId} not found", HttpStatusCode.NotFound);
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if(fileExtension != ".pdf")
            {
                return Result<string>.Fail($"File type {fileExtension} is not allowed", HttpStatusCode.BadRequest);
            }

            var fileName = $"{itemId}_{Guid.NewGuid()}{fileExtension}";
            var filesPath = Path.Combine("wwwroot", "Files");
            
            if (!Directory.Exists(filesPath))
                Directory.CreateDirectory(filesPath);

            var filePath = Path.Combine(filesPath, fileName);

            try
            {
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var item = await itemRepository.GetAsync(itemId);
                if (item == null)
                {
                    return Result<string>.Fail($"Item with id : {itemId} not found", HttpStatusCode.NotFound);
                }

                if (!string.IsNullOrEmpty(item.FilePath) && File.Exists(item.FilePath))
                {
                    File.Delete(item.FilePath);
                }

                item.FilePath = filePath;
                item.LastUpdated = DateTime.UtcNow;

                await itemRepository.SaveChangesAsync();
                return Result<string>.Ok(item.FilePath);
            }
            catch (Exception)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                throw;
            }
        }

        public Result<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Result<string>.Fail("File Is Empty or Null", HttpStatusCode.BadRequest);
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (fileExtension != ".pdf")
            {
                return Result<string>.Fail($"File type {fileExtension} is not allowed", HttpStatusCode.BadRequest);
            }

            var fileName = $"{file.Name}_{Guid.NewGuid()}{fileExtension}";
            var filesPath = Path.Combine("wwwroot", "Files");

            if (!Directory.Exists(filesPath))
                Directory.CreateDirectory(filesPath);

            var filePath = Path.Combine(filesPath, fileName);

            return Result<string>.Ok(filePath);
        }
    }
}