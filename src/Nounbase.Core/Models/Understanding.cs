using Newtonsoft.Json;
using Nounbase.Core.Models.Schema;
using Nounbase.Core.Models.Semantic;
using Nounbase.Core.Models.Semantic.Narrative;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models
{
    public class Understanding
    {
        public Understanding() { }

        public Understanding(string modelName) =>
            ModelName = modelName ?? throw new ArgumentNullException(nameof(modelName));

        [JsonProperty("model_name")]
        [JsonPropertyName("model_name")]
        public string? ModelName { get; set; }

        [JsonProperty("schema")]
        [JsonPropertyName("schema")]
        public Schema.Schema? Schema { get; set; }

        [JsonProperty("table_samples")]
        [JsonPropertyName("table_samples")]
        public IDictionary<string, DbRecordSet> TableSamples { get; set; } = new Dictionary<string, DbRecordSet>();

        [JsonProperty("noun_samples")]
        [JsonPropertyName("noun_samples")]
        public IDictionary<string, DbRecordSet> NounSamples { get; set; } = new Dictionary<string, DbRecordSet>();

        [JsonProperty("nouns")]
        [JsonPropertyName("nouns")]
        public IList<Noun> Nouns { get; set; } = new List<Noun>();

        [JsonProperty("narrative")]
        [JsonPropertyName("narrative")]
        public ModelNarrative? Narrative { get; set; }
    }
}
