namespace ManualMate.Interfaces
{
    public interface IManualProcessingService
    {
        Task ProcessManualAsync(int productId);
    }
}
