using ManualMate.Domain.Interfaces;
using ManualMate.Domain.Models;
using ManualMate.Infrastructure.Presistence;
using Microsoft.EntityFrameworkCore;

namespace ManualMate.Infrastructure.Repositories
{
    public class ProductRepository(ManualMateDbContext dbContext) : IGenericRepository<Product, int>
    {
        public async Task AddAsync(Product entity)
        {
            await dbContext.Set<Product>().AddAsync(entity);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await dbContext.Set<Product>().ToListAsync();
        }

        public async Task<Product?> GetAsync(int id)
        {
            return await dbContext.Set<Product>().FirstOrDefaultAsync(p => p.Id == id);
        }

        public void Remove(Product entity)
        {
            dbContext.Set<Product>().Remove(entity);
        }
        public void Update(Product entity)
        {
            dbContext.Set<Product>().Update(entity);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public async Task<bool> ProductExists(int id)
        {
            return await dbContext.Set<Product>().AnyAsync(p => p.Id == id);
        }
    }
}
