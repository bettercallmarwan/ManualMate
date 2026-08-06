using ManualMate.Application.Responses;

namespace ManualMate.Application.Interfaces.Services
{
    public interface IFileProcessingService
    {
        Task<Result<bool>> ProcessFileAsync(Guid itemId);
        Task<Result<bool>> DeleteFileEmbeddingsAsync(Guid itemId);
    }
}