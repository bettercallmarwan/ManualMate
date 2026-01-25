namespace ManualMate.Application.DTOs
{
    public class ManualEmbeddingDto
    {
        public string TextChunk { get; set; }
        public int ChunkIndex { get; set; }
        public Pgvector.Vector Embedding { get; set; }
    }
}
