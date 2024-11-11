using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Semantic
{
    public class Dimension
    {
        public Dimension() { }

        public Dimension(string alias, string foreignKeyName, string nounName)
        {
            ArgumentNullException.ThrowIfNull(alias, nameof(alias));
            ArgumentNullException.ThrowIfNull(foreignKeyName, nameof(foreignKeyName));
            ArgumentNullException.ThrowIfNull(nounName, nameof(nounName));

            Alias = alias;
            ForeignKeyName = foreignKeyName;
            NounName = nounName;
        }

        [JsonProperty("alias")]
        [JsonPropertyName("alias")]
        public string? Alias { get; set; }

        [JsonProperty("foreign_key_name")]
        [JsonPropertyName("foreign_key_name")]
        public string? ForeignKeyName { get; set; }

        [JsonProperty("noun_name")]
        [JsonPropertyName("noun_name")]
        public string? NounName { get; set; }

        [JsonProperty("mermaid")]
        [JsonPropertyName("mermaid")]
        public string? MermaidDefinition { get; set; }

        [JsonProperty("lineage")]
        [JsonPropertyName("lineage")]
        public List<string> Lineage { get; set; } = new List<string>();

        [JsonProperty("dimensions")]
        [JsonPropertyName("dimensions")]
        public List<Dimension> Dimensions { get; set; } = new List<Dimension>();
    }
}
