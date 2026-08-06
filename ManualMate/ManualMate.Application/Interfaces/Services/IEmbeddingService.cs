using ManualMate.Application.Responses;

namespace ManualMate.Application.Interfaces.Services
{
    public interface IEmbeddingService
    {
        Task<Result<Pgvector.Vector>> GetEmbeddingAsync(string text);
    }
}
