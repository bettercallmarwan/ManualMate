namespace ManualMate.Domain.Models
{
    public class FileEmbedding
    {
        public int Id { get; set; }
        public Guid ItemId { get; set; }
        public Item item { get; set; }

        public string TextChunk { get; set; }
        public int ChunkIndex { get; set; }

        public Pgvector.Vector Embedding { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
