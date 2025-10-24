namespace ManualMate.Interfaces
{
    public interface IManualQaService
    {
        Task<string> GetAnswerAsync(int productId, string question);
    }
}
