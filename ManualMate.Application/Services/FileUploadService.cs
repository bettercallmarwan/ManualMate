using ManualMate.API.Controllers.Responses;
using ManualMate.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace ManualMate.Application.Services
{
    public class FileUploadService 
    {
        private readonly IApplicationDbContext _dbContext;

        public FileUploadService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<string>> UploadItemFileAsync(int itemId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Result<string>.Fail("File Is Empty or Null", HttpStatusCode.BadRequest);
            }

            var itemExists = await _dbContext.Items.AnyAsync(i => i.Id == itemId);
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

                var item = await _dbContext.Items.FindAsync(itemId);
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

                await _dbContext.SaveChangesAsync();
                return Result<string>.Ok(item.FilePath);
            }
            catch (Exception)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return Result<string>.Fail($"Cannot upload file", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<string>> UploadFileAsync(IFormFile file)
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

            try
            {
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Result<string>.Ok(filePath);
            }
            catch (Exception)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return Result<string>.Fail($"Cannot upload file", HttpStatusCode.InternalServerError);
            }
        }
    }
}