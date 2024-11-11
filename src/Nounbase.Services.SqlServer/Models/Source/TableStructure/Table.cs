using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Services.SqlServer.Models.Source.TableStructure
{
    public class Table
    {
        [JsonProperty("table_id")]
        [JsonPropertyName("table_id")]
        public int Id { get; set; }

        [JsonProperty("table_name")]
        [JsonPropertyName("table_name")]
        public string? Name { get; set; }

        [JsonProperty("columns")]
        [JsonPropertyName("columns")]
        public List<Column> Columns { get; set; } = new List<Column>();

        public Core.Models.Schema.Table ToCoreModel() =>
            new Core.Models.Schema.Table
            {
                Id = Id.ToString(),
                Name = Name,
                Columns = Columns
                    .Select(c => c.ToCoreModel())
                    .ToList()
            };
    }
}
