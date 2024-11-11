using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Schema
{
    public class DbRecordSetContext
    {
        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonProperty("record_set")]
        [JsonPropertyName("record_set")]
        public DbRecordSet? RecordSet { get; set; }

        [JsonProperty("query")]
        [JsonPropertyName("query")]
        public Query? Query { get; set; }
    }
}
