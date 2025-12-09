using ManualMate.API.Controllers.Responses;

namespace ManualMate.Application.Interfaces
{
    public interface IManualProcessingService
    {
        Task<Result<bool>> ProcessManualAsync(int productId);
    }
}
