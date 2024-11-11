using Newtonsoft.Json;
using Nounbase.Core.Interfaces.Models;
using Nounbase.Core.Models.Schema;
using Nounbase.Core.Models.Semantic.Narrative;
using Nounbase.Core.Models.Semantic.Relational;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Semantic
{
    public class Noun
    {
        public Noun() { }

        public Noun(Table table, string schemaName)
        {
            ArgumentNullException.ThrowIfNull(table, nameof(table));
            ArgumentNullException.ThrowIfNull(schemaName, nameof(schemaName));

            Name = table.Name;
            TableName = table.Name;
            SchemaName = schemaName;
        }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonProperty("singular_name")]
        [JsonPropertyName("singular_name")]
        public string? SingularName { get; set; }

        [JsonProperty("plural_name")]
        [JsonPropertyName("plural_name")]
        public string? PluralName { get; set; }

        [JsonProperty("kind")]
        [JsonPropertyName("kind")]
        public string? Kind { get; set; } // person, place, or thing

        [JsonProperty("description")]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonProperty("narrative")]
        [JsonPropertyName("narrative")]
        public NounNarrative? Narrative { get; set; }

        [JsonProperty("table_name")]
        [JsonPropertyName("table_name")]
        public string? TableName { get; set; }

        [JsonProperty("schema_name")]
        [JsonPropertyName("schema_name")]
        public string? SchemaName { get; set; }

        [JsonProperty("chronological_sort_property")]
        [JsonPropertyName("chronological_sort_property")]
        public string? ChronologicalSortPropertyName { get; set; }

        [JsonProperty("is_chronological")]
        [JsonPropertyName("is_chronological")]
        public bool IsChronological { get; set; }

        [JsonProperty("is_excluded")]
        [JsonPropertyName("is_excluded")]
        public bool IsExcluded { get; set; }

        [JsonProperty("groupable_column_sets")]
        [JsonPropertyName("groupable_column_sets")]
        public string[][]? GroupableColumnSets { get; set; }

        [JsonProperty("dimensions")]
        [JsonPropertyName("dimensions")]
        public List<Dimension> Dimensions { get; set; } = new List<Dimension>();

        [JsonProperty("properties")]
        [JsonPropertyName("properties")]
        public List<Property> Properties { get; set; } = new List<Property>();

        [JsonProperty("root")]
        [JsonPropertyName("root")]
        public Root? Root { get; set; }
    }
}
