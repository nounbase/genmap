using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Services.SqlServer.Models.Source.TableStructure
{
    public class Column
    {
        [JsonProperty("column_id")]
        [JsonPropertyName("column_id")]
        public int Id { get; set; }

        [JsonProperty("column_name")]
        [JsonPropertyName("column_name")]
        public string? Name { get; set; }

        [JsonProperty("system_types")]
        [JsonPropertyName("system_types")]
        public List<DataType> DataTypes { get; set; } = new List<DataType>();

        public Core.Models.Schema.Column ToCoreModel() =>
            new Core.Models.Schema.Column
            {
                Id = Id.ToString(),
                Name = Name,
                Type = DataTypes.FirstOrDefault()?.ToCoreDataType() ?? default
            };
    }
}
