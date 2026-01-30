namespace ManualMate.Domain.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? FilePath { get; set; }
        public List<FileEmbedding>? FileEmbeddings { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
