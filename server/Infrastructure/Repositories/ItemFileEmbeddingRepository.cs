using ManualMate.Domain.Interfaces;
using ManualMate.Domain.Models;
using ManualMate.Infrastructure.Presistence;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace ManualMate.Infrastructure.Repositories
{
    public class ItemFileEmbeddingRepository(ManualMateDbContext dbContext, IConfiguration configuration) : IGenericRepository<FileEmbedding, int>
    {
        private int top_k = int.Parse(configuration.GetSection("RAG")["top_k"]!);

        public async Task AddAsync(FileEmbedding entity)
        {
            await dbContext.Set<FileEmbedding>().AddAsync(entity);
        }

        public async Task RemoveForItem(int itemId)
        {
            var embeddings = await dbContext.Set<FileEmbedding>()
                .Where(e => e.ItemId == itemId)
                .ToListAsync();
            
            dbContext.Set<FileEmbedding>().RemoveRange(embeddings);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<string>> GetItemTopChunks(int itemId, Pgvector.Vector questionVector)
        {
            return await dbContext.Set<FileEmbedding>()
                .Where(e => e.ItemId == itemId)
                .OrderBy(e => e.Embedding.CosineDistance(questionVector))
                .Take(top_k)
                .Select(e => e.TextChunk)
                .ToListAsync();
        } 

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        #region Not implemented
        public Task<IEnumerable<FileEmbedding>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<FileEmbedding?> GetAsync(int id)
        {
            throw new NotImplementedException();
        }

        public void Remove(FileEmbedding entity)
        {
            throw new NotImplementedException();
        }
        public void Update(FileEmbedding entity)
        {
            throw new NotImplementedException();
        } 
        #endregion
    }
}
