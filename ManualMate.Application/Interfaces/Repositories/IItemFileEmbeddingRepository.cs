using ManualMate.Domain.Interfaces;
using ManualMate.Domain.Models;

namespace ManualMate.Application.Interfaces.Repositories
{
    public interface IItemFileEmbeddingRepository : IGenericRepository<FileEmbedding, int>
    {
        Task RemoveForItem(int itemId);
        Task<List<string>> GetItemTopChunks(int itemId, Pgvector.Vector questionVector);
    }
}
