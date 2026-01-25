using Pgvector;
using System.Numerics;

namespace ManualMate.Domain.Models
{
    public class ManualEmbedding
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string TextChunk { get; set; }
        public int ChunkIndex { get; set; }

        public Pgvector.Vector Embedding { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
