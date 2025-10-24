namespace ManualMate.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string text);
        double CosineSimilarity(float[] embedding1, float[] embedding2);
    }
}
