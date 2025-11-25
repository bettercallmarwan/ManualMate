using ManualMate.API.Controllers.Responses;

namespace ManualMate.Application.Interfaces
{
    public interface IEmbeddingService
    {
        Task<Result<double[]>> GetEmbeddingAsync(string text);
        Result<double> CosineSimilarity(double[] embedding1, double[] embedding2);
    }
}
