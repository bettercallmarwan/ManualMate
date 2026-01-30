using ManualMate.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ManualMate.Infrastructure.Presistence
{
    public class ManualMateDbContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
        public DbSet<FileEmbedding> FileEmbeddings { get; set; }
        public ManualMateDbContext(DbContextOptions<ManualMateDbContext> options)
            : base(options)
        {
            
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
