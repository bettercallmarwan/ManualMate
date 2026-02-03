using ManualMate.Application.Interfaces;
using ManualMate.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ManualMate.Infrastructure.Presistence
{
    public class ManualMateDbContext : DbContext, IApplicationDbContext
    {
        public ManualMateDbContext(DbContextOptions<ManualMateDbContext> options): 
            base(options) { }

        public DbSet<Item> Items => Set<Item>();
        public DbSet<FileEmbedding> FileEmbeddings => Set<FileEmbedding>();

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await base.SaveChangesAsync(ct);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.Entity<FileEmbedding>()
                .Property(e => e.Embedding) 
                .HasColumnType("vector(384)"); 

            modelBuilder.Entity<FileEmbedding>()
            .HasOne(e => e.item)
            .WithMany(p => p.FileEmbeddings)
            .HasForeignKey(e => e.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
