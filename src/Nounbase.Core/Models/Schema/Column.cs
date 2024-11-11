using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Schema
{
    public class Column
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonProperty("type")]
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
