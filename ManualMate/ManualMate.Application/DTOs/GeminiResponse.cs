using Newtonsoft.Json;

namespace ManualMate.Application.DTOs
{
    [Serializable]
    public class GeminiResponse
    {
        [JsonProperty("text")]
        public string text { get; set; }
    }
}
