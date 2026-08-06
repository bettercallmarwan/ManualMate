using ManualMate.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ManualMate.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<Item> Items { get; }
        public DbSet<FileEmbedding> FileEmbeddings { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}