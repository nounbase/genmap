using Newtonsoft.Json;
using Nounbase.Core.Models.Semantic;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models
{
    public class SemanticMap
    {
        public SemanticMap() { }

        public SemanticMap(string modelName) =>
            ModelName = modelName ?? throw new ArgumentNullException(nameof(modelName));

        [JsonProperty("model_name")]
        [JsonPropertyName("model_name")]
        public string? ModelName { get; set; }

        [JsonProperty("domain")]
        [JsonPropertyName("domain")]
        public Domain? Domain { get; set; }

        [JsonProperty("schema")]
        [JsonPropertyName("schema")]
        public Schema.Schema? Schema { get; set; }
    }
}
