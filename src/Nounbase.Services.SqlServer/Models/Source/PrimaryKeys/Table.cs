using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Services.SqlServer.Models.Source.PrimaryKeys
{
    public class Table
    {
        [JsonProperty("table_name")]
        [JsonPropertyName("table_name")]
        public string? Name { get; set; }

        [JsonProperty("columns")]
        [JsonPropertyName("columns")]
        public List<Column> PrimaryKeyColumns { get; set; } = new List<Column>();
    }
}
