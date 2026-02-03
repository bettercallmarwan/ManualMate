using ManualMate.API.Controllers.Responses;

namespace ManualMate.Application.Interfaces.Services
{
    public interface IFileProcessingService
    {
        Task<Result<bool>> ProcessFileAsync(int itemId);
        Task<Result<bool>> DeleteFileEmbeddingsAsync(int itemId);
    }
}