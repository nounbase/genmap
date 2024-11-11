using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Schema
{
    public class ForeignKey
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonProperty("primary")]
        [JsonPropertyName("primary")]
        public ColumnReference? PrimaryKeyRef { get; set; }

        [JsonProperty("foreign")]
        [JsonPropertyName("foreign")]
        public ColumnReference? ForeignKeyRef { get; set; }
    }
}
