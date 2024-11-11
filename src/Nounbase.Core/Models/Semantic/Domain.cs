using Newtonsoft.Json;
using Nounbase.Core.Models.Semantic.Narrative;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Semantic
{
    public class Domain
    {
        [JsonProperty("narrative")]
        [JsonPropertyName("narrative")]
        public DomainNarrative? Narrative { get; set; }

        [JsonProperty("nouns")]
        [JsonPropertyName("nouns")]
        public List<Noun> Nouns { get; set; } = new List<Noun>();
    }
}
