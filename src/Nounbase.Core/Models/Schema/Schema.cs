using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Schema
{
    public class Schema
    {
        public Schema() { }

        public Schema(string databaseType, string schemaName)
        {
            ArgumentNullException.ThrowIfNull(databaseType, nameof(databaseType));
            ArgumentNullException.ThrowIfNull(schemaName, nameof(schemaName));

            DatabaseType = databaseType;
            Name = schemaName;
        }

        [JsonProperty("db_type")]
        [JsonPropertyName("db_type")]
        public string? DatabaseType { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonProperty("tables")]
        [JsonPropertyName("tables")]
        public List<Table> Tables { get; set; } = new List<Table>();

        [JsonProperty("foreign_keys")]
        [JsonPropertyName("foreign_keys")]
        public List<ForeignKey> ForeignKeys { get; set; } = new List<ForeignKey>();
    }
}
