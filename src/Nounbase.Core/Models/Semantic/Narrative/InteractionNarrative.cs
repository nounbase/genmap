using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Semantic.Narrative
{
    public class InteractionNarrative
    {
        [JsonProperty("people")]
        [JsonPropertyName("people")]
        public string? OtherPeople { get; set; }

        [JsonProperty("places")]
        [JsonPropertyName("places")]
        public string? OtherPlaces { get; set; }

        [JsonProperty("things")]
        [JsonPropertyName("things")]
        public string? OtherThings { get; set; }
    }
}
