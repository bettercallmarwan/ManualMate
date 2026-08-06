using ManualMate.Application.Responses;

namespace ManualMate.Application.Interfaces.Services
{
    public interface IQaService
    {
        Task<Result<string>> GetAnswerAsync(Guid itemId, string question);
    }
}
