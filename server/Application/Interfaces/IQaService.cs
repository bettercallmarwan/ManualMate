using ManualMate.API.Controllers.Responses;

namespace ManualMate.Application.Interfaces
{
    public interface IQaService
    {
        Task<Result<string>> GetAnswerAsync(int itemId, string question);
    }
}
