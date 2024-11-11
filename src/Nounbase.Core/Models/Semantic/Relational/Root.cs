using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Semantic.Relational
{
    public class Root
    {
        [JsonProperty("branches")]
        [JsonPropertyName("branches")]
        public List<Branch> Branches { get; set; } = new List<Branch>();
    }
}
