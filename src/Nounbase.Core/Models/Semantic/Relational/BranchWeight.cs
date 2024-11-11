using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Semantic.Relational
{
    public class BranchWeight
    {
        public BranchWeight() { }

        public BranchWeight(string foreignKeyName, int weight)
        {
            ArgumentNullException.ThrowIfNull(foreignKeyName, nameof(foreignKeyName));

            ForeignKeyName = foreignKeyName;
            Weight = weight;
        }

        [JsonProperty("foreign_key_name")]
        [JsonPropertyName("foreign_key_name")]
        public string? ForeignKeyName { get; set; }

        [JsonProperty("weight")]
        [JsonPropertyName("weight")]
        public int Weight { get; set; }
    }
}
