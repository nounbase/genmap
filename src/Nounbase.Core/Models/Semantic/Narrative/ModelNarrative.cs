using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Semantic.Narrative
{
    public class ModelNarrative
    {
        [JsonProperty("domain_narrative")]
        [JsonPropertyName("domain_narrative")]
        public DomainNarrative? DomainNarrative { get; set; }

        [JsonProperty("noun_narratives")]
        [JsonPropertyName("noun_narratives")]
        public IDictionary<string, NounNarrative> NounNarratives { get; set; } = new Dictionary<string, NounNarrative>();
    }
}
