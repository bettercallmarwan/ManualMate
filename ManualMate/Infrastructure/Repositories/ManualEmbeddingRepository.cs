using ManualMate.Domain.Interfaces;
using ManualMate.Domain.Models;
using ManualMate.Infrastructure.Presistence;

namespace ManualMate.Infrastructure.Repositories
{
    public class ManualEmbeddingRepository(ManualMateDbContext dbContext) : IGenericRepository<ManualEmbedding, int>
    {
        public async Task AddAsync(ManualEmbedding entity)
        {
            await dbContext.Set<ManualEmbedding>().AddAsync(entity);
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
