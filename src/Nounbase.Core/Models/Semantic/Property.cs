using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Semantic
{
    public class Property
    {
        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonProperty("type")]
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonProperty("alias")]
        [JsonPropertyName("alias")]
        public string? Alias { get; set; }

        [JsonProperty("foreign_key_name")]
        [JsonPropertyName("foreign_key_name")]
        public string? ForeignKeyName { get; set; }

        [JsonProperty("source_alias")]
        [JsonPropertyName("source_alias")]
        public string? SourceAlias { get; set; }

        [JsonProperty("source_table")]
        [JsonPropertyName("source_table")]
        public string? SourceTable { get; set; }

        [JsonProperty("lineage")]
        [JsonPropertyName("lineage")]
        public List<string> Lineage { get; set; } = new List<string>();

        [JsonProperty("title")]
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonProperty("description")]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonProperty("is_calculable")]
        [JsonPropertyName("is_calculable")]
        public bool IsCalculable { get; set; }

        [JsonProperty("is_critical")]
        [JsonPropertyName("is_critical")]
        public bool IsCritical { get; set; }

        [JsonProperty("is_unique")]
        [JsonPropertyName("is_unique")]
        public bool IsUnique { get; set; }

        [JsonProperty("is_label")]
        [JsonPropertyName("is_label")]
        public bool IsLabel { get; set; }

        [JsonProperty("is_mutable")]
        [JsonPropertyName("is_mutable")]
        public bool IsMutable { get; set; }
    }
}
