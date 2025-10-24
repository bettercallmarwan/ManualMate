using System.Text;
using System.Text.RegularExpressions;

namespace ManualMate.Services
{
    public static class TextChunker
    {
        public static List<string> ChunkText(string text, int maxChars = 500)
        {
            var chunks = new List<string>();
            var sentences = Regex.Split(text, @"(?<=[.!?])\s+");

            var currentChunk = new StringBuilder();

            foreach (var sentence in sentences)
            {
                if (currentChunk.Length + sentence.Length > maxChars && currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                }

                currentChunk.Append(sentence).Append(' ');
            }

            if (currentChunk.Length > 0)
                chunks.Add(currentChunk.ToString().Trim());

            return chunks;
        }
    }
}
