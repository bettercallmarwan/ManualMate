using ManualMate.API.Controllers.Responses;
using ManualMate.Infrastructure.Repositories;
using System.Net;

namespace ManualMate.Application.Services
{
    public class FileUploadService(ProductRepository productRepository)
    {
        public async Task<Result<string>> UploadProductManualAsync(int productId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Result<string>.Fail("File Is Empty or Null", HttpStatusCode.BadRequest);
            }

            var productExists = await productRepository.ProductExists(productId);
            if (!productExists)
            {
                return Result<string>.Fail($"Product with id : {productId} not found", HttpStatusCode.NotFound);
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if(fileExtension != ".pdf")
            {
                return Result<string>.Fail($"File type {fileExtension} is not allowed", HttpStatusCode.BadRequest);
            }

            var fileName = $"{productId}_{Guid.NewGuid()}{fileExtension}";
            var manualsPath = Path.Combine("wwwroot", "Manuals");
            
            if (!Directory.Exists(manualsPath))
                Directory.CreateDirectory(manualsPath);

            var filePath = Path.Combine(manualsPath, fileName);

            try
            {
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var product = await productRepository.GetAsync(productId);
                if (product == null)
                {
                    return Result<string>.Fail($"Product with id : {productId} not found", HttpStatusCode.NotFound);
                }

                if (!string.IsNullOrEmpty(product.ManualPath) && File.Exists(product.ManualPath))
                {
                    File.Delete(product.ManualPath);
                }

                product.ManualPath = filePath;
                product.LastUpdated = DateTime.UtcNow;

                await productRepository.SaveChangesAsync();
                return Result<string>.Ok(product.ManualPath);
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
    }
}