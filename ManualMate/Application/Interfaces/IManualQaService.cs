using ManualMate.API.Controllers.Responses;

namespace ManualMate.Application.Interfaces
{
    public interface IManualQaService
    {
        Task<Result<string>> GetAnswerAsync(int productId, string question);
    }
}
