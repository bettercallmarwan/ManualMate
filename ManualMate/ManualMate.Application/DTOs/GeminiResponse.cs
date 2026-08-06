using Newtonsoft.Json;

namespace ManualMate.Application.DTOs
{
    [Serializable]
    public record GeminiResponse
    {
        [JsonProperty("text")]
        public string text { get; set; }
    }
}
