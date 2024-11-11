using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Schema
{
    public class Table
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonProperty("columns")]
        [JsonPropertyName("columns")]
        public List<Column> Columns { get; set; } = new List<Column>();

        [JsonProperty("primary_keys")]
        [JsonPropertyName("primary_keys")]
        public List<string> PrimaryKeys { get; set; } = new List<string>();
    }
}
