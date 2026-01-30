using ManualMate.Domain.Interfaces;
using ManualMate.Domain.Models;
using ManualMate.Infrastructure.Presistence;
using Microsoft.EntityFrameworkCore;

namespace ManualMate.Infrastructure.Repositories
{
    public class ItemRepository(ManualMateDbContext dbContext) : IGenericRepository<Item, int>
    {
        public async Task AddAsync(Item entity)
        {
            await dbContext.Set<Item>().AddAsync(entity);
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            return await dbContext.Set<Item>().ToListAsync();
        }

        public async Task<Item?> GetAsync(int id)
        {
            return await dbContext.Set<Item>().FirstOrDefaultAsync(p => p.Id == id);
        }

        public void Remove(Item entity)
        {
            dbContext.Set<Item>().Remove(entity);
        }
        public void Update(Item entity)
        {
            dbContext.Set<Item>().Update(entity);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public async Task<bool> ItemExists(int id)
        {
            return await dbContext.Set<Item>().AnyAsync(p => p.Id == id);
        }
    }
}
