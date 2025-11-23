namespace ManualMate.Application.Interfaces
{
    public interface IEmbeddingService
    {
        Task<double[]> GetEmbeddingAsync(string text);
        double CosineSimilarity(double[] embedding1, double[] embedding2);
    }
}
