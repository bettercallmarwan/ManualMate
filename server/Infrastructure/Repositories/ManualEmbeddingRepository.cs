using ManualMate.Domain.Interfaces;
using ManualMate.Domain.Models;
using ManualMate.Infrastructure.Presistence;
using Microsoft.EntityFrameworkCore;

namespace ManualMate.Infrastructure.Repositories
{
    public class ManualEmbeddingRepository(ManualMateDbContext dbContext) : IGenericRepository<ManualEmbedding, int>
    {
        public async Task AddAsync(ManualEmbedding entity)
        {
            await dbContext.Set<ManualEmbedding>().AddAsync(entity);
        }
        public async Task RemoveForProduct(int productId)
        {
            var embeddings = await dbContext.Set<ManualEmbedding>()
                .Where(e => e.ProductId == productId)
                .ToListAsync();
            
            dbContext.Set<ManualEmbedding>().RemoveRange(embeddings);
            await dbContext.SaveChangesAsync();
        }
        public Task<IEnumerable<ManualEmbedding>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ManualEmbedding?> GetAsync(int id)
        {
            throw new NotImplementedException();
        }

        public void Remove(ManualEmbedding entity)
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public void Update(ManualEmbedding entity)
        {
            throw new NotImplementedException();
        }
    }
}
