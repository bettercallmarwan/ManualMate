using ManualMate.API.Controllers.Responses;

namespace ManualMate.Application.Interfaces.Services
{
    public interface IQaService
    {
        Task<Result<string>> GetAnswerAsync(int itemId, string question);
    }
}
