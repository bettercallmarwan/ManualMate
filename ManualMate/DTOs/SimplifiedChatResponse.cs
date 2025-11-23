using System.Text.Json.Serialization;

namespace ManualMate.DTOs
{
    public class SimplifiedChatResponse
    {
        [JsonPropertyName("choices")]
        public List<SimplifiedChoice> Choices { get; set; }
    }
    public class SimplifiedChoice
    {
        [JsonPropertyName("message")]
        public SimplifiedMessage Message { get; set; }
    }

    public class SimplifiedMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
