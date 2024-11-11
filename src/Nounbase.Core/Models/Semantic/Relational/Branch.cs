using Newtonsoft.Json;
using Nounbase.Core.Interfaces.Models;
using Nounbase.Core.Models.Schema;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Semantic.Relational
{
    public class Branch : INoun
    {
        public Branch() { }

        public Branch(string tableName, ForeignKey foreignKey)
        {
            ArgumentNullException.ThrowIfNull(tableName, nameof(tableName));
            ArgumentNullException.ThrowIfNull(foreignKey, nameof(foreignKey));

            TableName = tableName;
            ForeignKey = foreignKey;
        }

        [JsonProperty("table_name")]
        [JsonPropertyName("table_name")]
        public string? TableName { get; set; }

        [JsonProperty("foreign_key")]
        [JsonPropertyName("foreign_key")]
        public ForeignKey? ForeignKey { get; set; }

        [JsonProperty("branches")]
        [JsonPropertyName("branches")]
        public List<Branch> Branches { get; set; } = new List<Branch>();

        [JsonProperty("dimensions")]
        [JsonPropertyName("dimensions")]
        public List<Dimension> Dimensions { get; set; } = new List<Dimension>();

        [JsonProperty("properties")]
        [JsonPropertyName("properties")]
        public List<Property> Properties { get; set; } = new List<Property>();

        [JsonProperty("chronological_sort_property")]
        [JsonPropertyName("chronological_sort_property")]
        public string? ChronologicalSortPropertyName { get; set; }

        [JsonProperty("is_chronological")]
        [JsonPropertyName("is_chronological")]
        public bool IsChronological { get; set; }

        [JsonProperty("groupable_column_sets")]
        [JsonPropertyName("groupable_column_sets")]
        public string[][]? GroupableColumnSets { get; set; }
    }
}
