namespace ManualMate.Interfaces
{
    public interface IGenericRepository<T, Key>
    {
        Task<T?> GetAsync(Key id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task SaveChangesAsync();
    }
}
