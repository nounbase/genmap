using Newtonsoft.Json;
using Nounbase.Core.Models.Schema;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Semantic
{
    public class NarrationContext
    {
        [JsonProperty("map")]
        [JsonPropertyName("map")]
        public SemanticMap? Map { get; set; }

        [JsonProperty("samples")]
        [JsonPropertyName("samples")]
        public IDictionary<string, DbRecordSet> Samples { get; set; } = new Dictionary<string, DbRecordSet>();
    }
}
