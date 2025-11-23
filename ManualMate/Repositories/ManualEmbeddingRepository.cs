using ManualMate.Interfaces;
using ManualMate.Models;
using ManualMate.Presistence;

namespace ManualMate.Repositories
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

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        public void Update(ManualEmbedding entity)
        {
            throw new NotImplementedException();
        }
    }
}
