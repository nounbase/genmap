using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Schema
{
    public class DbRecordSet
    {
        [JsonProperty("column_names")]
        [JsonPropertyName("column_names")]
        public string[]? ColumnNames { get; set; }

        [JsonProperty("rows")]
        [JsonPropertyName("rows")]
        public string[][]? Rows { get; set; }

        public int RowCount => Rows?.Length ?? 0;
    }
}
