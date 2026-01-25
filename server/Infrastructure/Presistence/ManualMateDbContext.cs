using ManualMate.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ManualMate.Infrastructure.Presistence
{
    public class ManualMateDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ManualEmbedding> ManualEmbeddings { get; set; }
        public ManualMateDbContext(DbContextOptions<ManualMateDbContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.Entity<ManualEmbedding>()
                .Property(e => e.Embedding) // Replace 'Embedding' with your actual property name
                .HasColumnType("vector(384)"); // 384 is the dimension size for bge-small-en-v1.5

            modelBuilder.Entity<ManualEmbedding>()
            .HasOne(e => e.Product)
            .WithMany(p => p.ManualEmbeddings)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
