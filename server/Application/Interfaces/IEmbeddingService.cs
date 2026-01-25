using ManualMate.API.Controllers.Responses;

namespace ManualMate.Application.Interfaces
{
    public interface IEmbeddingService
    {
        Task<Result<Pgvector.Vector>> GetEmbeddingAsync(string text);
    }
}
