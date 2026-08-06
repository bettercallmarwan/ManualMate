using System.Text;
using System.Text.RegularExpressions;
using ManualMate.Application.Interfaces.Services;
using ManualMate.Application.Responses;

namespace ManualMate.Infrastructure.Services
{
    public class TextChunker : ITextChunker
    {
        public Result<List<string>> ChunkText(string text, int maxChars = 500)
        {
            try
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

                return Result<List<string>>.Ok(chunks);
            }
            catch (Exception ex)
            {
                return Result<List<string>>.Fail("Error chunking text: " + ex.Message);            }
        }
    }
}
