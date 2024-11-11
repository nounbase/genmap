using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Schema
{
    public class ColumnReference
    {
        public ColumnReference() { }

        public ColumnReference(string? tableName, string? columnName)
        {
            TableName = tableName;
            ColumnName = columnName;
        }

        [JsonProperty("table_name")]
        [JsonPropertyName("table_name")]
        public string? TableName { get; set; }

        [JsonProperty("column_name")]
        [JsonPropertyName("column_name")]
        public string? ColumnName { get; set; }
    }
}
