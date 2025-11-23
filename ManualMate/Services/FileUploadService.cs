using ManualMate.Controllers.Responses;
using ManualMate.Exceptions;
using ManualMate.Models;
using ManualMate.Presistence;
using Microsoft.EntityFrameworkCore;

namespace ManualMate.Services
{
    public class FileUploadService(ManualMateDbContext dbContext)
    {
        public async Task<Result<string>> UploadProductManualAsync(int productId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Result<string>.Fail("File Is Empty or Null");

            var product = await dbContext.Set<Product>().FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
                throw new NotFoundException("Product", productId);

            var fileName = file.FileName;
            var manualsPath = Path.Combine("wwwroot", "Manuals");
            
            if (!Directory.Exists(manualsPath))
                Directory.CreateDirectory(manualsPath);

            var filePath = Path.Combine(manualsPath, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            product.ManualPath = $"wwwroot/Manuals/{fileName}";
            product.LastUpdated = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return Result<string>.Ok(product.ManualPath);
        }
    }
}