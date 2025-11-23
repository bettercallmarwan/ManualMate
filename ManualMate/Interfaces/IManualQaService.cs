using ManualMate.Controllers.Responses;

namespace ManualMate.Interfaces
{
    public interface IManualQaService
    {
        Task<Result<string>> GetAnswerAsync(int productId, string question);
    }
}
