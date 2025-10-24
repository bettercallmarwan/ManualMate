namespace ManualMate.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ManualPath { get; set; }
        public List<ManualEmbedding>? ManualEmbeddings { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
