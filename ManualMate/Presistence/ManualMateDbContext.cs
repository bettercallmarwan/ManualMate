using ManualMate.Models;
using Microsoft.EntityFrameworkCore;

namespace ManualMate.Presistence
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

            modelBuilder.Entity<ManualEmbedding>()
            .HasOne(e => e.Product)
            .WithMany(p => p.ManualEmbeddings)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
