using ManualMate.Models;
using ManualMate.Presistence;
using Microsoft.EntityFrameworkCore;

namespace ManualMate.Services
{
    public class FileUploadService(ManualMateDbContext dbContext, IWebHostEnvironment environment)
    {
        public async Task<string?> UploadProductManualAsync(int productId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var product = await dbContext.Set<Product>().FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
                return null;

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

            return product.ManualPath;
        }
    }
}