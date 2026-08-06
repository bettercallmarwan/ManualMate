using ManualMate.Application.Responses;

namespace ManualMate.Application.Interfaces.Services
{
    public interface ILlmService
    {
        Task<Result<string>> GenerateAnswerAsync(string context, string question);
    }
}
