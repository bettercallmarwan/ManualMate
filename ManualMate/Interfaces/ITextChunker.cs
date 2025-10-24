namespace ManualMate.Interfaces
{
    public interface ITextChunker
    {
        List<string> ChunkText(string text, int maxChars = 2000);
    }
}
