using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Services.SqlServer.Models.Source.PrimaryKeys
{
    public class Column
    {
        [JsonProperty("column_name")]
        [JsonPropertyName("column_name")]
        public string? Name { get; set; }
    }
}
