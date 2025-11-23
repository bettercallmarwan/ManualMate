using ManualMate.API.Controllers.Responses;

namespace ManualMate.Application.Interfaces
{
    public interface ILlmService
    {
        Task<Result<string>> GenerateAnswerAsync(string context, string question);
    }
}
