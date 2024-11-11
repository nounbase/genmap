using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Services.SqlServer.Models.Source.ForeignKeys
{
    public class ForeignKey
    {
        [JsonProperty("constraint_id")]
        [JsonPropertyName("constraint_id")]
        public int Id { get; set; }

        [JsonProperty("constraint_name")]
        [JsonPropertyName("constraint_name")]
        public string? Name { get; set; }

        [JsonProperty("fk_table_name")]
        [JsonPropertyName("fk_table_name")]
        public string? ForeignKeyTableName { get; set; }

        [JsonProperty("fk_column_name")]
        [JsonPropertyName("fk_column_name")]
        public string? ForeignKeyColumnName { get; set; }

        [JsonProperty("pk_table_name")]
        [JsonPropertyName("pk_table_name")]
        public string? PrimaryKeyTableName { get; set; }

        [JsonProperty("pk_column_name")]
        [JsonPropertyName("pk_column_name")]
        public string? PrimaryKeyColumnName { get; set; }

        public Core.Models.Schema.ForeignKey ToCoreModel() =>
            new Core.Models.Schema.ForeignKey
            {
                Id = Id.ToString(),
                Name = Name,
                PrimaryKeyRef = new Core.Models.Schema.ColumnReference(PrimaryKeyTableName, PrimaryKeyColumnName),
                ForeignKeyRef = new Core.Models.Schema.ColumnReference(ForeignKeyTableName, ForeignKeyColumnName)
            };

        public override string ToString() =>
            Name == null ? Id.ToString() : $"{Id}: {Name}";
    }
}
